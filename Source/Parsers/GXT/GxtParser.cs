using System;
using System.Collections.Generic;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Parses GXT Files used in PSVita and PSP. </summary>

public static class GxtParser
{
/// <summary> The Header of a PackedTexture. </summary>

private const string HEADER = "GXT";

/// <summary> The Identifier of a PTX File. </summary>

private const byte FLAGS = 0x00;

/// <summary> The Version of a PTX File. </summary>

private const uint VERSION = 0x10000003;

// Encode single texture

private static int Encode(ref SKBitmap source, Stream target, GxtFormat format)
{

return format switch
{
GxtFormat.RGBA4444 => RGBA4444.Write(target, source),
GxtFormat.RGB555 => RGB555.Write(target, source),
GxtFormat.RGB565 => RGB565.Write(target, source),
GxtFormat.ARGB8888 => ARGB8888.Write(target, source),
GxtFormat.XRGB888 => XRGB888.Write(target, source),
GxtFormat.ARGB4444 => ARGB4444.Write(target, source),
GxtFormat.PVR_2BPP => PVRTC_2BPP_RGBA.Write(target, ref source),
GxtFormat.PVR_4BPP => PVRTC_4BPP_RGBA.Write(target, ref source),
// GxtFormat.PVRII_2BPP => PVRTCII_2BPP_RGBA.Write(target, source),
// GxtFormat.PVRII_4BPP => PVRTCII_4BPP_RGBA.Write(target, source),
GxtFormat.DXT1 => DXT1_RGBA.Write(target, source),
GxtFormat.DXT3 => DXT3_RGBA.Write(target, source),
GxtFormat.DXT5 => DXT5_RGBA_Morton.Write(target, source),
GxtFormat.RGB888 => RGB888.Write(target, source),
_ => ARGB1555.Write(target, source)
};

}

// Encode multiple textures

public static void Encode(Dictionary<string, GxtImageParams> images, Stream target)
{
int fileCount = images.Count;
GxtContainer fileInfo = new(fileCount);

TraceLogger.WriteLine("Step #1 - Write Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reserving space for metadata...");

int entryPoolLen = fileCount * 32;
int headerLen = entryPoolLen + 32; // Header(3) + Flags(1) + Container(28) + Entries

target.SetLength(headerLen);
target.Seek(0, SeekOrigin.Begin);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing header...");

target.WriteString(HEADER);
target.WriteByte(FLAGS);
fileInfo.WriteBin(target);

TraceLogger.WriteActionEnd();

long entriesStartPos = target.Position;

TraceLogger.WriteLine("Step #2 - Encode Images:");
TraceLogger.WriteLine();

List<GxtEntry> entries = new(fileCount);
long textureOffset = headerLen;

TraceLogger.Disable(); // Don't trace Encoding per Image (logger may grow so fast)

foreach(var img in images)
{
string path = img.Key;
var cfg = img.Value;

var bitmap = SKPlugin.FromFile(path);
target.Seek(textureOffset, SeekOrigin.Begin);

_ = Encode(ref bitmap, target, cfg.Format);
var size = (int)(target.Position - textureOffset);

int width = bitmap.Width;
int height = bitmap.Height;

GxtEntry entry = new(width, height, cfg.Format, textureOffset, size, cfg.Type);
entries.Add(entry);

bitmap.Dispose();

textureOffset += size;
}

TraceLogger.Enable(); // Logger is back!

TraceLogger.WriteLine("Step #3 - Write Entries:");
TraceLogger.WriteLine();

target.Seek(entriesStartPos, SeekOrigin.Begin);

TraceLogger.WriteActionStart("Writing entries...");

foreach(var entry in entries)
entry.WriteBin(target);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #4 - Finalize Info:");
TraceLogger.WriteLine();

long currentPos = target.Position;
var totalSize = (int)(target.Length - headerLen); // Don't include header bytes

TraceLogger.WriteActionStart("Updating header...");

target.Seek(16, SeekOrigin.Begin);
target.WriteInt32(totalSize);

TraceLogger.WriteActionEnd();

target.Seek(currentPos, SeekOrigin.Begin);
}

/** <summary> Encodes a single texture as a GXT File. </summary>

<param name = "inputPath"> The Image to Encode. </param>
<param name = "outputPath"> The Location where the Encoded GXT File will be Saved. </param> */

public static void EncodeFile(string inputPath, string outputPath, GxtFormat format)
{
TraceLogger.Init();
TraceLogger.WriteLine("GXT Encoding Started");

try
{
PathHelper.ChangeExtension(ref outputPath, ".gxt");
TraceLogger.WriteDebug($"{inputPath} --> {outputPath}");

TraceLogger.WriteActionStart("Opening files...");
using var dstFile = FileManager.OpenWrite(outputPath);

TraceLogger.WriteActionEnd();

Dictionary<string, GxtImageParams> map = new()
{
	{ inputPath, new(format) }
};

Encode(map, dstFile);
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Encode file");
}

TraceLogger.WriteLine("GXT Encoding Finished");

var outSize = FileManager.GetFileSize(outputPath);
TraceLogger.WriteInfo($"Output Size: {SizeT.FormatSize(outSize)}", false);
}

// Decode single texture

private static SKBitmap Decode(Stream source, int width, int height, GxtFormat format)
{

return format switch
{
GxtFormat.RGBA4444 => RGBA4444.Read(source, width, height),
GxtFormat.RGB555 => RGB555.Read(source, width, height),
GxtFormat.RGB565 => RGB565.Read(source, width, height),
GxtFormat.ARGB8888 => ARGB8888.Read(source, width, height),
GxtFormat.XRGB888 => XRGB888.Read(source, width, height),
GxtFormat.ARGB4444 => ARGB4444.Read(source, width, height),
GxtFormat.PVR_2BPP => PVRTC_2BPP_RGBA.Read(source, width, height),
GxtFormat.PVR_4BPP => PVRTC_4BPP_RGBA.Read(source, width, height),
// GxtFormat.PVRII_2BPP => PVRTCII_2BPP_RGBA.Read(source, width, height),
// GxtFormat.PVRII_4BPP => PVRTCII_4BPP_RGBA.Read(source, width, height),
GxtFormat.DXT1 => DXT1_RGBA.Read(source, width, height),
GxtFormat.DXT3 => DXT3_RGBA.Read(source, width, height),
GxtFormat.DXT5 => DXT5_RGBA_Morton.Read(source, width, height),
GxtFormat.RGB888 => RGB888.Read(source, width, height),
_ => ARGB1555.Read(source, width, height)
};

}

// Extract Images

public static void Decode(Stream source, string outputDir)
{
TraceLogger.WriteLine("Step #1 - Read Metadata:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reading header...");

using var hOwner = source.ReadString(HEADER.Length);
var inputHeader = hOwner.AsSpan();

if(!inputHeader.SequenceEqual(HEADER) )
{
TraceLogger.WriteError($"Invalid header: \"{inputHeader}\", expected: \"{HEADER}\"");

return;
}

byte inputFlags = source.ReadUInt8();

if(inputFlags != FLAGS)
{
const string ERROR_INVALID_FLAGS = "Invalid GXT identifier: {0:X2}, expected: {1:X2}";

TraceLogger.WriteError(string.Format(ERROR_INVALID_FLAGS, inputFlags, FLAGS) );
return;
}

var fileInfo = GxtContainer.ReadBin(source);
var fileVer = fileInfo.Version;

if(fileVer != VERSION)
TraceLogger.WriteWarn($"Unknown version: V{fileVer} - Expected: V{VERSION}");

TraceLogger.WriteActionEnd();

int fileCount = fileInfo.FileCount;
TraceLogger.WriteInfo($"Files embedded: {fileCount}");

TraceLogger.WriteLine("Step #2 - Read Entries:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Reading entries...");
var entries = GxtContainer.ReadEntries(source, fileCount);

TraceLogger.WriteActionEnd();

TraceLogger.WriteLine("Step #3 - Decode Images:");
TraceLogger.WriteLine();

Dictionary<string, int> ctr = new();

TraceLogger.Disable(); // Don't trace Textures Decoding (logger may grow so fast)

for(int i = 0; i < fileCount; i++)
{
var entry = entries[i];

GxtFormat format = entry.Format;

int width = entry.Width;
int height = entry.Height;

using SubStream texture = new(source, entry.Offset, entry.Size);
using var decImg = Decode(texture, width, height, format);

var dim = $"{width}x{height}";
var folder = Path.Combine(outputDir, entry.Type.ToString(), format.ToString(), dim);

if(!ctr.TryGetValue(folder, out int index) )
index = 0;

string outFile = Path.Combine(folder, $"{index:D3}.png");
decImg.Save(outFile);

ctr[folder] = index + 1;
}

TraceLogger.Enable(); // Logger is back!
}

/** <summary> Decodes a GXT File into multiple Images. </summary>

<param name = "inputFile"> The GXT File to Decode. </param>
<param name = "outputDir"> The Folder where to Save Decoded Images </param> */

public static void DecodeFile(string inputFile, string outputDir)
{
TraceLogger.Init();
TraceLogger.WriteLine("GXT Decoding Started");

try
{
TraceLogger.WriteDebug($"{inputFile} --> {outputDir}");

TraceLogger.WriteActionStart("Opening files...");
using var srcFile = FileManager.OpenRead(inputFile);

TraceLogger.WriteActionEnd();

Decode(srcFile, outputDir);
}

catch(Exception error)
{
TraceLogger.WriteError(error, "Failed to Decode file");
}

TraceLogger.WriteLine("GXT Decoding Finished");
}

}

}