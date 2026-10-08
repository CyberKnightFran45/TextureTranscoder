using System;
using SkiaSharp;

// Parse Luminance + Alpha Images (16 bits)

public static class LA88
{
// Decode Contrast

private static TextureColor DecodeContrast(in ushort flags)
{
var l = (byte)BitHelper.Extract(flags, 8, 8);
var a = (byte)BitHelper.Extract(flags, 0, 8);

return new(l, l, l, a);
}

// Decode LA88 Texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode16(source, width, height, DecodeContrast);
}

// Encode Contrast

private static ushort EncodeContrast(in TextureColor color)
{
int packed = 0;

int l = L8.EncodeLuminance(color);
int a = color.Alpha;

packed = BitHelper.Insert(packed, l, 8, 8);
packed = BitHelper.Insert(packed, a, 0, 8);

return (ushort)packed;
}

// Encode LA88 Texture

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode16(image, EncodeContrast);
}

}