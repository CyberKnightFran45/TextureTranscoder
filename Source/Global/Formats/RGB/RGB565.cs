using System.IO;
using SkiaSharp;

public static class RGB565
{
// Get Color from RGB565

internal static TextureColor DecodeColor(ushort flags)
{
int r5 = BitHelper.Extract(flags, 11, 5);
int g6 = BitHelper.Extract(flags, 5, 6);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g6, 6);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• RGB-565 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int r5 = color.Red >> 3;
int g6 = color.Green >> 2;
int b5 = color.Blue >> 3;

packed = BitHelper.Insert(packed, r5, 11, 5);
packed = BitHelper.Insert(packed, g6, 5, 6);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• RGB-565 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}