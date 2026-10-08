using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Unpacks a GXT container into a directory of images (used in PSVita and PSP) </summary>

public static class GxtUnpacker
{
// Read header

private static GxtContainer ReadInfo(Stream source)
{
TraceLogger.WriteActionStart("Reading header...");

uint flags = source.ReadUInt32(Endianness.BigEndian);

if(flags != GxtConstants.MAGIC)
throw new Exception($"Invalid file identifier: 0x{flags:X8}, expected: 0x{GxtConstants.MAGIC:X8}");

var fileInfo = GxtContainer.ReadBin(source);
var fileVer = fileInfo.Version;

if(fileVer != GxtConstants.VERSION)
TraceLogger.WriteWarn($"Unknown version: 0x{fileVer:X8} - Expected: 0x{GxtConstants.VERSION:X8}");

TraceLogger.WriteActionEnd();

return fileInfo;
}

// Read entries

private static GxtEntry[] ReadEntries(Stream reader, int count)
{
TraceLogger.WriteActionStart("Reading entries...");
var entries = GxtContainer.ReadEntries(reader, count);

TraceLogger.WriteActionEnd();

return entries;
}

// Decode single texture

private static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, GxtFormat format)

{

return format switch
{
GxtFormat.ARGB1555 => ARGB1555.Decode(source, width, height),
GxtFormat.RGBA4444 => RGBA4444.Decode(source, width, height),
GxtFormat.RGB555 => RGB555.Decode(source, width, height),
GxtFormat.RGB565 => RGB565.Decode(source, width, height),
GxtFormat.ARGB8888 => ARGB8888.Decode(source, width, height),
GxtFormat.XRGB888 => XRGB888.Decode(source, width, height),
GxtFormat.ARGB4444 => ARGB4444.Decode(source, width, height),
GxtFormat.PVR_2BPP => PVRTC_2BPP_RGBA.Decode(source, width, height),
GxtFormat.PVR_4BPP => PVRTC_4BPP_RGBA.Decode(source, width, height),
GxtFormat.PVRII_2BPP => PVRTCII_2BPP_RGBA.Decode(source, width, height),
GxtFormat.PVRII_4BPP => PVRTCII_4BPP_RGBA.Decode(source, width, height),
GxtFormat.DXT1 => DXT1_RGBA.Decode(source, width, height),
GxtFormat.DXT3 => DXT3_RGBA.Decode(source, width, height),
GxtFormat.DXT5 => DXT5_RGBA_Morton.Decode(source, width, height),
GxtFormat.RGB888 => RGB888.Decode(source, width, height),
_ => throw new NotSupportedException(GxtConstants.FormatMsg(format) )
};

}

// Decode images

private static GxtManifest DecodeImages(Stream source, string outputDir, GxtEntry[] entries)
{
TraceLogger.WriteActionStart("Decoding images...");

GxtManifest manifest = new();

string imgDir = Path.Combine(outputDir, GxtConstants.SRC_IMAGES);
Directory.CreateDirectory(imgDir);

int fileCount = entries.Length;

for(int i = 0; i < fileCount; i++)
{
var entry = entries[i];

int width = entry.Width;
int height = entry.Height;

var format = entry.Format;

source.Seek(entry.Offset, SeekOrigin.Begin);

using var texture = source.ReadPtr(entry.Size);
using var decImg = Decode(texture.AsSpan(), width, height, format);

GxtImageParams cfg = new(entry.PaletteIndex, entry.Flags, entry.Type, format, entry.MipCount);

string fileName = $"{i:D4}.png";
string outFile = Path.Combine(imgDir, fileName);

decImg.Save(outFile);
manifest.Add(fileName, cfg);
}

TraceLogger.WriteActionEnd();

return manifest;
}

// Save GXT manifest

private static void SaveManifest(string outputDir, GxtManifest manifest)
{
string infoPath = Path.Combine(outputDir, GxtConstants.SRC_CONFIG);

TraceLogger.WriteActionStart("Saving GXT manifest...");

using var cfgStream = FileManager.OpenWrite(infoPath);
JsonSerializer.SerializeObject(manifest, cfgStream);

TraceLogger.WriteActionEnd();
}

// Unpack GXT stream

public static void Unpack(Stream source, string outputDir)
{
TraceLogger.WriteStep(1, "Read Metadata");

var info = ReadInfo(source);
int fileCount = info.FileCount;

TraceLogger.WriteInfo($"Files embedded: {fileCount}");

TraceLogger.WriteStep(2, "Read Entries");

var entries = ReadEntries(source, fileCount);

TraceLogger.WriteStep(3, "Decode Images");

var manifest = DecodeImages(source, outputDir, entries);

TraceLogger.WriteStep(4, "Save Manifest");

SaveManifest(outputDir, manifest);
}

// Unpack internal

private static void UnpackInternal(string inputFile, string outputDir)
{
using var srcFile = FileManager.OpenRead(inputFile);

Unpack(srcFile, outputDir);
}

/** <summary> Unpacks GXT File </summary>

<param name = "inputFile"> The GXT File to Decode. </param>
<param name = "outputDir"> The Folder where to Save Decoded Images </param> */

public static void UnpackFile(string inputFile, string outputDir)
{

TraceExecutor.Run("GXT Unpack Started",
                  ctx => UnpackInternal(inputFile, outputDir),
				  ("InputFile", inputFile),
			      ("OutputFolder", outputDir)

);

}

}

}