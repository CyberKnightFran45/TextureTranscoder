using System;
using System.IO;
using SkiaSharp;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Parses XNB Images (from Microsoft) </summary>

public static class XnbParser
{
// Reader info serializer

private static readonly XnbReaderInfoSerializer readerInfoSerializer = new();

/// <summary> File identifier </summary>

private const string MAGIC = "XNB";

/// <summary> File version </summary>

private const byte VERSION = 5;

// Xnb Texture2DReader AssemblyName (from Microsoft Xna Framework)

private const string XNB_TEXTURE_2D = "Microsoft.Xna.Framework.Content.Texture2DReader, " +
                                      "Microsoft.Xna.Framework.Graphics, " +
                                      "Version=4.0.0.0, " +
                                      "Culture=neutral, " +
                                      "PublicKeyToken=842cf8be1de50553";

// Default reader type

private static readonly XnbReaderInfo DEFAULT_READER_TYPE = GetDefaultTypeReader();

// Get default type reader

private static XnbReaderInfo GetDefaultTypeReader()
{
XnbReaderEntry[] entries = [ new(XNB_TEXTURE_2D, 0) ];

return new(entries, 0, 1);
}

// Get error msg

private static string FormatMsg(XnbFormat format) => $"Unsupported format: {format} (0x{(uint)format:X8})";

// Write XNB header

private static void WriteXnbHeader(Stream writer, XnbPlatform platform)
{
writer.WriteString(MAGIC);

XnbHeader header = new(platform);
header.WriteBin(writer);

readerInfoSerializer.WriteBin(writer, DEFAULT_READER_TYPE);
}

// Write Texture info

private static void WriteTexInfo(Stream writer, int width, int height, XnbFormat format, int mipSize)
{
XnbTextureInfo info = new(width, height, format, mipSize);

info.WriteBin(writer);
}

// Encode raw image (in RAM)

private static NativeBuffer EncodeRaw(SKBitmap source, XnbFormat format) => format switch
{
XnbFormat.Color => DXT5_RGBA.Encode(source),
XnbFormat.Bgr565 => RGB565.Encode(source),
XnbFormat.Bgra5551 => RGBA5551.Encode(source),
XnbFormat.Bgra4444 => RGBA4444.Encode(source),
XnbFormat.NormalizedByte2 => NormVector2D.Encode(source),
XnbFormat.NormalizedByte4 => NormVector4D.Encode(source),
_ => throw new NotSupportedException(FormatMsg(format) )
};

// Encode XNB (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, XnbPlatform platform, XnbFormat format)
{
using var raw = EncodeRaw(source, format);

var mipSize = (int)raw.Size;

WriteXnbHeader(target, platform);
WriteTexInfo(target, source.Width, source.Height, format, mipSize);

target.Write(raw.GetView() ); // Dump texture bytes to final stream
}

// Encode XNB (stream based)

public static void Encode(Stream input, Stream output, XnbPlatform platform, XnbFormat format)
{
using var bitmap = SKPlugin.FromStream(input);

Encode(bitmap, output, platform, format);
}

// Encode XNB (stream to buffer)

public static NativeBuffer Encode(Stream src, XnbPlatform platform, XnbFormat format)
{
using MemoryStream dest = new();

Encode(src, dest, platform, format);
dest.Seek(0, SeekOrigin.Begin);

return dest.ReadPtr();
}

/** <summary> Encodes a XNB file. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded XNB File will be Saved. </param> **/

public static void EncodeFile(string inputPath, string outputPath, XnbPlatform platform, XnbFormat format)
{

TraceImgParser.Encode("XNB Encoding",
                      inputPath,
                      outputPath,
                      ".xnb",
                      (input, output) => Encode(input, output, platform, format),
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath),
                      ("Platform", platform),
                      ("Format", format)

);
					  
}

// Read XNB header

private static XnbHeader ReadXnbHeader(Stream reader)
{
using var hOwner = reader.ReadString(3);

var flags = hOwner.AsSpan();

if(!flags.SequenceEqual(MAGIC) )
throw new Exception($"Invalid header: \"{flags}\", expected: \"{MAGIC}\"");

var header = XnbHeader.ReadBin(reader);

var xnbPlatform = header.PlatformID;

if(!Enum.IsDefined(xnbPlatform) )
throw new Exception($"Invalid Platform identifier: {(byte)xnbPlatform:X2}");

byte xnbVer = header.Version;

if(xnbVer != VERSION)
TraceLogger.WriteWarn($"Unknown version: V{xnbVer} - Expected: V{VERSION}");

var readerInfo = readerInfoSerializer.ReadBin(reader);

string readerName = readerInfo.TypeReaders[0].ReaderName;

if(!readerName.Equals(XNB_TEXTURE_2D) )
throw new NotImplementedException($"Non-implemented reader type: {readerName}");

return header;
}

// Decode raw tex

private static SKBitmap DecodeRaw(ReadOnlySpan<byte> source, XnbFormat format, int width, int height)
{

return format switch
{
XnbFormat.Color => DXT5_RGBA.Decode(source, width, height),
XnbFormat.Bgr565 => RGB565.Decode(source, width, height),
XnbFormat.Bgra5551 => RGBA5551.Decode(source, width, height),
XnbFormat.Bgra4444 => RGBA4444.Decode(source, width, height),
XnbFormat.NormalizedByte2 => NormVector2D.Decode(source, width, height),
XnbFormat.NormalizedByte4 => NormVector4D.Decode(source, width, height),
_ => throw new NotSupportedException(FormatMsg(format) )
};

}

// Decode XNB (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
var header = ReadXnbHeader(source);
var info = XnbTextureInfo.ReadBin(source);

using var texture = source.ReadPtr();

return DecodeRaw(texture.GetView(), info.Format, info.Width, info.Height);
}

// Decode XNB (stream based)

public static void Decode(Stream input, Stream output)
{
using var xnb = input.ReadPtr();

Decode(xnb, output);
}

// Decode XNB (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
using var srcStream = source.GetStream();

return Decode(srcStream);
}

// Decode XNB (buffer to stream)

public static void Decode(NativeBuffer source, Stream output)
{
using var image = Decode(source);

image.Save(output);
}

/** <summary> Decodes a XNB file. </summary>

<param name = "inputPath"> The Path where the XNB File to Decode is Located. </param>
<param name = "outputPath"> The Location where the Decoded Image will be Saved. </param> **/

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("XNB Decoding",
                      inputPath,
                      outputPath,
                      Decode,
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath)

);
					  
}

}

}