using System;
using SkiaSharp;

public static class ETC1_RGB
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return ETC1Decoder.Decode(source, width, height);
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return ETC1Encoder.Encode(image);
}

}