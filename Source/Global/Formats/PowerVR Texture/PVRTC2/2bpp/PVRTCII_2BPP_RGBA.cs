using System.IO;
using SkiaSharp;

// Parse PVRTCII Images in RGBA (2 bpp Mode)

public static class PVRTCII_2BPP_RGBA
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• PVRTCII-2BPP-RGBA Texture Decode:");
TraceLogger.WriteLine();

return PVRII_2BPP_Decoder.Decode(reader, width, height, true);
}

// Write pixels to Bitmap

public static int Write(Stream writer, ref SKBitmap image)
{
TraceLogger.WriteLine("• PVRTCII-2BPP-RGBA Texture Encode:");
TraceLogger.WriteLine();

return PVRII_2BPP_Encoder.Encode(writer, ref image, true);
}

}