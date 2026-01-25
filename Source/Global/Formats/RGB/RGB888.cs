using System.IO;
using SkiaSharp;

public static class RGB888
{
// Get Color from RGB888

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
TraceLogger.WriteLine("• RGB-888 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode24(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static uint EncodeColor(TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Red, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Blue, 0, 8);

return (uint)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• RGB-888 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode24(writer, image, EncodeColor);
}

}