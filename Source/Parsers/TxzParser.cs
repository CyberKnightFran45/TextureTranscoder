using System;
using System.IO;
using SkiaSharp;
using TextureTranscoder.Parsers.UTexture;
using BlossomLib.Modules.Compression;

namespace TextureTranscoder.Parsers
{
/// <summary> Decompress and Compress TXZ Images (same as U-Tex but with ZLIB). </summary>

public static class TxzParser
{
// Get TXZ Stream

public static int Encode(SKBitmap source, Stream target, UTexFormat format)
{
using ChunkedMemoryStream rawStream = new();

int stride = UTexParser.Encode(source, rawStream, format);
rawStream.Seek(0, SeekOrigin.Begin);

TraceLogger.WriteLine("Step #3 - Compress ZLib:");
TraceLogger.WriteLine();

string fileSize = SizeT.FormatSize(rawStream.Length);

TraceLogger.WriteActionStart($"Compressing data... ({fileSize})");
ZLibCompressor.CompressStream(rawStream, target, default);

TraceLogger.WriteActionEnd();

return stride;
}

/** <summary> Encodes an Image as a TXZ File. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded File will be Saved. </param>  */

public static void EncodeFile(string inputPath, string outputPath, UTexFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("TXZ Encoding Started");

int pxWritten = 0;

try
{
PathHelper.ChangeExtension(ref outputPath, ".txz");
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

TraceLogger.WriteLine("TXZ Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)} ({pxWritten} px)", false);
}

// Get Plain Image

public static SKBitmap Decode(Stream source)
{
int zlChunks = MemoryManager.GetBufferSize(source);
using ChunkedMemoryStream rawStream = new(zlChunks);

string fileSize = SizeT.FormatSize(source.Length);
TraceLogger.WriteActionStart($"Decompressing data... ({fileSize})");

ZLibCompressor.DecompressStream(source, rawStream);
rawStream.Seek(0, SeekOrigin.Begin);

TraceLogger.WriteActionEnd();

return UTexParser.Decode(rawStream);
}

/** <summary> Decodes a TXZ File as an Image. </summary>

<param name = "inputPath"> The Path to the File to Decode. </param>
<param name = "outputPath"> The Location where to Save the Decoded file. </param> */

public static void DecodeFile(string inputPath, string outputPath)
{
TraceLogger.Init();
TraceLogger.WriteLine("TXZ Decoding Started");

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

TraceLogger.WriteLine("TXZ Decoding Finished");

TraceLogger.WriteInfo($"Image dimensions: {width}x{height}", false);
}
   
}

}