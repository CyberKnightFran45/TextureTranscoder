using System.IO;
using SkiaSharp;

// Parse PVR2-RGB Images followed by A8 (4bpp)

public static class PVRTCII_4BPP_RGB_A8
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• PVRTCII-4BPP-RGB + A8 Texture Decode:");
TraceLogger.WriteLine();

var decImg = PVRII_4BPP_Decoder.Decode(reader, width, height, false);
AlphaCodec.Decode8(reader, decImg);

return decImg;
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTCII-4BPP-RGB + A8 Texture Encode:");
TraceLogger.WriteLine();

_ = PVRII_4BPP_Encoder.Encode(writer, ref image, false);
AlphaCodec.Encode8(writer, image);

return image.Width * 4;
}

}