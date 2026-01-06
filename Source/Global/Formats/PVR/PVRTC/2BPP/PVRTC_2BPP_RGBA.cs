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

return PVR.Decode2bpp(reader, width, height);
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTC-2BPP-RGBA Texture Encode:");
TraceLogger.WriteLine();

return PVR.Encode2bpp(writer, ref image);
}

}