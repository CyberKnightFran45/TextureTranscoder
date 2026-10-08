using System;
using System.IO;
using BlossomLib.Modules.Compression;
using SkiaSharp;

namespace TextureTranscoder.Parsers.SexyTexture
{
/// <summary> Encodes images to SexyTex files and viceversa </summary>

public static class SexyTexParser
{
/// <summary> File identifier </summary>

private const ulong MAGIC = 0x5345585954455800;

/// <summary> File version </summary>

private const uint VERSION = 0;

// Get error msg

private static string FormatMsg(SexyTexFormat format) => $"Unsupported format: {format} (0x{(int)format:X8})";

// Encode raw img

private static NativeBuffer EncodeRaw(SKBitmap source, SexyTexFormat format) => format switch
{
SexyTexFormat.ARGB8888 => ARGB8888.Encode(source),
SexyTexFormat.ARGB4444 => ARGB4444.Encode(source),
SexyTexFormat.ARGB1555 => ARGB1555.Encode(source),
SexyTexFormat.RGB565 => RGB565.Encode(source),
SexyTexFormat.ABGR8888 => ABGR8888.Encode(source),
SexyTexFormat.RGBA4444 => RGBA4444.Encode(source),
SexyTexFormat.RGBA5551 => RGBA5551.Encode(source),
_ => throw new NotSupportedException(FormatMsg(format) )
};

// Compress image

private static void CompressImg(NativeBuffer input, Stream output)
{
using var inputStream = input.GetStream();
ZLibCompressor.CompressStream(inputStream, output, default);

long currentPos = output.Position;
var sizeCompressed = (int)(output.Length - 48); // Size doesn't include header bytes

output.Seek(32, SeekOrigin.Begin);

output.WriteInt32(sizeCompressed); // Update header property: SizeCompressed

output.Seek(currentPos, SeekOrigin.Begin);
}

// Encode SexyTex (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, SexyTexFormat format, bool useZlib)
{
var flags = useZlib ? CompressionFlags.ZLib : CompressionFlags.NoCompression;

SexyTexInfo info = new(source.Width, source.Height, format, flags);

target.WriteUInt64(MAGIC, Endianness.BigEndian);
info.WriteBin(target);

using var raw = EncodeRaw(source, format);

if(useZlib)
CompressImg(raw, target);

else
target.Write(raw.GetView() );

}

// Encode SexyTex (bitmap to buffer)

public static NativeBuffer Encode(SKBitmap source, SexyTexFormat format, bool useZlib)
{
using MemoryStream output = new();

Encode(source, output, format, useZlib);
output.Seek(0, SeekOrigin.Begin);

return output.ReadPtr();
}

// Encode SexyTex (stream based)

public static void Encode(Stream input, Stream output, SexyTexFormat format, bool useZlib)
{
using var bitmap = SKPlugin.FromStream(input);

Encode(bitmap, output, format, useZlib);
}

// Encode SexyTex (stream to buffer)

public static NativeBuffer Encode(Stream src, SexyTexFormat format, bool useZlib)
{
using var bitmap = SKPlugin.FromStream(src);

return Encode(bitmap, format, useZlib);
}

/** <summary> Encodes an Image as a SexyTexture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded TEX File will be Saved. </param> **/

public static void EncodeFile(string inputPath, string outputPath, SexyTexFormat format, bool useZlib)
{

TraceImgParser.Encode("SexyTex Encoding",
                      inputPath,
                      outputPath,
                      ".tex",
                      (input, output) => Encode(input, output, format, useZlib),
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath),
                      ("Format", format),
                      ("UseZlib", useZlib)
					 
);
			  
}

// Read SexyTex info

private static SexyTexInfo ReadTexInfo(NativeBuffer source)
{
ulong flags = source.GetUInt64(0, Endianness.BigEndian);

if(flags != MAGIC)
throw new Exception($"Invalid file identifier: 0x{flags:X16}, expected: 0x{MAGIC:X16}");

var rawInfo = source.GetView(8, 40);
var info = SexyTexInfo.ReadBin(rawInfo);

if(info.Version != VERSION)
TraceLogger.WriteWarn($"Unknown version: V{info.Version} - Expected: V{VERSION}");

return info;
}

// Decompress image

private static NativeBuffer DecompressImg(Stream input, int bytesCompressed)
{
using MemoryStream rawStream = new();

ZLibCompressor.DecompressStream(input, rawStream, bytesCompressed);
rawStream.Seek(0, SeekOrigin.Begin);

return rawStream.ReadPtr();
}

// Decode raw tex

private static SKBitmap DecodeRaw(ReadOnlySpan<byte> source, SexyTexFormat format, int width, int height)
{

return format switch
{
SexyTexFormat.ARGB8888 => ARGB8888.Decode(source, width, height),
SexyTexFormat.ARGB4444 => ARGB4444.Decode(source, width, height),
SexyTexFormat.ARGB1555 => ARGB1555.Decode(source, width, height),
SexyTexFormat.RGB565 => RGB565.Decode(source, width, height),
SexyTexFormat.ABGR8888 => ABGR8888.Decode(source, width, height),
SexyTexFormat.RGBA4444 => RGBA4444.Decode(source, width, height),
SexyTexFormat.RGBA5551 => RGBA5551.Decode(source, width, height),
_ => throw new NotSupportedException(FormatMsg(format) )
};

}

// Decode SexyTex (ZLib compressed)

private static unsafe SKBitmap DecodeCompressed(ReadOnlySpan<byte> source, in SexyTexInfo info)
{

fixed(byte* ptr = source)
{
using ReadOnlyUnmanagedStream compressedStream = new(ptr, source.Length);
using var raw = DecompressImg(compressedStream, info.SizeCompressed);

return DecodeRaw(raw.GetView(), info.Format, info.Width, info.Height);
}

}

// Decode SexyTex (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
var info = ReadTexInfo(source); 
var textureBytes = source.GetView(48);

if(info.CompressionType == CompressionFlags.ZLib)
return DecodeCompressed(textureBytes, info);

return DecodeRaw(textureBytes, info.Format, info.Width, info.Height);
}

// Decode SexyTex (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var tex = source.ReadPtr();

return Decode(tex);
}

// Decode SexyTex (buffer to stream)

public static void Decode(NativeBuffer source, Stream output)
{
using var image = Decode(source);

image.Save(output);
}

// Decode SexyTex (stream based)

public static void Decode(Stream input, Stream output)
{
using var tex = input.ReadPtr();

Decode(tex, output);
}

/** <summary> Decodes a SexyTexture as an Image. </summary>

<param name = "inputPath"> The Path where the SexyTex to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> **/

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("SexyTexture Decoding",
                      inputPath,
                      outputPath,
                      Decode,
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath)
					  
);
					  
}

}

}