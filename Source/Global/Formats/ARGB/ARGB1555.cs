using System.IO;
using SkiaSharp;

// Parse Images with ARGB 1555

public static class ARGB1555
{
// Get Color from ARGB1555

private static TextureColor DecodeColor(ushort flags)
{
int r5 = BitHelper.Extract(flags, 10, 5);
int g5 = BitHelper.Extract(flags, 5, 5);
int b5 = BitHelper.Extract(flags, 0, 5);

var r = BitHelper.ExpandTo8(r5, 5);
var g = BitHelper.ExpandTo8(g5, 5);
var b = BitHelper.ExpandTo8(b5, 5);
var a = (flags & 0x8000) != 0 ? (byte)255 : (byte)0;

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ARGB-1555 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

private static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int a1 = color.Alpha >= 128 ? 1 : 0;
int r5 = color.Red >> 3;
int g5 = color.Green >> 3;
int b5 = color.Blue >> 3;

packed = BitHelper.Insert(packed, a1, 15, 1);
packed = BitHelper.Insert(packed, r5, 10, 5);
packed = BitHelper.Insert(packed, g5, 5, 5);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ARGB-1555 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}