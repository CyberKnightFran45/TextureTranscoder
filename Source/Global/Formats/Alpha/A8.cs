using System;
using SkiaSharp;

// Parse Alpha Images

public static class A8
{
// Decode Alpha Channel

private static TextureColor DecodeAlpha(in byte a) => new(255, 255, 255, a);

// Decode A8 Texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode8(source, width, height, DecodeAlpha);
}

// Encode Alpha Channel

private static byte EncodeAlpha(in TextureColor color) => color.Alpha;

// Encode A8 Texture

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode8(image, EncodeAlpha);
}

}