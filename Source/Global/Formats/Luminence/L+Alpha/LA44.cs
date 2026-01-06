using System.IO;
using SkiaSharp;

// Parse Luminance + Alpha Images (8 bits)

public static class LA44
{
// Decode Contrast

private static TextureColor DecodeContrast(byte flags)
{
byte l = BitHelper.ExtractAndExpandTo8(flags, 4, 4);
byte a = BitHelper.ExtractAndExpandTo8(flags, 0, 4);

return new(l, l, l, a);
}

// Read LA44 Texture

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• LA44 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode8(reader, width, height, DecodeContrast);
}

// Encode Contrast

private static byte EncodeContrast(TextureColor color)
{
int packed = 0;

int l4 = L8.EncodeLuminance(color) >> 4;
int a4 = color.Alpha >> 4;

packed = BitHelper.Insert(packed, l4, 4, 4);
packed = BitHelper.Insert(packed, a4, 0, 4);

return (byte)packed;
}

// Write LA44 Texture

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• LA44 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode8(writer, image, EncodeContrast);
}

}