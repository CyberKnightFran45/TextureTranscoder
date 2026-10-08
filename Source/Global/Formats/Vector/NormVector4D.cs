using System;
using SkiaSharp;

// Normalize Bytes for 4D Vectors 

public static class NormVector4D
{
// Decode color bits

private static TextureColor DecodeColor(in uint flags)
{
var r = (byte)BitHelper.Extract(flags, 24, 8);
var g = (byte)BitHelper.Extract(flags, 16, 8);
var b = (byte)BitHelper.Extract(flags, 8, 8);
var a = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b, a);
}

// Decode vector

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode32(source, width, height, DecodeColor);
}

// Encode Color bits

private static uint EncodeColor(in TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Red, 24, 8);
packed = BitHelper.Insert(packed, color.Green, 16, 8);
packed = BitHelper.Insert(packed, color.Blue, 8, 8);
packed = BitHelper.Insert(packed, color.Alpha, 0, 8);

return (uint)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode32(image, EncodeColor);
}

}