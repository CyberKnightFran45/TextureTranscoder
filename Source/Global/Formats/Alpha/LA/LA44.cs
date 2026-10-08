using System;
using SkiaSharp;

// Parse Luminance + Alpha Images (8 bits)

public static class LA44
{
// Decode Contrast

private static TextureColor DecodeContrast(in byte flags)
{
byte l = BitHelper.ExtractAndExpandTo8(flags, 4, 4);
byte a = BitHelper.ExtractAndExpandTo8(flags, 0, 4);

return new(l, l, l, a);
}

// Decode LA44 Texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode8(source, width, height, DecodeContrast);
}

// Encode Contrast

private static byte EncodeContrast(in TextureColor color)
{
int packed = 0;

int l4 = L8.EncodeLuminance(color) >> 4;
int a4 = color.Alpha >> 4;

packed = BitHelper.Insert(packed, l4, 4, 4);
packed = BitHelper.Insert(packed, a4, 0, 4);

return (byte)packed;
}

// Encode LA44 Texture

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode8(image, EncodeContrast);
}

}