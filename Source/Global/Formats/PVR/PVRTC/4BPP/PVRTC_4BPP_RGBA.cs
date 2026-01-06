using System.IO;
using SkiaSharp;

// Parse PVRTC Images in RGBA (4 bpp Mode)

public static class PVRTC_4BPP_RGBA
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• PVRTC-4BPP-RGBA Texture Decode:");
TraceLogger.WriteLine();

return PVR.Decode4bpp(reader, width, height, true);
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTC-4BPP-RGBA Texture Encode:");
TraceLogger.WriteLine();

return PVR.Encode4bpp(writer, ref image, true);
}

}