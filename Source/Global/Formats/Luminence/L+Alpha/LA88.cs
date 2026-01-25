using System.IO;
using SkiaSharp;

// Parse Luminance + Alpha Images (16 bits)

public static class LA88
{
// Decode Contrast

private static TextureColor DecodeContrast(ushort flags)
{
var l = (byte)BitHelper.Extract(flags, 8, 8);
var a = (byte)BitHelper.Extract(flags, 0, 8);

return new(l, l, l, a);
}

// Read LA88 Texture

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• LA88 Texture Decode:");
TraceLogger.WriteLine();

return RGB.Decode16(reader, width, height, DecodeContrast);
}

// Encode Contrast

private static ushort EncodeContrast(TextureColor color)
{
int packed = 0;

int l = L8.EncodeLuminance(color);
int a = color.Alpha;

packed = BitHelper.Insert(packed, l, 8, 8);
packed = BitHelper.Insert(packed, a, 0, 8);

return (ushort)packed;
}

// Write LA88 Texture

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• LA88 Texture Encode:");
TraceLogger.WriteLine();

return RGB.Encode16(writer, image, EncodeContrast);
}

}