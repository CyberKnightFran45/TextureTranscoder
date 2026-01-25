using System.IO;
using SkiaSharp;

// Normalize Bytes for 4D Vectors 

public static class NormVector4D
{
// Get Color

private static TextureColor DecodeColor(uint flags)
{
var r = (byte)BitHelper.Extract(flags, 24, 8);
var g = (byte)BitHelper.Extract(flags, 16, 8);
var b = (byte)BitHelper.Extract(flags, 8, 8);
var a = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• Normalized Vector4D Decode:");
TraceLogger.WriteLine();

return RGB.Decode32(reader, width, height, DecodeColor);
}

// Encode Color

private static uint EncodeColor(TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Red, 24, 8);
packed = BitHelper.Insert(packed, color.Green, 16, 8);
packed = BitHelper.Insert(packed, color.Blue, 8, 8);
packed = BitHelper.Insert(packed, color.Alpha, 0, 8);

return (uint)packed;
}

// Write vector

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• Normalized Vector4D Encode:");
TraceLogger.WriteLine();

return RGB.Encode32(writer, image, EncodeColor);
}

}