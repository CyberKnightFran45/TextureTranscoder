using System.IO;
using SkiaSharp;

// Parse Image in the RGBA4444 format (32x32 Tiled)

public static class RGBA4444_Tiled
{
// Tile Size

private const int TILE_SIZE = 32;

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• Tiled-RGBA-4444 Texture Decode:");
TraceLogger.WriteLine();

return RGB.DecodeTile16(reader, width, height, TILE_SIZE, RGBA4444.DecodeColor);
}

// Write pixels

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• Tiled-RGBA-4444 Texture Encode:");
TraceLogger.WriteLine();

return RGB.EncodeTile16(writer, image, TILE_SIZE, RGBA4444.EncodeColor);
}

}