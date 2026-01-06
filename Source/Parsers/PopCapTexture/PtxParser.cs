using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.PopCapTexture
{
/// <summary> Parses PopCap Textures (PTX). </summary>

public static class PtxParser
{
// PTX Identifier

private const uint MAGIC = 0x70747831;

// PTX Identifier (Big Endian)

private const uint MAGIC_BE = 0x31787470;

// Get PTX Stream

public static PtxInfo Encode(ref SKBitmap source, Stream target, PtxFormat format)
{
var alphaChannel = format == PtxFormat.ETC1_RGB_A_Palette ? PtxAlphaChannel.A_Palette : default;
int alphaSize = 0;

int pitch = format switch
{
PtxFormat.RGBA8888 => ABGR8888.Write(target, source),
PtxFormat.RGBA4444 => RGBA4444.Write(target, source),
PtxFormat.RGB565 => RGB565.Write(target, source),
PtxFormat.RGBA5551 => RGBA5551.Write(target, source),
PtxFormat.RGBA4444_Tiled => RGBA4444_Tiled.Write(target, source),
PtxFormat.RGB565_Tiled => RGB565_Tiled.Write(target, source),
PtxFormat.RGBA5551_Tiled => RGBA5551_Tiled.Write(target, source),
PtxFormat.PVRTC_4BPP_RGBA => PVRTC_4BPP_RGBA.Write(target, ref source),
PtxFormat.ETC1_RGB_A8 => ETC1_RGB_A8.Write(target, source),
PtxFormat.ETC1_RGB_A_Palette => ETC1_RGB_A_Palette.Write(target, source, out alphaSize),
PtxFormat.PVRTC_4BPP_RGB_A8 => PVRTC_4BPP_RGB_A8.Write(target, ref source),
_ => ARGB8888.Write(target, source)
};

return new(source.Width, source.Height, pitch, format, alphaSize, alphaChannel);
}

/** <summary> Encodes an Image as a PopCapTexture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded PTX File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, string pathToInfo,
                              PtxFormat format, Endianness endian)
{
TraceLogger.Init();
TraceLogger.WriteLine("PopTexture Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".ptx");	
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

var srcImage = SKPlugin.FromFile(inputPath);
using var dstFile = FileManager.OpenWrite(outputPath);
using var cfgFile = FileManager.OpenWrite(pathToInfo);

TraceLogger.WriteActionEnd();

var info = Encode(ref srcImage, dstFile, format);

TraceLogger.WriteActionStart("Saving image info...");
info.WriteBin(cfgFile, endian);

TraceLogger.WriteActionEnd();

TraceLogger.WriteInfo($"Saved at: {pathToInfo}");

pxWritten = info.Pitch * srcImage.Height;
srcImage.Dispose();
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("PopTexture Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
}

// Get Plain Image

public static SKBitmap Decode(Stream source, PtxFormat format, int width, int height,
                              Endianness endian = default)
{

return format switch
{
PtxFormat.RGBA8888 => ABGR8888.Read(source, width, height),
PtxFormat.RGBA4444 => RGBA4444.Read(source, width, height),
PtxFormat.RGB565 => RGB565.Read(source, width, height),
PtxFormat.RGBA5551 => RGBA5551.Read(source, width, height),
PtxFormat.RGBA4444_Tiled => RGBA4444_Tiled.Read(source, width, height),
PtxFormat.RGB565_Tiled => RGB565_Tiled.Read(source, width, height),
PtxFormat.RGBA5551_Tiled => RGBA5551_Tiled.Read(source, width, height),
PtxFormat.PVRTC_4BPP_RGBA => PVRTC_4BPP_RGBA.Read(source, width, height),
PtxFormat.ETC1_RGB_A8 => ETC1_RGB_A8.Read(source, width, height),
PtxFormat.ETC1_RGB_A_Palette => ETC1_RGB_A_Palette.Read(source, width, height),
PtxFormat.PVRTC_4BPP_RGB_A8 => PVRTC_4BPP_RGB_A8.Read(source, width, height),
_ => ARGB8888.Read(source, width, height)
};

}

// Decode PTX

private static SKBitmap Decode(Stream source, ref PtxInfo info)
{
const string ERROR_INVALID_FLAGS = "Invalid PTX identifier: {0:X8}, expected: {1:X8} ({2:X8} in BigEndian)";

uint flags = info.Magic;
Endianness endian;

switch(flags)
{
case MAGIC:
endian = Endianness.LittleEndian;
break;

case MAGIC_BE:
endian = Endianness.BigEndian;

info.SwapEndian();
break;

default:
TraceLogger.WriteError(string.Format(ERROR_INVALID_FLAGS, flags, MAGIC, MAGIC_BE) );

return null;
}

TraceLogger.WriteInfo($"Endianness detected: {endian}");

return Decode(source, info.Format, info.Width, info.Height, endian);
}

/** <summary> Decodes a PopCapTexture. </summary>

<param name = "inputPath"> The Path where the PTX to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */	

public static void DecodeFile(string inputPath, string outputPath, string pathToInfo)
{
TraceLogger.Init();
TraceLogger.WriteLine("PopTexture Decoding Started");

int width, height = width = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".png");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

using var srcFile = FileManager.OpenRead(inputPath);
using var cfgFile = FileManager.OpenRead(pathToInfo);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Loading image info...");
var info = PtxInfo.ReadBin(cfgFile);

TraceLogger.WriteActionEnd();

using var image = Decode(srcFile, ref info);

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

TraceLogger.WriteLine("PopTexture Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}