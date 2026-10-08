using System;
using SkiaSharp;

// Parse Image in the RGB565 format (Tiled)

public static class RGB565_Tiled
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, int tileSize)
{
return RGB.DecodeTile16(source, width, height, tileSize, RGB565.DecodeColor);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, int tileSize)
{
return RGB.EncodeTile16(image, tileSize, RGB565.EncodeColor);
}

}