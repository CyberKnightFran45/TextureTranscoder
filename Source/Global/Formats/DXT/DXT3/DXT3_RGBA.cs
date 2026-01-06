using System;
using System.IO;
using SkiaSharp;

// Parse DXT3 Images in RGBA Order

public static class DXT3_RGBA
{
// Decode Color (add alpha)

private static void DecodeColor(byte alpha, ref TextureColor color) => color.Alpha = alpha;

// Decode Block

internal static void DecodeBlock(ReadOnlySpan<byte> alpha, Span<TextureColor> block)
{

for(int i = 0; i < 16; i++)
DecodeColor(alpha[i], ref block[i]);

}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• DXT3-RGBA Texture Decode:");
TraceLogger.WriteLine();

return DXT.Decode(reader, width, height, DecodeBlock, DXT2_RGBA.DecodeAlpha);
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• DXT3-RGBA Texture Encode:");
TraceLogger.WriteLine();

return DXT.Encode(writer, image, false, null, DXT2_RGBA.EncodeAlpha);
}

}