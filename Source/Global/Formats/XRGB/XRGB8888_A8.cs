using System.IO;
using SkiaSharp;

// Parse Images as XRGB8888 followed by an Alpha Chanel (8-bits)

public static class XRGB8888_A8
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• XRGB-8888 + A8 Texture Decode:");
TraceLogger.WriteLine();

var decImg = RGB.Decode32(reader, width, height, XRGB8888.DecodeColor);
AlphaCodec.Decode8(reader, decImg);

return decImg;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• XRGB-8888 + A8 Texture Encode:");
TraceLogger.WriteLine();

_ = RGB.Encode32(writer, image, XRGB8888.EncodeColor);
AlphaCodec.Encode8(writer, image);

return image.Width * 5;
}

}