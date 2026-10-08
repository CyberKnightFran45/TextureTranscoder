using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.UTexture
{
/// <summary> Encodes images to UTex files and viceversa </summary>

public static class UTexParser
{
/// <summary> UTex identifier </summary>

private const ushort MAGIC = 0x0A75;

// Get error msg

private static string FormatMsg(UTexFormat format) => $"Unsupported format: {format} (0x{(ushort)format:X4})";

// Encode raw image

private static NativeBuffer EncodeRaw(SKBitmap source, UTexFormat format) => format switch
{
UTexFormat.ABGR8888 => ABGR8888.Encode(source),
UTexFormat.RGBA4444 => RGBA4444.Encode(source),
UTexFormat.RGBA5551 => RGBA5551.Encode(source),
UTexFormat.RGB565 => RGB565.Encode(source),
_ => throw new NotSupportedException(FormatMsg(format) )
};

// Encode U-Tex (bitmap to buffer)

public static NativeBuffer Encode(SKBitmap source, UTexFormat format)
{
UTexInfo info = new(source.Width, source.Height, format);

using var raw = EncodeRaw(source, format);

ulong bufferSize = raw.Size + 8;
NativeBuffer texBuffer = new(bufferSize);

texBuffer.SetUInt16(0, MAGIC, Endianness.LittleEndian);

var rawInfo = texBuffer.AsSpan(2, 6);
info.WriteBin(rawInfo);

texBuffer.CopyFrom(raw, 8);

return texBuffer;
}

// Encode U-Tex (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, UTexFormat format)
{
using var tex = Encode(source, format);

target.Write(tex.GetView() );
}

// Encode U-Tex (stream based)

public static void Encode(Stream input, Stream output, UTexFormat format)
{
using var bitmap = SKPlugin.FromStream(input);
using var tex = Encode(bitmap, format);

output.Write(tex.GetView() );
}

// Encode U-Tex (stream to buffer)

public static NativeBuffer Encode(Stream src, UTexFormat format)
{
using var bitmap = SKPlugin.FromStream(src);

return Encode(bitmap, format);
}

/** <summary> Encodes a U-Texture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, UTexFormat format)
{

TraceImgParser.Encode("U-Texture Encoding",
                      inputPath,
                      outputPath,
                      ".tex",
                      (input, output) => Encode(input, output, format),
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath),
					  ("Format", format)

);

}

// Read U-Tex info

private static UTexInfo ReadTexInfo(NativeBuffer source)
{
ushort flags = source.GetUInt16(0, Endianness.LittleEndian);

if(flags != MAGIC)
throw new Exception($"Invalid file identifier: 0x{flags:X4}, expected: 0x{MAGIC:X4}");

var rawInfo = source.GetView(2, 6);

return UTexInfo.ReadBin(rawInfo);
}

// Decode raw tex

private static SKBitmap DecodeRaw(ReadOnlySpan<byte> source, UTexFormat format, int width, int height)
{

return format switch
{
UTexFormat.ABGR8888 => ABGR8888.Decode(source, width, height),
UTexFormat.RGBA4444 => RGBA4444.Decode(source, width, height),
UTexFormat.RGBA5551 => RGBA5551.Decode(source, width, height),
UTexFormat.RGB565 => RGB565.Decode(source, width, height),
_ => throw new NotSupportedException(FormatMsg(format))
};

}

// Decode U-Tex (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
var info = ReadTexInfo(source);

return DecodeRaw(source.GetView(8), info.Format, info.Width, info.Height);
}

// Decode U-Tex (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var tex = source.ReadPtr();

return Decode(tex);
}

// Decode U-Tex (buffer to stream)

public static void Decode(NativeBuffer source, Stream output)
{
using var image = Decode(source);

image.Save(output);
}

// Decode U-Tex (stream based)

public static void Decode(Stream input, Stream output)
{
using var tex = input.ReadPtr();

Decode(tex, output);
}

/** <summary> Decodes a U-Tex file. </summary>

<param name = "inputPath"> The Path where the TEX File to be Decoded is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image File will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("U-Texture Decoding",
                      inputPath,
                      outputPath,
					  Decode,
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath)

);

}

}

}