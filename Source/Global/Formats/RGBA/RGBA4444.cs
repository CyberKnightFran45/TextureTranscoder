using System.IO;
using SkiaSharp;

public static class RGBA4444
{
// Get Color from RGBA4444

internal static TextureColor DecodeColor(ushort flags)
{
int r4 = BitHelper.Extract(flags, 12, 4);
int g4 = BitHelper.Extract(flags, 8, 4);
int b4 = BitHelper.Extract(flags, 4, 4);
int a4 = BitHelper.Extract(flags, 0, 4);

byte r = BitHelper.ExpandTo8(r4, 4);
byte g = BitHelper.ExpandTo8(g4, 4);
byte b = BitHelper.ExpandTo8(b4, 4);
byte a = BitHelper.ExpandTo8(a4, 4);

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• RGBA-4444 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int r4 = color.Red >> 4;
int g4 = color.Green >> 4;
int b4 = color.Blue >> 4;
int a4 = color.Alpha >> 4;

packed = BitHelper.Insert(packed, r4, 12, 4);
packed = BitHelper.Insert(packed, g4, 8, 4);
packed = BitHelper.Insert(packed, b4, 4, 4);
packed = BitHelper.Insert(packed, a4, 0, 4);

return (ushort)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• RGBA-4444 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}