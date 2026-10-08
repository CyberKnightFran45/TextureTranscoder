using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.PopCapTexture
{
/// <summary> Parses PopCap Textures (PTX) from Mobile Games </summary>

public static class PtxParser
{
// PTX Identifier

private const uint MAGIC = 0x70747831;

// PTX Identifier (Big Endian)

private const uint MAGIC_BE = 0x31787470;

// Macro tile size

private const int MACRO_SIZE = 32;

// Get error msg

private static string FormatMsg(PtxFormat format) => $"Unsupported format: {format} (0x{(int)format:X8})";

// Get pitch for RGB Tiled texture

private static int GetRgbTiledPitch(int width)
{
int paddedWidth = (width + (MACRO_SIZE - 1) ) & ~(MACRO_SIZE - 1);

return paddedWidth * 2;
}

// Get pitch for PVR texture

private static int GetPvrPitch(int width, bool is2BPP)
{
var blockWidth = is2BPP ? PVRBase.BLOCK_WIDTH_2BPP : PVRBase.BLOCK_WIDTH_4BPP;
int paddedWidth = TextureHelper.Pad(width, blockWidth);

return paddedWidth * 2;
}

// Get pitch for ASTC Texture

private static int GetAstcPitch(int width, int blockWidth)
{
int blocks = TextureHelper.GetBlockDim(width, blockWidth);

return blocks * 16;
}

// Compute texture pitch

private static int ComputePitch(int width, PtxFormat format) => format switch
{
PtxFormat.ARGB8888 or PtxFormat.RGBA8888 => width * 4,
PtxFormat.ETC1_RGB_A8 or PtxFormat.ETC1_RGB_A_Palette => width * 4,
PtxFormat.RGB565 or PtxFormat.RGBA4444 or PtxFormat.RGBA5551 => width * 2,
PtxFormat.RGB565_Tiled or PtxFormat.RGBA4444_Tiled or PtxFormat.RGBA5551_Tiled => GetRgbTiledPitch(width),
PtxFormat.PVRTC_4BPP_RGBA or PtxFormat.PVRTC_4BPP_RGB_A8 => GetPvrPitch(width, false),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR => GetAstcPitch(width, 4),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR => GetAstcPitch(width, 5),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR => GetAstcPitch(width, 6),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR => GetAstcPitch(width, 8),
_ => 0,
};

// Encode raw image

private static NativeBuffer EncodeRaw(ref SKBitmap source,
                                      PtxFormat format,
                                      out PtxAlphaChannel aChannel,
                                      out int aSize,
                                      out uint pitch)
{
pitch = (uint)ComputePitch(source.Width, format);

aChannel = format == PtxFormat.ETC1_RGB_A_Palette ? PtxAlphaChannel.A_Palette : default;
aSize = 0;

return format switch
{
PtxFormat.ARGB8888 => ARGB8888.Encode(source),
PtxFormat.RGBA8888 => ABGR8888.Encode(source),
PtxFormat.RGBA4444 => RGBA4444.Encode(source),
PtxFormat.RGB565 => RGB565.Encode(source),
PtxFormat.RGBA5551 => RGBA5551.Encode(source),
PtxFormat.RGBA4444_Tiled => RGBA4444_Tiled.Write(source, MACRO_SIZE),
PtxFormat.RGB565_Tiled => RGB565_Tiled.Encode(source, MACRO_SIZE),
PtxFormat.RGBA5551_Tiled => RGBA5551_Tiled.Encode(source, MACRO_SIZE),
PtxFormat.PVRTC_4BPP_RGBA => PVRTC_4BPP_RGBA.Encode(ref source),
PtxFormat.ETC1_RGB_A8 => ETC1_RGB_A8.Encode(source, true),
PtxFormat.ETC1_RGB_A_Palette => ETC1_RGB_A_Palette.Encode(source, out aSize),
PtxFormat.PVRTC_4BPP_RGB_A8 => PVRTC_4BPP_RGB_A8.Encode(ref source),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR => AstcBridge.Encode(source, 4, 4),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR => AstcBridge.Encode(source, 5, 5),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR => AstcBridge.Encode(source, 6, 6),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR => AstcBridge.Encode(source, 8, 8),
_ => throw new NotSupportedException(FormatMsg(format) )
};

}

// Encode PTX (bitmap to buffer)

public static NativeBuffer Encode(ref SKBitmap source, PtxFormat format, out PtxInfo info)
{
var ptxBuffer = EncodeRaw(ref source, format, out var aChannel, out var aSize, out var pitch);

var width = (uint)source.Width;
var height = (uint)source.Height;

info = new(width, height, pitch, format, (uint)aSize, aChannel);

return ptxBuffer;
}

// Encode PTX (bitmap to stream)

public static PtxInfo Encode(ref SKBitmap source, Stream target, PtxFormat format)
{
using var ptx = Encode(ref source, format, out var info);
target.Write(ptx.GetView() );

return info;
}

// Encode PTX (stream based)

public static PtxInfo Encode(Stream input, Stream output, PtxFormat format)
{
var bitmap = SKPlugin.FromStream(input);

using var ptx = Encode(ref bitmap, format, out var info);
output.Write(ptx.GetView() );

bitmap.Dispose();

return info;
}

// Encode PTX (stream to buffer)

public static NativeBuffer Encode(Stream src, PtxFormat format, out PtxInfo info)
{
var bitmap = SKPlugin.FromStream(src);
var ptx = Encode(ref bitmap, format, out info);

bitmap.Dispose();

return ptx;
}

// Save ptx info

private static void SavePtxInfo(string outPath, Endianness endian, in PtxInfo info)
{
using var cfg = FileManager.OpenWrite(outPath);

info.WriteBin(cfg, endian);
}

// Encode internal

private static void EncodeInternal(Stream input,
                                   Stream output,
								   PtxFormat format,
								   Endianness endian,
                                   string pathToInfo)
{

var info = Encode(input, output, format);

if(pathToInfo != null)
SavePtxInfo(pathToInfo, endian, info);

}

/** <summary> Encodes an Image as a PopCapTexture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded PTX File will be Saved. </param> */

public static void EncodeFile(string inputPath,
                              string outputPath,
							  PtxFormat format,
							  Endianness endian,
                              string pathToInfo)
{

TraceImgParser.Encode("PopCap Texture Encoding",
                      inputPath,
                      outputPath,
                      ".ptx",
                      (input, output) => EncodeInternal(input, output, format, endian, pathToInfo),
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath),
					  ("Format", format),
					  ("PathToInfo", pathToInfo),
					  ("InfoEndianness", endian)

);

}

// Check PTX info

private static void CheckPtxInfo(ref PtxInfo info)
{
uint flags = info.Magic;

switch(flags)
{
case MAGIC:
break;

case MAGIC_BE:
info.SwapEndian();
break;

default:
var expectedFlags = $"0x{MAGIC:X8} | 0x{MAGIC_BE:X8} (BigEndian)";

throw new Exception($"Invalid PTX identifier: 0x{flags:X8}, expected: {expectedFlags}");
}

}

// Decode raw ptx

private static SKBitmap DecodeRaw(ReadOnlySpan<byte> source, PtxFormat format, int width, int height) 
{

return format switch
{
PtxFormat.ARGB8888 => ARGB8888.Decode(source, width, height),
PtxFormat.RGBA8888 => ABGR8888.Decode(source, width, height),
PtxFormat.RGBA4444 => RGBA4444.Decode(source, width, height),
PtxFormat.RGB565 => RGB565.Decode(source, width, height),
PtxFormat.RGBA5551 => RGBA5551.Decode(source, width, height),
PtxFormat.RGBA4444_Tiled => RGBA4444_Tiled.Read(source, width, height, MACRO_SIZE),
PtxFormat.RGB565_Tiled => RGB565_Tiled.Decode(source, width, height, MACRO_SIZE),
PtxFormat.RGBA5551_Tiled => RGBA5551_Tiled.Decode(source, width, height, MACRO_SIZE),
PtxFormat.PVRTC_4BPP_RGBA => PVRTC_4BPP_RGBA.Decode(source, width, height),
PtxFormat.ETC1_RGB_A8 => ETC1_RGB_A8.Decode(source, width, height, true),
PtxFormat.ETC1_RGB_A_Palette => ETC1_RGB_A_Palette.Decode(source, width, height),
PtxFormat.PVRTC_4BPP_RGB_A8 => PVRTC_4BPP_RGB_A8.Decode(source, width, height),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_4x4_KHR => AstcBridge.Decode(source, width, height, 4, 4),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_5x5_KHR => AstcBridge.Decode(source, width, height, 5, 5),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_6x6_KHR => AstcBridge.Decode(source, width, height, 6, 6),
PtxFormat.GL_COMPRESSED_RGBA_ASTC_8x8_KHR => AstcBridge.Decode(source, width, height, 8, 8),
_ => throw new NotSupportedException(FormatMsg(format) )
};

}

// Decode PTX (buffer to bitmap)

public static SKBitmap Decode(ReadOnlySpan<byte> source, ref PtxInfo info)
{
CheckPtxInfo(ref info);

return DecodeRaw(source, info.Format, (int)info.Width, (int)info.Height);
}

// Decode PTX (stream to bitmap)

public static SKBitmap Decode(Stream source, ref PtxInfo info)
{
using var ptx = source.ReadPtr();

return Decode(ptx.GetView(), ref info);
}

// Decode PTX (buffer to stream)

public static void Decode(ReadOnlySpan<byte> source, Stream output, ref PtxInfo info)
{
using var image = Decode(source, ref info);

image.Save(output);
}

// Decode PTX (stream based)

public static void Decode(Stream input, Stream output, ref PtxInfo info)
{
using var ptx = input.ReadPtr();

Decode(ptx.GetView(), output, ref info);
}

/** <summary> Decodes a PopCapTexture. </summary>

<param name = "inputPath"> The Path where the PTX to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */	

public static void DecodeFile(string inputPath, string outputPath, PtxInfo info)
{

TraceImgParser.Decode("PopCap Texture Decoding",
                      inputPath,
                      outputPath,
                      (input, output) => Decode(input, output, ref info),
					  ("InputPath", inputPath),
					  ("OutputPath", outputPath),
					  ("Dimensions", $"{info.Width}x{info.Height}"),
					  ("Format", info.Format)

);

}

}

}