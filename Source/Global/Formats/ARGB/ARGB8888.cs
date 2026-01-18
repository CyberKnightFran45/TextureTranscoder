using System.IO;
using SkiaSharp;

// Parse Image in the ARGB format (8 bits per Color)

public static class ARGB8888
{
// Get Color from Binary

internal static TextureColor DecodeColor(uint flags)
{
var a = (byte)BitHelper.Extract(flags, 24, 8);
var r = (byte)BitHelper.Extract(flags, 16, 8);
var g = (byte)BitHelper.Extract(flags, 8, 8);
var b = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ARGB-8888 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode32(reader, width, height, DecodeColor);
}

// Get Color from Image

internal static uint EncodeColor(TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Alpha, 24, 8);
packed = BitHelper.Insert(packed, color.Red, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Blue, 0, 8);

return (uint)packed;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ARGB-8888 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode32(writer, image, EncodeColor);
}

}