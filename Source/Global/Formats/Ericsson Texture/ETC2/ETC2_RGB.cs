using System;
using System.IO;
using SkiaSharp;

// ETC2 RGB compression (no alpha channel)

public static class ETC2_RGB
{
// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ETC2-RGB Texture Encode:");
TraceLogger.WriteLine();

return ETCBase.Encode(writer, image, ETC2Encoder.EncodeBlock);
}

// Read bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ETC2-RGB Texture Decode:");
TraceLogger.WriteLine();

return ETCBase.Decode(reader, width, height, ETC2Decoder.DecodeBlock);
}

}