using System;
using SkiaSharp;

// Parse DXT5 Images in RGBA Order

public static class DXT5_RGBA
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return DXTDecoder.Decode(source, width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return DXTEncoder.Encode(image, false, null, DXT4_RGBA.EncodeAlpha);
}

}