using System;
using SkiaSharp;

// Parse Luminance Images

public static class L8
{
// Decode Luminance

private static TextureColor DecodeLuminance(in byte l) => new(l, l, l, 255);

// Decode L8 Texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode8(source, width, height, DecodeLuminance);
}

// Encode Luminance

internal static byte EncodeLuminance(in TextureColor color)
{
double lumi = color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114;

return (byte)lumi;
}

// Encode L8 Texture

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode8(image, EncodeLuminance);
}

}