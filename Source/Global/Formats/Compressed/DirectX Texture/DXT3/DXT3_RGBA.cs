using System;
using SkiaSharp;

// Parse DXT3 Images in RGBA Order

public static class DXT3_RGBA
{
// Decode Color (add alpha)

private static void DecodeColor(byte alpha, ref TextureColor color) => color.Alpha = alpha;

// Decode Block

internal static void DecodeBlock(ReadOnlySpan<byte> alpha, Span<TextureColor> block)
{

for(int i = 0; i < 16; i++)
DecodeColor(alpha[i], ref block[i] );

}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return DXTDecoder.Decode(source, width, height, DecodeBlock, DXT2_RGBA.DecodeAlpha);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return DXTEncoder.Encode(image, false, null, DXT2_RGBA.EncodeAlpha);
}

}