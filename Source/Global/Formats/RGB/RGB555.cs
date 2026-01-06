using System.IO;
using SkiaSharp;

public static class RGB555
{
// Get Color from RGB555

private static TextureColor DecodeColor(ushort flags)
{
int r5 = BitHelper.Extract(flags, 10, 5);
int g5 = BitHelper.Extract(flags, 5, 5);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g5, 5);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• RGB-555 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Get Color from Image

private static ushort EncodeColor(TextureColor color)
{
int packed = 0;

int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g5 = BitHelper.QuantizeFrom8(color.Green, 5);
int b5 = BitHelper.QuantizeFrom8(color.Blue, 5);

packed = BitHelper.Insert(packed, r5, 10, 5);
packed = BitHelper.Insert(packed, g5, 5, 5);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Write pixels to Bitmap
	
public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• RGB-555 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}