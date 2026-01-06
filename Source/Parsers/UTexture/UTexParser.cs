using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.UTexture
{
/// <summary> Initializes Parsing Tasks for UTex Files. </summary>

public static class UTexParser
{
/// <summary> The Identifier of a UTex File. </summary>

private const ushort MAGIC = 2677;

// Get TEX Stream

public static int Encode(SKBitmap source, Stream target, UTexFormat format)
{
UTexInfo fileInfo = new(source.Width, source.Height, format);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Writing header...");

target.WriteUInt16(MAGIC);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Encode Image:");
TraceLogger.WriteLine();

return format switch
{
UTexFormat.RGBA4444 => RGBA4444.Write(target, source),
UTexFormat.RGBA5551 => RGBA5551.Write(target, source),
UTexFormat.RGB565 => RGB565.Write(target, source),
_ => ABGR8888.Write(target, source)
};

}

/** <summary> Encodes an Image as U-Texture. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, UTexFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("U-Texture Encoding Started");

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

TraceLogger.WriteLine("U-Texture Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
}

// Get Plain Image

public static SKBitmap Decode(Stream source)
{
TraceLogger.WriteLine("Step #1 - Read Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reading header...");

ushort inputMagic = source.ReadUInt16();

if(inputMagic != MAGIC)
{
const string ERROR_INVALID_MAGIC = "Invalid UTex Identifier: {0:X4}, expected: {1:X4}";

TraceLogger.WriteError(string.Format(ERROR_INVALID_MAGIC, inputMagic, MAGIC) );
return null;
}

var fileInfo = UTexInfo.ReadBin(source);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Decode Image:");
TraceLogger.WriteLine();

var format = fileInfo.Format;

if(!Enum.IsDefined(format) )
{
TraceLogger.WriteError($"Unknown format: {format}, cannot decode this File.");

return null;
}

int width = fileInfo.Width;
int height = fileInfo.Height;

return format switch
{
UTexFormat.RGBA4444 => RGBA4444.Read(source, width, height),
UTexFormat.RGBA5551 => RGBA5551.Read(source, width, height),
UTexFormat.RGB565 => RGB565.Read(source, width, height),       
_ => ABGR8888.Read(source, width, height)
};

}

/** <summary> Decodes a TEX File as an Image. </summary>

<param name = "inputPath"> The Path where the TEX File to be Decoded is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image File will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("U-Texture Decoding Started");

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

TraceLogger.WriteLine("U-Texture Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}