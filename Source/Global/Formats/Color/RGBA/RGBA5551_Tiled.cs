using System;
using SkiaSharp;

// Parse Image in the RGBA5551 format (Tiled)

public static class RGBA5551_Tiled
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, int tileSize)
{
return RGB.DecodeTile16(source, width, height, tileSize, RGBA5551.DecodeColor);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, int tileSize)
{
return RGB.EncodeTile16(image, tileSize, RGBA5551.EncodeColor);
}

}