using System;
using SkiaSharp;

// Parse Images as XRGB8888 followed by Alpha Chanel (8-bits)

public static class XRGB8888_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
int colorSize = width * height * 4;

var decImg = RGB.Decode32(source[ .. colorSize ], width, height, XRGB8888.DecodeColor);
AlphaCodec.Decode8(source[ colorSize .. ], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
int square = image.GetSquare();

int colorSize = square * 4;
int alphaSize = square;

NativeBuffer owner = new(colorSize + alphaSize);

var colorBytes = owner.AsSpan(0, colorSize);
var alphaBytes = owner.AsSpan(colorSize, alphaSize);

RGB.Encode32(image, colorBytes, XRGB8888.EncodeColor);
AlphaCodec.Encode8(image, alphaBytes);

return owner;
}

}