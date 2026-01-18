using System.IO;
using SkiaSharp;

public static class RGBA5551
{
// Get Color from RGBA551

internal static TextureColor DecodeColor(ushort flags)
{
int r5 = BitHelper.Extract(flags, 11, 5);
int g5 = BitHelper.Extract(flags, 6, 5);
int b5 = BitHelper.Extract(flags, 1, 5);

var r = BitHelper.ExpandTo8(r5, 5);
var g = BitHelper.ExpandTo8(g5, 5);
var b = BitHelper.ExpandTo8(b5, 5);
var a = (flags & 0x1) != 0 ? (byte)255 : (byte)0;

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• RGBA-5551 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int r5 = color.Red >> 3;
int g5 = color.Green >> 3;
int b5 = color.Blue >> 3;
int a1 = color.Alpha >= 128 ? 1 : 0;

packed = BitHelper.Insert(packed, r5, 11, 5);
packed = BitHelper.Insert(packed, g5, 6, 5);
packed = BitHelper.Insert(packed, b5, 1, 5);
packed = BitHelper.Insert(packed, a1, 0, 1);

return (ushort)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• RGBA-5551 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}