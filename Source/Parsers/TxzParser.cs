using System.IO;
using System.IO.Compression;
using BlossomLib.Modules.Compression;
using SkiaSharp;
using TextureTranscoder.Parsers.UTexture;

namespace TextureTranscoder.Parsers
{
/// <summary> Decompress and Compress TXZ Images (same as U-Tex but with ZLib) </summary>

public static class TxzParser
{
// Compress U-Tex image

private static void CompressImg(NativeBuffer input, Stream output, CompressionLevel level)
{
using var inputStream = input.GetStream();

ZLibCompressor.CompressStream(inputStream, output, level);
}

// Encode TXZ (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, UTexFormat format, CompressionLevel level)
{
using var raw = UTexParser.Encode(source, format);

CompressImg(raw, target, level);
}

// Encode TXZ (stream based)

public static void Encode(Stream input, Stream output, UTexFormat format, CompressionLevel level)
{
using var bitmap = SKPlugin.FromStream(input);

Encode(bitmap, output, format, level);
}

// Encode TXZ (stream to buffer)

public static NativeBuffer Encode(Stream src, UTexFormat format, CompressionLevel level)
{
using var bitmap = SKPlugin.FromStream(src);

using MemoryStream dest = new();
Encode(bitmap, dest, format, level);

dest.Seek(0, SeekOrigin.Begin);

return dest.ReadPtr();
}

/** <summary> Encodes an Image as a TXZ File. </summary>

<param name = "inputPath"> The Path where the Image to Encode is Located. </param>
<param name = "outputPath"> The Location where the Encoded File will be Saved. </param> **/

public static void EncodeFile(string inputPath, string outputPath, UTexFormat format, CompressionLevel level)
{

TraceImgParser.Encode("TXZ Encoding",
                      inputPath,
                      outputPath,
                      ".txz",
                      (input, output) => Encode(input, output, format, level),
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath),
                      ("Format", format),
                      ("CompressionLevel", level)
					  
);
			  
}

// Decompress U-Tex image

private static NativeBuffer DecompressImg(Stream source)
{
int zlChunks = MemoryManager.GetBufferSize(source);

using MemoryStream rawStream = new(zlChunks);
ZLibCompressor.DecompressStream(source, rawStream);

rawStream.Seek(0, SeekOrigin.Begin);

return rawStream.ReadPtr();
}

// Decode U-Tex (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
using var srcStream = source.GetStream();
using var raw = DecompressImg(srcStream);

return UTexParser.Decode(raw);
}

// Decode U-Tex (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var raw = DecompressImg(source);

return UTexParser.Decode(raw);
}

// Decode U-Tex (buffer to stream)

public static void Decode(NativeBuffer source, Stream output)
{
using var image = Decode(source);

image.Save(output);
}

// Decode U-Tex (stream based)

public static void Decode(Stream input, Stream output)
{
using var image = Decode(input);

image.Save(output);
}

/** <summary> Decodes a TXZ File as an Image. </summary>

<param name = "inputPath"> The Path to the File to Decode. </param>
<param name = "outputPath"> The Location where to Save the Decoded file. </param> **/

public static void DecodeFile(string inputPath, string outputPath)
{

TraceImgParser.Decode("TXZ Decoding",
                      inputPath,
                      outputPath,
                      Decode,
                      ("InputPath", inputPath),
                      ("OutputPath", outputPath)
			  
);

}

}

}