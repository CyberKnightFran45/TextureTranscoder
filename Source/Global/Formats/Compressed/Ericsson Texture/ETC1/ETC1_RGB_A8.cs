using System;
using SkiaSharp;

// Parse ETC1-RGB Images followed by A8

public static class ETC1_RGB_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, bool swapBytes = false)
{
int compressedSize = ETCBase.ComputeETCSize(width, height);

var decImg = ETC1Decoder.Decode(source[.. compressedSize], width, height, swapBytes);
AlphaCodec.Decode8(source[compressedSize .. ], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, bool swapBytes = false)
{
int compressedSize = ETCBase.ComputeETCSize(image.Width, image.Height);
int alphaSize = image.GetSquare();

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

ETC1Encoder.Encode(image, colorInfo, swapBytes);
AlphaCodec.Encode8(image, alphaInfo);

return output;
}

}