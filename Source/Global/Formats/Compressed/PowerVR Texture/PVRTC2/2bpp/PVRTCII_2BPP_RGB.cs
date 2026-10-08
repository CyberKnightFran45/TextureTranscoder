using System;
using SkiaSharp;

// Parse PVRTCII Images in RGB (2 bpp Mode)

public static class PVRTCII_2BPP_RGB
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return PVRII_2BPP_Decoder.Decode(source, width, height, false);
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
return PVRII_2BPP_Encoder.Encode(ref image, false);
}

}