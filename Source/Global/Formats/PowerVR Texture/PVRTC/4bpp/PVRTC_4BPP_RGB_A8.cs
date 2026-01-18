using System.IO;
using SkiaSharp;

// Parse PVR-RGB Images followed by A8 (4bpp)

public static class PVRTC_4BPP_RGB_A8
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• PVRTC-4BPP-RGB + A8 Texture Decode:");
TraceLogger.WriteLine();

var decImg = PVRDecoder.Decode(reader, width, height, false, false);
AlphaCodec.Decode8(reader, decImg);

return decImg;
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTC-4BPP-RGB + A8 Texture Encode:");
TraceLogger.WriteLine();

_ = PVREncoder.Encode(writer, ref image, false, false);
AlphaCodec.Encode8(writer, image);

return image.Width * 4;
}

}