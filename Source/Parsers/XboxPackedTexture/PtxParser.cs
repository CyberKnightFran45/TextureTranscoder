using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.XboxPackedTexture
{
/// <summary> Parses PTX Files from Xbox360 </summary>

public static class PtxParser
{
/// <summary> File identifier (BE) </summary>

private const uint MAGIC = 0x5400201A;

// Create PtxInfo

private static PtxInfo BuildInfo(SKBitmap source)
{
int width = source.Width;
int height = source.Height;

int blockSize = TextureHelper.ComputeBlockSize(width, 128);

return new(width, height, blockSize);
}

// Encode PTX-Xbox360 (bitmap to buffer)

public static NativeBuffer Encode(SKBitmap source)
{
var info = BuildInfo(source);

using var raw = DXT5_RGBA_Padding.Encode(source, info.BlockSize);

ulong bufferSize = raw.Size + 16;
NativeBuffer ptxBuffer = new(bufferSize);

var rawInfo = ptxBuffer.AsSpan(0, 12);
info.WriteBin(rawInfo);

ptxBuffer.SetUInt32(12, MAGIC, Endianness.BigEndian);

ptxBuffer.CopyFrom(raw, 16);

return ptxBuffer;
}

// Encode PTX-Xbox360 (bitmap to stream)

public static void Encode(SKBitmap source, Stream target)
{
using var owner = Encode(source);

target.Write(owner.GetView() );
}

// Encode PTX-Xbox360 (stream based)

public static void Encode(Stream input, Stream output)
{
using var bitmap = SKPlugin.FromStream(input);
using var owner = Encode(bitmap);

output.Write(owner.GetView() );
}

// Encode PTX-Xbox360 (stream to buffer)

public static NativeBuffer Encode(Stream src)
{
using var bitmap = SKPlugin.FromStream(src);

return Encode(bitmap);
}

/** <summary> Encodes a PTX file </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded PTX File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath)
{

TraceImgParser.Encode("PTX-Xbox360 Encoding",
                      inputPath,
                      outputPath,
                      ".ptx",
                      Encode,
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath)

);

}

// Read Ptx info

private static PtxInfo ReadPtxInfo(NativeBuffer source)
{
var rawInfo = source.GetView(0, 12);
var info = PtxInfo.ReadBin(rawInfo);

uint flags = source.GetUInt32(12, Endianness.BigEndian);

if(flags != MAGIC)
throw new Exception($"Invalid file identifier: 0x{flags:X8}, expected: 0x{MAGIC:X8}");

return info;
}

// Decode PTX-Xbox360 (buffer to bitmap) 

public static SKBitmap Decode(NativeBuffer source)
{
var info = ReadPtxInfo(source);

return DXT5_RGBA_Padding.Decode(source.GetView(16), info.Width, info.Height, info.BlockSize);
}

// Decode PTX-Xbox360 (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var ptx = source.ReadPtr();

return Decode(ptx);
}

// Decode PTX-Xbox360 (buffer to stream)

public static void Decode(NativeBuffer input, Stream output)
{
using var image = Decode(input);

image.Save(output);
}

// Decode PTX-Xbox360 (stream based)

public static void Decode(Stream input, Stream output)
{
using var ptx = input.ReadPtr();

Decode(ptx, output);
}

/** <summary> Decodes a PTX File. </summary>

<param name = "inputPath"> The Path where the PTX File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("PTX-Xbox360 Decoding",
                      inputPath,
                      outputPath,
					  Decode,
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath)

);

}

}

}