using System.IO;
using SkiaSharp;

public static class XRGB8888
{
// Get Color from Binary

internal static TextureColor DecodeColor(uint flags)
{
var r = (byte)BitHelper.Extract(flags, 16, 8);
var g = (byte)BitHelper.Extract(flags, 8, 8);
var b = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• XRGB-8888 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode32(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static uint EncodeColor(TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, 0xFF, 24, 8);
packed = BitHelper.Insert(packed, color.Red, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Blue, 0, 8);

return (uint)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• XRGB-8888 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode32(writer, image, EncodeColor);
}

}