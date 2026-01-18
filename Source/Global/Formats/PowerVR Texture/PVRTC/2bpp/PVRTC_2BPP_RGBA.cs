using System.IO;
using SkiaSharp;

// Parse PVRTC Images in RGBA (2 bpp Mode)

public static class PVRTC_2BPP_RGBA
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• PVRTC-2BPP-RGBA Texture Decode:");
TraceLogger.WriteLine();

return PVRDecoder.Decode(reader, width, height, true, true);
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTC-2BPP-RGBA Texture Encode:");
TraceLogger.WriteLine();

return PVREncoder.Encode(writer, ref image, true, true);
}

}