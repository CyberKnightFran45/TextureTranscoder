using System;
using SkiaSharp;

// Parse ETC1-RGB Images followed by Alpha Palette

public static class ETC1_RGB_A_Palette
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
int compressedSize = ETCBase.ComputeETCSize(width, height);

var decImg = ETC1Decoder.Decode(source[.. compressedSize], width, height);
AlphaCodec.DecodePalette4(source[compressedSize ..], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, out int paletteSize)
{
int compressedSize = ETCBase.ComputeETCSize(image.Width, image.Height);
int alphaSize = ( (image.GetSquare() + 1) >> 1) + 17;

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

ETC1Encoder.Encode(image, colorInfo);
AlphaCodec.EncodePalette4(image, alphaInfo);

paletteSize = alphaSize;

return output;
}

}