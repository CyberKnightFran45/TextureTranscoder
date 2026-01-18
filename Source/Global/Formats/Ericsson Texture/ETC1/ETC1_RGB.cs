using System.IO;
using SkiaSharp;

// Parse ETC1 Images in RGB Mode

public static class ETC1_RGB
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ETC1-RGB Texture Decode:");
TraceLogger.WriteLine();

return ETC1Decoder.Decode(reader, width, height);
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ETC1-RGB Texture Encode:");
TraceLogger.WriteLine();

return ETC1Encoder.Encode(writer, image);
}

}