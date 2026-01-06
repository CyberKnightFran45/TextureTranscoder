using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.DirectDrawSurface
{
/// <summary> Parse DDS Files. </summary>

public static class DdsParser
{
/// <summary> The Header of a DDS File. </summary>

private const string HEADER = "DDS ";

// Get PTX Stream

public static int Encode(SKBitmap source, Stream target, DdsFormat format)
{
DdsInfo fileInfo = new(source.Width, source.Height, format);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Writing header...");

target.WriteString(HEADER);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Encode Image:");
TraceLogger.WriteLine();

return format switch
{
DdsFormat.DXT3 => DXT3_RGBA.Write(target, source),
DdsFormat.DXT5 => DXT5_RGBA.Write(target, source),
_ => DXT1_RGBA.Write(target, source)
};

}

/** <summary> Encodes a DDS Image. </summary>

<param name = "inputPath"> The Path where the Image to Encode is located. </param>
<param name = "outputPath"> The Location where the Encoded DDS File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, DdsFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("DDS Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".dds");
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

TraceLogger.WriteLine("DDS Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
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

var fileInfo = DdsInfo.ReadBin(source);

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
DdsFormat.DXT3 => DXT3_RGBA.Read(source, width, height),
DdsFormat.DXT5 => DXT5_RGBA.Read(source, width, height),
_ => DXT1_RGBA.Read(source, width, height)
};

}

/** <summary> Decodes a DDS File. </summary>

<param name = "inputPath"> The Path where the DDS File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("DDS Decoding Started");

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

TraceLogger.WriteLine("DDS Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}