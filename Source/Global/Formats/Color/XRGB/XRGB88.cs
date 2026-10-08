using System;
using SkiaSharp;

public static class XRGB888
{
// Decode image

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode32(source, width, height, RGB888.DecodeColor);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode32(image, RGB888.EncodeColor);
}

}