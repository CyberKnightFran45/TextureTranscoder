using System;
using System.IO;
using System.Linq;
using SkiaSharp;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Parses XNB Images (from Microsoft). </summary>

public static class XnbParser
{
/// <summary> The Header of a XNB File. </summary>

private const string HEADER = "XNB";

/// <summary> The Version of a XNB File. </summary>

private const byte VERSION = 5;

// Get XNB Stream

public static int Encode(SKBitmap source, Stream target, XnbPlatform platform, XnbFormat format)
{
XnbInfo fileInfo = new(source.Width, source.Height, platform, format);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Writing header...");

target.WriteString(HEADER);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #2 - Encode Image:");
TraceLogger.WriteLine();

int stride = format switch
{
XnbFormat.Bgr565 => RGB565.Write(target, source),
XnbFormat.Bgra5551 => RGBA5551.Write(target, source), 
XnbFormat.Bgra4444 => RGBA4444.Write(target, source),
XnbFormat.NormalizedByte2 => NormVector2D.Write(target, source),
XnbFormat.NormalizedByte4 => NormVector4D.Write(target, source),
_ => DXT5_RGBA.Write(target, source)
};

TraceLogger.WriteLine("Step #3 - Finalize Info:");
TraceLogger.WriteLine();

long currentPos = target.Position;

TraceLogger.WriteActionStart("Updating header...");

target.Seek(6, SeekOrigin.Begin);

var sizeCompressed = (int)target.Length;
target.WriteInt32(sizeCompressed);

TraceLogger.WriteActionEnd();

target.Seek(currentPos, SeekOrigin.Begin);

return stride;
}

/** <summary> Encodes an Image as a XNB File. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded XNB File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, XnbPlatform platform, XnbFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("XNB Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".xnb");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");

using var srcImage = SKPlugin.FromFile(inputPath);
using var dstFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

int stride = Encode(srcImage, dstFile, platform, format);
pxWritten = stride * srcImage.Height;
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("XNB Encoding Finished");

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

var fileInfo = XnbInfo.ReadBin(source);
var xnbPlatform = fileInfo.PlatformID;

if(!Enum.IsDefined(xnbPlatform) )
{
const string ERROR_INVALID_PLATFORM = "Invalid Platform identifier: {0:X2}";
TraceLogger.WriteError(string.Format(ERROR_INVALID_PLATFORM, xnbPlatform) );

return null;
}

byte xnbVer = fileInfo.Version;

if(xnbVer != VERSION)
TraceLogger.WriteWarn($"Unknown version: V{xnbVer} - Expected: V{VERSION}");

TraceLogger.WriteInfo($"Platform detected: {xnbPlatform}");

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
XnbFormat.Bgr565 => RGB565.Read(source, width, height),
XnbFormat.Bgra5551 => RGBA5551.Read(source, width, height),
XnbFormat.Bgra4444 => RGBA4444.Read(source, width, height),
XnbFormat.NormalizedByte2 => NormVector2D.Read(source, width, height),
XnbFormat.NormalizedByte4 => NormVector4D.Read(source, width, height),
_ => DXT5_RGBA.Read(source, width, height),
};

}

/** <summary> Decodes a XNB File as an Image. </summary>

<param name = "inputPath"> The Path where the XNB File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("XNB Decoding Started");

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

TraceLogger.WriteLine("XNB Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}

}

}