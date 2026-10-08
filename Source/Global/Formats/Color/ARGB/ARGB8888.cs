using System;
using SkiaSharp;

// Parse Image in the ARGB format (8 bits per Color)

public static class ARGB8888
{
// Decode color bits

internal static TextureColor DecodeColor(in uint flags)
{
var a = (byte)BitHelper.Extract(flags, 24, 8);
var r = (byte)BitHelper.Extract(flags, 16, 8);
var g = (byte)BitHelper.Extract(flags, 8, 8);
var b = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b, a);
}

// Read Bitmap

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode32(source, width, height, DecodeColor);
}

// Encode color bits

internal static uint EncodeColor(in TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Alpha, 24, 8);
packed = BitHelper.Insert(packed, color.Red, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Blue, 0, 8);

return (uint)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode32(image, EncodeColor);
}

}