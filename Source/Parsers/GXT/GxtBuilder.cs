using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Build GXT files from a directory of images (used in PSVita and PSP) </summary>

public static class GxtBuilder
{
// Read manifest

private static GxtManifest LoadManifest(string baseDir)
{
TraceLogger.WriteActionStart("Loading GXT manifest...");

string infoPath = Path.Combine(baseDir, GxtConstants.SRC_CONFIG);

using var cfgStream = FileManager.OpenRead(infoPath);
var cfg = JsonSerializer.DeserializeObject<GxtManifest>(cfgStream);

TraceLogger.WriteActionEnd();

return cfg;
}

// Encode single texture

private static NativeBuffer Encode(ref SKBitmap source, GxtFormat format) => format switch
{
GxtFormat.ARGB1555 => ARGB1555.Encode(source),
GxtFormat.RGBA4444 => RGBA4444.Encode(source),
GxtFormat.RGB555 => RGB555.Encode(source),
GxtFormat.RGB565 => RGB565.Encode(source),
GxtFormat.ARGB8888 => ARGB8888.Encode(source),
GxtFormat.XRGB888 => XRGB888.Encode(source),
GxtFormat.ARGB4444 => ARGB4444.Encode(source),
GxtFormat.PVR_2BPP => PVRTC_2BPP_RGBA.Encode(ref source),
GxtFormat.PVR_4BPP => PVRTC_4BPP_RGBA.Encode(ref source),
GxtFormat.PVRII_2BPP => PVRTCII_2BPP_RGBA.Encode(ref source),
GxtFormat.PVRII_4BPP => PVRTCII_4BPP_RGBA.Encode(ref source),
GxtFormat.DXT1 => DXT1_RGBA.Encode(source),
GxtFormat.DXT3 => DXT3_RGBA.Encode(source),
GxtFormat.DXT5 => DXT5_RGBA_Morton.Encode(ref source),
GxtFormat.RGB888 => RGB888.Encode(source),
_ => throw new NotSupportedException(GxtConstants.FormatMsg(format) )
};

// Encode images

private static GxtEntry[] EncodeImages(string inputDir,
                                       long startPos,
                                       GxtManifest images,
                                       out NativeBuffer[] encodedImages)
{
TraceLogger.WriteActionStart("Encoding images...");

int fileCount = images.Count;

GxtEntry[] entries = new GxtEntry[fileCount];
encodedImages = new NativeBuffer[fileCount];

string imgDir = Path.Combine(inputDir, GxtConstants.SRC_IMAGES);
long localOffset = 0;

int i = 0;

foreach(var img in images)
{
var cfg = img.Value;

string fullPath = Path.Combine(imgDir, img.Key);
var bitmap = SKPlugin.FromFile(fullPath);

var encoded = Encode(ref bitmap, cfg.Format);
var size = (int)encoded.Size;

encodedImages[i] = encoded;

long textureOffset = startPos + localOffset;

entries[i] = new(bitmap.Width, bitmap.Height, textureOffset, size, cfg);
localOffset += size;

i++;

bitmap.Dispose();
}

TraceLogger.WriteActionEnd();

return entries;
}

// Write header

private static void WriteHeader(int fileCount, int fileSize, Stream target)
{
TraceLogger.WriteActionStart("Writing header...");

target.WriteUInt32(GxtConstants.MAGIC, Endianness.BigEndian);

GxtContainer fileInfo = new(fileCount, fileSize);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();
}

// Write entries

private static void WriteEntries(GxtEntry[] entries, Stream target)
{
TraceLogger.WriteActionStart("Writing entries...");

foreach(var entry in entries)
entry.WriteBin(target);

TraceLogger.WriteActionEnd();
}

// Dump image blob

private static void WriteImages(NativeBuffer[] encodedImages, Stream target)
{
TraceLogger.WriteActionStart("Writing raw images...");

foreach(var img in encodedImages)
{
target.Write(img.GetView() );

img.Dispose();
}

TraceLogger.WriteActionEnd();
}

// Build GXT stream

public static void Build(string inputDir, Stream target)
{
TraceLogger.WriteStep(1, "Load Manifest");

var images = LoadManifest(inputDir);
int fileCount = images.Count;

TraceLogger.WriteStep(2, "Encode Images");

int entryPoolLen = fileCount * 32;
int headerLen = 32 + entryPoolLen;

long textureStartPos = headerLen;
var entries = EncodeImages(inputDir, textureStartPos, images, out var encodedImages);

int textureDataSize = 0;

foreach(var image in encodedImages)
textureDataSize += (int)image.Size;

TraceLogger.WriteStep(3, "Write Metadata");

WriteHeader(fileCount, textureDataSize, target);

TraceLogger.WriteStep(4, "Write Entries");

WriteEntries(entries, target);

TraceLogger.WriteStep(5, "Write Images Blob");

WriteImages(encodedImages, target);
}

// Build internal

private static void BuildInternal(string inputDir, string outputFile)
{
PathHelper.ChangeExtension(ref outputFile, ".gxt");

using var dstFile = FileManager.OpenWrite(outputFile);

Build(inputDir, dstFile);
}

/** <summary> Builds a GXT file from a single image </summary>

<param name = "inputDir"> The Dir to pack. </param>
<param name = "outputFile"> The Location where the GXT File will be Saved. </param> */

public static void BuildFile(string inputDir, string outputFile)
{

TraceExecutor.Run("GXT Build Started",
                  ctx => BuildInternal(inputDir, outputFile),
				  ("InputFolder", inputDir),
			      ("OutputFile", outputFile)

);	

}

}

}