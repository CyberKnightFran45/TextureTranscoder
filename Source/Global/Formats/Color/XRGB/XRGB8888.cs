using System;
using SkiaSharp;

public static class XRGB8888
{
// Decode color bits

internal static TextureColor DecodeColor(in uint flags)
{
var r = (byte)BitHelper.Extract(flags, 16, 8);
var g = (byte)BitHelper.Extract(flags, 8, 8);
var b = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b);
}

// Decode image

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode32(source, width, height, DecodeColor);
}

// Encode color bits

internal static uint EncodeColor(in TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, 0xFF, 24, 8);
packed = BitHelper.Insert(packed, color.Red, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Blue, 0, 8);

return (uint)packed;
}

// Encode Image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode32(image, EncodeColor);
}

}