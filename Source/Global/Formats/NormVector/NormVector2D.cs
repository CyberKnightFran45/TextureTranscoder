using System.IO;
using SkiaSharp;

// Normalize Bytes for 2D Vectors 

public static class NormVector2D
{
// Get Color

private static TextureColor DecodeColor(ushort flags)
{
var r = (byte)BitHelper.Extract(flags, 8, 8);
var g = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, 0);
}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• Normalized Vector2D Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeColor);
}

// Encode Color

private static ushort EncodeColor(TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Red, 8, 8);
packed = BitHelper.Insert(packed, color.Green, 0, 8);

return (ushort)packed;
}

// Write vector

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• Normalized Vector2D Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeColor);
}

}