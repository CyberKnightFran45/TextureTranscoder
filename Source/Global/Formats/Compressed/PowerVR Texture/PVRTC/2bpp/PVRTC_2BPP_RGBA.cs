using System;
using SkiaSharp;

// Parse PVRTC Images in RGBA (2 bpp Mode)

public static class PVRTC_2BPP_RGBA
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return PVRDecoder.Decode(source, width, height, true, true);
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
return PVREncoder.Encode(ref image, true, true);
}

}