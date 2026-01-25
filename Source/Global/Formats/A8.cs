using System.IO;
using SkiaSharp;

// Parse Alpha Images

public static class A8
{
// Decode Alpha Channel

private static TextureColor DecodeAlpha(byte a) => new(255, 255, 255, a);

// Read A8 Texture

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• A8 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode8(reader, width, height, DecodeAlpha);
}

// Encode Alpha Channel

private static byte EncodeAlpha(TextureColor color) => color.Alpha;

// Write A8 Texture

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• A8 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode8(writer, image, EncodeAlpha);
}

}