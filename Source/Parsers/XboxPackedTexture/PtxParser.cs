using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.XboxPackedTexture
{
/// <summary> Parses PTX Files from Xbox360. </summary>

public static class PtxParser
{
/// <summary> The Identifier of a PTX File (BE). </summary>

private const uint MAGIC = 0x5400201A;

/// <summary> The Endianness used. </summary>

private const Endianness ENDIAN = Endianness.BigEndian;

// Get PTX Stream

public static int Encode(SKBitmap source, Stream target)
{
int width = source.Width;

int paddedWidth = TextureHelper.Pad(width, 128);
int blockSize = paddedWidth * 4;

PtxInfo fileInfo = new(source.Height, width, blockSize);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Writing header...");

fileInfo.WriteBin(target);
target.WriteUInt32(MAGIC, ENDIAN);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Encode Image:");
TraceLogger.WriteLine();

return DXT5_RGBA_Padding.Write(target, source, blockSize);
}

/** <summary> Encodes a PTX File. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded PTX File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("PTX-Xbox360 Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".ptx");	
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

using var srcImage = SKPlugin.FromFile(inputPath);
using var dstFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

int stride = Encode(srcImage, dstFile);

pxWritten = stride * srcImage.Height;
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("PTX-Xbox360 Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
}

// Get Plain Image

public static SKBitmap Decode(Stream source)
{
TraceLogger.WriteLine("Step #1 - Read Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reading header...");

var fileInfo = PtxInfo.ReadBin(source);
uint inputMagic = source.ReadUInt32(ENDIAN);

if(inputMagic != MAGIC)
{
const string ERROR_INVALID_FLAGS = "Invalid PTX identifier: {0:X8}, expected: {1:X8}";

TraceLogger.WriteError(string.Format(ERROR_INVALID_FLAGS, inputMagic, MAGIC) );
return null;
}

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Decode Image:");
TraceLogger.WriteLine();

return DXT5_RGBA_Padding.Read(source, fileInfo.Width, fileInfo.Height, fileInfo.BlockSize);
}
/** <summary> Decodes a PTX File. </summary>

<param name = "inputPath"> The Path where the PTX File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("PTX-Xbox360 Decoding Started");

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

TraceLogger.WriteLine("PTX-Xbox360 Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}