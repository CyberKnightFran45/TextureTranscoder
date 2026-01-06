using System.IO;
using SkiaSharp;

// Parse Image in the ARGB format (4 bits per Color)

public static class ARGB4444
{
// Get Color from Binary

private static TextureColor DecodeColor(ushort flags)
{
int a4 = BitHelper.Extract(flags, 12, 4);
int r4 = BitHelper.Extract(flags, 8, 4);
int g4 = BitHelper.Extract(flags, 4, 4);
int b4 = BitHelper.Extract(flags, 0, 4);

byte r = BitHelper.ExpandTo8(r4, 4);
byte g = BitHelper.ExpandTo8(g4, 4);
byte b = BitHelper.ExpandTo8(b4, 4);
byte a = BitHelper.ExpandTo8(a4, 4);

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ARGB-4444 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

private static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int a4 = color.Alpha >> 4;
int r4 = color.Red >> 4;
int g4 = color.Green >> 4;
int b4 = color.Blue >> 4;

packed = BitHelper.Insert(packed, a4, 12, 4);
packed = BitHelper.Insert(packed, r4, 8, 4);
packed = BitHelper.Insert(packed, g4, 4, 4);
packed = BitHelper.Insert(packed, b4, 0, 4);

return (ushort)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ARGB-4444 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}