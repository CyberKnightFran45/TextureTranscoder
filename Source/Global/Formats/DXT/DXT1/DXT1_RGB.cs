using System.IO;
using SkiaSharp;

// Parse DXT1 Images in RGB Order

public static class DXT1_RGB
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• DXT1-RGB Texture Decode:");
TraceLogger.WriteLine();

return DXT.Decode(reader, width, height);
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• DXT1-RGB Texture Encode:");
TraceLogger.WriteLine();

return DXT.Encode(writer, image, false);
}

}