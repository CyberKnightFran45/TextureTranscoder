using System.IO;
using SkiaSharp;

// Parse ETC1-RGB Images followed by A8

public static class ETC1_RGB_A8
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ETC1-RGB + A8 Texture Decode:");
TraceLogger.WriteLine();

var decImg = ETC1Decoder.Decode(reader, width, height);
AlphaCodec.Decode8(reader, decImg);

return decImg;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ETC1-RGB + A8 Texture Encode:");
TraceLogger.WriteLine();

_ = ETC1Encoder.Encode(writer, image);
AlphaCodec.Encode8(writer, image);

return image.Width * 4;
}

}