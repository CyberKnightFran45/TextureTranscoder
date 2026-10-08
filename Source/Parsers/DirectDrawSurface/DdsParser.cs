using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.DirectDrawSurface
{
/// <summary> Encodes images to DDS files and viceversa </summary>

public static class DdsParser
{
/// <summary> DDS identifier </summary>

private const uint MAGIC = 0x44445320;

// Get error msg

private static string FormatMsg(DdsFormat format) => $"Unsupported format: {format} (0x{(uint)format:X8})";

// Encode raw image

private static NativeBuffer EncodeRaw(SKBitmap source, DdsFormat format) => format switch
{
DdsFormat.DXT1 => DXT1_RGBA.Encode(source),
DdsFormat.DXT3 => DXT3_RGBA.Encode(source),
DdsFormat.DXT5 => DXT5_RGBA.Encode(source),
_ => throw new NotSupportedException(FormatMsg(format) )
};

// Encode DDS (bitmap to buffer)

public static NativeBuffer Encode(SKBitmap source, DdsFormat format)
{
DdsInfo info = new(source.Width, source.Height, format);

using NativeBuffer raw = EncodeRaw(source, format);

ulong bufferSize = raw.Size + 128;
NativeBuffer ddsBuffer = new(bufferSize);

ddsBuffer.SetUInt32(0, MAGIC, Endianness.BigEndian);

var rawInfo = ddsBuffer.AsSpan(4, 124);
info.WriteBin(rawInfo);

ddsBuffer.CopyFrom(raw, 128);

return ddsBuffer;
}

// Encode DDS (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, DdsFormat format)
{
using var dds = Encode(source, format);

target.Write(dds.GetView() );
}

// Encode DDS (stream based)

public static void Encode(Stream input, Stream output, DdsFormat format)
{
using var bitmap = SKPlugin.FromStream(input);
using var dds = Encode(bitmap, format);

output.Write(dds.GetView() );
}

// Encode DDS (stream to buffer)

public static NativeBuffer Encode(Stream src, DdsFormat format)
{
using var bitmap = SKPlugin.FromStream(src);

return Encode(bitmap, format);
}

/** <summary> Encodes a DDS Image. </summary>

<param name = "inputPath"> The Path where the Image to Encode is located. </param>
<param name = "outputPath"> The Location where the Encoded DDS File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, DdsFormat format)
{

TraceImgParser.Encode("DDS Encoding",
                      inputPath,
                      outputPath,
                      ".dds",
                      (input, output) => Encode(input, output, format),
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath),
					  ("Format", format)

);

}

// Read DDS info

private static DdsInfo ReadDdsInfo(NativeBuffer source)
{
uint flags = source.GetUInt32(0, Endianness.BigEndian);

if(flags != MAGIC)
throw new Exception($"Invalid file identifier: 0x{flags:X8}, expected: 0x{MAGIC:X8}");

var rawInfo = source.GetView(4, 124);

return DdsInfo.ReadBin(rawInfo);
}

// Decode raw dds

private static SKBitmap DecodeRaw(ReadOnlySpan<byte> source, DdsFormat format, int width, int height)
{

return format switch
{
DdsFormat.DXT1 => DXT1_RGBA.Decode(source, width, height),
DdsFormat.DXT3 => DXT3_RGBA.Decode(source, width, height),
DdsFormat.DXT5 => DXT5_RGBA.Decode(source, width, height),
_ => throw new NotSupportedException(FormatMsg(format))
};

}

// Decode DDS (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
var info = ReadDdsInfo(source);

return DecodeRaw(source.GetView(128), info.Format, info.Width, info.Height);
}

// Decode DDS (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var dds = source.ReadPtr();

return Decode(dds);
}

// Decode DDS (buffer to stream)

public static void Decode(NativeBuffer source, Stream output)
{
using var image = Decode(source);

image.Save(output);
}

// Decode DDS (stream based)

public static void Decode(Stream input, Stream output)
{
using var dds = input.ReadPtr();

Decode(dds, output);
}

/** <summary> Decodes a DDS File. </summary>

<param name = "inputPath"> The Path where the DDS File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("DDS Decoding",
                      inputPath,
                      outputPath,
					  Decode,
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath)

);

}

}

}