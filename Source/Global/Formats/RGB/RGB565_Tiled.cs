using System.IO;
using SkiaSharp;

// Parse Image in the RGB565 format (32x32 Tiled)

public static unsafe class RGB565_Tiled
{
// Tile Size

private const int TILE_SIZE = 32;
 

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• Tiled-RGB-565 Texture Decode:");
TraceLogger.WriteLine();

return RGB.DecodeTile16(reader, width, height, TILE_SIZE, RGB565.DecodeColor);
}

// Write pixels

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• Tiled-RGB-565 Texture Encode:");
TraceLogger.WriteLine();

return RGB.EncodeTile16(writer, image, TILE_SIZE, RGB565.EncodeColor);
}

}