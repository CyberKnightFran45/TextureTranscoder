using System.IO;
using System.IO.Compression;
using SkiaSharp;
using TextureTranscoder.Parsers.UTexture;

namespace TextureTranscoder.Parsers
{
/// <summary> Decompress and Compress TXZ Images (same as U-Tex but with ZLib) </summary>

public static class TxzParser
{
// Encode TXZ (bitmap to stream)

public static void Encode(SKBitmap source, Stream target, UTexFormat format, CompressionLevel level)
{
using var raw = UTexParser.Encode(source, format);

ZLibBufferCompressor.Compress(raw, target, level);
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

// Decode U-Tex (buffer to bitmap)

public static SKBitmap Decode(NativeBuffer source)
{
using var srcStream = source.GetStream();

return Decode(srcStream);
}

// Decode U-Tex (stream to bitmap)

public static SKBitmap Decode(Stream source)
{
using var raw = ZLibBufferCompressor.Decompress(source);

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