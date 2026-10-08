using System;
using SkiaSharp;

// ETC2 RGB compression (no alpha channel)

public static class ETC2_RGB
{
// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return ETCBase.Encode(image, ETC2Encoder.EncodeBlock);
}


// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return ETCBase.Decode(source, width, height, ETC2Decoder.DecodeBlock);
}

}