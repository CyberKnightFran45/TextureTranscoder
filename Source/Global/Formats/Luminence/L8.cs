using System.IO;
using SkiaSharp;

// Parse Luminance Images

public static class L8
{
// Decode Luminance

private static TextureColor DecodeLuminance(byte l) => new(l, l, l, 255);

// Read L8 Texture

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• L8 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode8(reader, width, height, DecodeLuminance);
}

// Encode Luminance

internal static byte EncodeLuminance(TextureColor color)
{
double lumi = color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114;

return (byte)lumi;
}

// Write L8 Texture

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• L8 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode8(writer, image, EncodeLuminance);
}

}