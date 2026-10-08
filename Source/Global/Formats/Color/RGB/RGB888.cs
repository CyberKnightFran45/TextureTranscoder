using System;
using SkiaSharp;

public static class RGB888
{
// Decode color bits from RGB888

internal static TextureColor DecodeColor(in TextureColor24 flags) => new(flags);

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode24(source, width, height, DecodeColor);
}

// Encode color bits

internal static TextureColor24 EncodeColor(in TextureColor color) => new(color);

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode24(image, EncodeColor);
}

}