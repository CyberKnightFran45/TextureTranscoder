using System;
using SkiaSharp;

// Parse DXT1 Images in RGB Order

public static class DXT1_RGB
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return DXTDecoder.Decode(source, width, height);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return DXTEncoder.Encode(image, false);
}

}