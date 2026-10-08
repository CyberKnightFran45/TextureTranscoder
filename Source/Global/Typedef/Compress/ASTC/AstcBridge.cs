using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SkiaSharp;

/// <summary> Bridge to ARM astcenc native library </summary>

public static unsafe class AstcBridge
{
#region =======================  NATIVE BINDINGS  =======================

private const string LIB_NAME = "astcenc";

private static bool _libLogged = false;

// Dynamic bind

static AstcBridge()
{
NativeLibrary.SetDllImportResolver(typeof(AstcBridge).Assembly, ResolveAstcLibrary);

AppDomain.CurrentDomain.ProcessExit += (_, _) => FreeContexts();
}

// Resolve lib handle

private static IntPtr ResolveAstcLibrary(string name, Assembly assembly, DllImportSearchPath? path)
{

if(name != LIB_NAME)
return IntPtr.Zero;

bool success = false;
IntPtr handle = IntPtr.Zero;

var libVariants = GetLibraryVariants();
string libName = LIB_NAME;

foreach(string variant in libVariants)
{
success = NativeLibrary.TryLoad(variant, assembly, path, out handle);
libName = variant;

if(success)
break;

}

if(success)
{

if(!_libLogged)
{
TraceLogger.WriteInfo($"{libName} lib: loaded");

_libLogged = true;
}

return handle;
}

TraceLogger.WriteError("Unable to load astcenc lib");

return IntPtr.Zero;
}

// Get library variants (Windows only, by the moment)

private static string[] GetLibraryVariants()
{

return RuntimeInformation.ProcessArchitecture switch
{	
Architecture.X64 => [ "astcenc-avx2", "astcenc-sse4.1", "astcenc-sse2" ],
Architecture.Arm64 => [ "astcenc-neon" ],
_ => [ "astcenc-sse2" ]
};

}

#endregion


#region =======================  TYPEDEFS/CONSTANTS  =======================

// Config size

private const int CONFIG_SIZE = 1024;

// Flags

public const uint FLAG_NONE = 0;

public const uint FLAG_DECOMPRESS_ONLY = 0x8;

// Block Z Dimension (2D images only)

private const uint BLOCK_Z = 1;

// ASTC Swizzle (SKiaSharp always use BGRA)

private static readonly AstcSwizzle SKIA_SWIZZLE = new(2, 1, 0, 3);

#endregion


#region =======================  ASTC CONTEXT  =======================

// Cache

private static readonly Dictionary<AstcContextKey, AstcContextEntry> _contexts = new();

// Lock

private static readonly Lock _contextsLock = new();

// Create new context

private static IntPtr CreateContext(AstcContextKey key)
{
IntPtr configPtr = Marshal.AllocHGlobal(CONFIG_SIZE);

try
{
NativeMemory.Clear( (void*)configPtr, CONFIG_SIZE);

uint flags = key.Decompress ? FLAG_DECOMPRESS_ONLY : FLAG_NONE;
float quality = key.Decompress ? AstcQuality.Fastest : key.Quality;

var blockW = (uint)key.BlockW;
var blockH = (uint)key.BlockH;

var initErr = NativeConfigInit(key.Profile, blockW, blockH, BLOCK_Z, quality, flags, configPtr);

ThrowIfError(initErr, "config_init");

var allocErr = NativeContextAlloc(configPtr, (uint)key.Threads, out var ctx, IntPtr.Zero);

ThrowIfError(allocErr, "context_alloc");

return ctx;
}

finally
{
Marshal.FreeHGlobal(configPtr);
}

}

// Get cached context

private static AstcContextEntry GetContext(AstcContextKey key)
{

lock(_contextsLock)
{

if(_contexts.TryGetValue(key, out var entry) )
return entry;

entry = new()
{
Context = CreateContext(key)
};

_contexts[key] = entry;

return entry;
}

}

// Free contexts

private static void FreeContexts()
{

lock(_contextsLock)
{

foreach(var pair in _contexts)
NativeContextFree(pair.Value.Context);

_contexts.Clear();
}

}

#endregion


#region =======================  P/Invoke  =======================

[DllImport(LIB_NAME, EntryPoint = "astcenc_config_init", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeConfigInit(AstcProfile profile,
                                                 uint blockX,
                                                 uint blockY,
                                                 uint blockZ,
                                                 float quality,
                                                 uint flags,
                                                 IntPtr config);

[DllImport(LIB_NAME, EntryPoint = "astcenc_context_alloc", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeContextAlloc(IntPtr config,
                                                   uint threadCount,
                                                   out IntPtr context,
                                                   IntPtr parent);

[DllImport(LIB_NAME, EntryPoint = "astcenc_compress_image", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeCompressImage(IntPtr context,
                                                    AstcImage* image,
                                                    AstcSwizzle* swizzle,
                                                    byte* dataOut,
                                                    nuint dataLen,
                                                    uint threadIndex);

[DllImport(LIB_NAME, EntryPoint = "astcenc_decompress_image", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeDecompressImage(IntPtr context,
                                                      byte* dataIn,
                                                      nuint dataLen,
                                                      AstcImage* imageOut,
                                                      AstcSwizzle* swizzle,
                                                      uint threadIndex);

[DllImport(LIB_NAME, EntryPoint = "astcenc_compress_reset", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeCompressReset(IntPtr context);

[DllImport(LIB_NAME, EntryPoint = "astcenc_decompress_reset", CallingConvention = CallingConvention.Cdecl)]

private static extern AstcError NativeDecompressReset(IntPtr context);

[DllImport(LIB_NAME, EntryPoint = "astcenc_context_free", CallingConvention = CallingConvention.Cdecl)]

private static extern void NativeContextFree(IntPtr context);

[DllImport(LIB_NAME, EntryPoint = "astcenc_get_error_string", CallingConvention = CallingConvention.Cdecl)]

private static extern byte* NativeGetErrorString(AstcError error);

#endregion


#region ======================= PARSER =======================

// Compute compressed block size

private static int CompressedSize(int width, int height, int blockW, int blockH)
{
int bX = (width  + blockW - 1) / blockW;
int bY = (height + blockH - 1) / blockH;

return bX * bY * 16;
}

// Compress ASTC (multi-thread)

private static void CompressWorker(IntPtr ctx,
                                   int width,
                                   int height,
                                   byte* pixels,
                                   AstcSwizzle swizzle,
                                   Span<byte> output,
                                   int outputSize,
                                   int threadIndex)
{
void** slices = stackalloc void*[1];
slices[0] = pixels;

AstcImage img = new(width, height, slices);

AstcSwizzle localSwizzle = swizzle;
var localOutSize = (nuint)outputSize;

AstcError err;

fixed(byte* outPtr = output)
{
NativeMemory.Clear(outPtr, localOutSize);

err = NativeCompressImage(ctx,
                          &img,
                          &localSwizzle,
                          outPtr,
                          localOutSize,
						  (uint)threadIndex);

}

if(err != AstcError.Success)
throw new AstcException($"compress thread {threadIndex}: {GetErrorString(err)}", err);

}

// Encode bitmap into ASTC blocks

public static NativeBuffer Encode(SKBitmap image,
                                  int blockW,
                                  int blockH,
                                  float quality = AstcQuality.Thorough,
                                  AstcProfile profile = AstcProfile.Ldr)
{
var pixels = (byte*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

int outputSize = CompressedSize(width, height, blockW, blockH);
NativeBuffer output = new(outputSize);

int threads = Environment.ProcessorCount;

AstcContextKey key = new(blockW, blockH, quality, profile, threads, false);
var entry = GetContext(key);

lock(entry.Locker)
{

RunThreads(threads, threadIndex =>
{

CompressWorker(entry.Context,
               width,
               height,
               pixels,
               SKIA_SWIZZLE,
               output.AsSpan(),
               outputSize,
               threadIndex);

}

);

var resetErr = NativeCompressReset(entry.Context);

ThrowIfError(resetErr, "compress_reset");
}

return output;
}

// Decompress ASTC (multi-thread)

private static void DecompressWorker(IntPtr ctx,
                                     int width,
                                     int height,
                                     byte* pixels,
                                     AstcSwizzle swizzle,
                                     ReadOnlySpan<byte> source,
                                     int bufferSize,
                                     int threadIndex)
{
void** slices = stackalloc void*[1];
slices[0] = pixels;

AstcImage img = new(width, height, slices);
AstcSwizzle localSwizzle = swizzle;

AstcError err;

fixed(byte* inputPtr = source)
{

err = NativeDecompressImage(ctx,
                            inputPtr,
                            (nuint)bufferSize,
                            &img,
                            &localSwizzle,
                            (uint)threadIndex);

}

if(err != AstcError.Success)
throw new AstcException($"decompress thread {threadIndex}: {GetErrorString(err)}", err);

}

// Decode ASTC blocks into bitmap

public static SKBitmap Decode(ReadOnlySpan<byte> source,
                              int width,
                              int height,
                              int blockW,
                              int blockH,
                              AstcProfile profile = AstcProfile.Ldr)
{
SKBitmap image = new(width, height);

int bufferSize = CompressedSize(width, height, blockW, blockH);

var pixels = (byte*)image.GetPixels().ToPointer();
int threads = Environment.ProcessorCount;

AstcContextKey key = new(blockW, blockH, AstcQuality.Fastest, profile, threads, true);
var entry = GetContext(key);

using NativeBuffer localSrc = new(source.Length);
localSrc.CopyFrom(source);

lock(entry.Locker)
{

RunThreads(threads, threadIndex =>
{

DecompressWorker(entry.Context,
                 width,
                 height,
                 pixels,
                 SKIA_SWIZZLE,
                 localSrc.GetView(),
                 bufferSize,
                 threadIndex);

}

);

var resetErr = NativeDecompressReset(entry.Context);

ThrowIfError(resetErr, "decompress_reset");
}

return image;
}

#endregion


#region ======================= UTILITIES =======================

// Get error string

private static string GetErrorString(AstcError err)
{
byte* msg = NativeGetErrorString(err);

return msg != null ? Marshal.PtrToStringAnsi( (IntPtr)msg ) ?? err.ToString() : err.ToString();
}

// Check cpp error state and throw Exception if no success

private static void ThrowIfError(AstcError err, string step)
{

if(err != AstcError.Success)
throw new AstcException($"astcenc {step} failed: {GetErrorString(err)}", err);

}

// Run threads simultaniously

private static void RunThreads(int threads, Action<int> workerAction)
{
Thread[] workers = new Thread[threads];
Exception threadEx = null;

for(int i = 0; i < threads; i++)
{
int threadIndex = i;

workers[i] = new( () =>
{

try
{
workerAction(threadIndex);
}

catch(Exception ex)
{
Interlocked.CompareExchange(ref threadEx, ex, null);
}

}

)

{ IsBackground = true };

}

foreach(Thread t in workers)
t.Start(); // Start threads

foreach(Thread t in workers)
t.Join(); // Merge work

if(threadEx != null)
throw threadEx;

}

#endregion
}