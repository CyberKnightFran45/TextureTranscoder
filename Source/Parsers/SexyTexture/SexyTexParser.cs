using System;
using System.IO;
using BlossomLib.Modules.Compression;
using SkiaSharp;

namespace TextureTranscoder.Parsers.SexyTexture
{
/// <summary> Initializes Parsing Tasks for SexyTex Files. </summary>

public static class SexyTexParser
{
/// <summary> The Header of a SexyTex File. </summary>

private const string HEADER = "SEXYTEX";

/// <summary> The Identifier of a SexyTex File. </summary>

private const byte FLAGS = 0x00;

/// <summary> The Version of a SexyTex File. </summary>

private const uint VERSION = 0;

// Compress image

private static void CompressStream(Stream input, Stream output)
{
input.Seek(0, SeekOrigin.Begin);

string fileSize = SizeT.FormatSize(input.Length);
TraceLogger.WriteActionStart($"Compressing data... ({fileSize})");

ZLibCompressor.CompressStream(input, output, default);
input.Dispose();

long currentPos = output.Position;
var sizeCompressed = (int)(output.Length - 48); // Don't include header bytes

output.Seek(32, SeekOrigin.Begin);
output.WriteInt32(sizeCompressed);

output.Seek(currentPos, SeekOrigin.Begin);

TraceLogger.WriteActionEnd();
}

// Get TEX Stream

public static int Encode(SKBitmap source, Stream target, SexyTexFormat format)
{
bool useZlib = (int)format > 7;
var flags = useZlib ? CompressionFlags.ZLib : CompressionFlags.NoCompression;

SexyTexInfo fileInfo = new(source.Width, source.Height, format, flags);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Writing header...");

target.WriteString(HEADER);
target.WriteByte(FLAGS);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Encode Image:");
TraceLogger.WriteLine();

Stream rawStream = useZlib ? new ChunkedMemoryStream() : target;

int stride = format switch
{
SexyTexFormat.ARGB4444 => ARGB4444.Write(rawStream, source),
SexyTexFormat.ARGB1555 => ARGB1555.Write(rawStream, source),
SexyTexFormat.RGB565 => RGB565.Write(rawStream, source),
SexyTexFormat.ABGR8888 => ABGR8888.Write(rawStream, source),
SexyTexFormat.RGBA4444 => RGBA4444.Write(rawStream, source),
SexyTexFormat.RGBA5551 => RGBA5551.Write(rawStream, source),
_ => ARGB8888.Write(rawStream, source)
};

if(useZlib)
CompressStream(rawStream, target);

return stride;
}

/** <summary> Encodes an Image as a SexyTexture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded TEX File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, SexyTexFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("SexyTex Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".tex");	
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

using var srcImage = SKPlugin.FromFile(inputPath);
using var dstFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

int stride = Encode(srcImage, dstFile, format);
pxWritten = stride * srcImage.Height;
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("SexyTex Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
}

// Decompress image

private static void DecompressStream(Stream input, Stream output, int bytesCompressed)
{
string fileSize = SizeT.FormatSize(bytesCompressed);

TraceLogger.WriteActionStart($"Decompressing data... ({fileSize})");

ZLibCompressor.DecompressStream(input, output, bytesCompressed);
output.Seek(0, SeekOrigin.Begin);

TraceLogger.WriteActionEnd();
}

// Get Plain Image

public static SKBitmap Decode(Stream source)
{
TraceLogger.WriteLine("Step #1 - Read Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reading header...");

using var hOwner = source.ReadString(HEADER.Length);
var inputHeader = hOwner.AsSpan();

if(!inputHeader.SequenceEqual(HEADER) )
{
TraceLogger.WriteError($"Invalid header: \"{inputHeader}\", expected: \"{HEADER}\"");

return null;
}

byte inputFlags = source.ReadUInt8();

if(inputFlags != FLAGS)
{
const string ERROR_INVALID_FLAGS = "Invalid SexyTex identifier: {0:X2}, expected: {1:X2}";

TraceLogger.WriteError(string.Format(ERROR_INVALID_FLAGS, inputFlags, FLAGS) );
return null;
}

var fileInfo = SexyTexInfo.ReadBin(source);
var fileVer = fileInfo.Version;

if(fileVer != VERSION)
TraceLogger.WriteWarn($"Unknown version: V{fileVer} - Expected: V{VERSION}");

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Decode Image:");
TraceLogger.WriteLine();

var format = fileInfo.Format;

if(!Enum.IsDefined(format) )
{
TraceLogger.WriteError($"Unknown format: {format}, cannot decode this File.");

return null;
}

bool useZlib = fileInfo.CompressionType == CompressionFlags.ZLib;
Stream rawStream = useZlib ? new ChunkedMemoryStream() : source;

if(useZlib)
DecompressStream(source, rawStream, fileInfo.SizeCompressed);

int width = fileInfo.Width;
int height = fileInfo.Height;

return format switch
{
SexyTexFormat.ARGB4444 => ARGB4444.Read(rawStream, width, height),
SexyTexFormat.ARGB1555 => ARGB1555.Read(rawStream, width, height),
SexyTexFormat.RGB565 => RGB565.Read(rawStream, width, height),
SexyTexFormat.ABGR8888 => ABGR8888.Read(rawStream, width, height),
SexyTexFormat.RGBA4444 => RGBA4444.Read(rawStream, width, height),
SexyTexFormat.RGBA5551 => RGBA5551.Read(rawStream, width, height),
_ => ARGB8888.Read(rawStream, width, height)
};

}

/** <summary> Decodes a SexyTexture as an Image. </summary>

<param name = "inputPath"> The Path where the SexyTex to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("SexyTex Decoding Started");

int width, height = width = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".png");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");
using var srcFile = FileManager.OpenRead(inputPath);

TraceLogger.WriteActionEnd();

using var image = Decode(srcFile);

if(image is null)
return;

width = image.Width;
height = image.Height;

TraceLogger.WriteActionStart("Saving image...");

image.Save(outputPath);
image.Dispose();

TraceLogger.WriteActionEnd();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Decode file");
}

TraceLogger.WriteLine("SexyTex Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}