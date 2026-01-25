using System.IO;
using SkiaSharp;

public static class XRGB888
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• XRGB-888 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode24(reader, width, height, RGB888.DecodeColor);
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• XRGB-888 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode24(writer, image, RGB888.EncodeColor);
}

}