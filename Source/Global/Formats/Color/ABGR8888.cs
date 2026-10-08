using System;
using SkiaSharp;

public static class ABGR8888
{
// Decode color bits from ABGR8888

private static TextureColor DecodeColor(in uint flags)
{
var a = (byte)BitHelper.Extract(flags, 24, 8);
var b = (byte)BitHelper.Extract(flags, 16, 8);
var g = (byte)BitHelper.Extract(flags, 8, 8);
var r = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, b, a);
}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode32(source, width, height, DecodeColor);
}

// Encode color bits

private static uint EncodeColor(in TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Alpha, 24, 8);
packed = BitHelper.Insert(packed, color.Blue, 16, 8);
packed = BitHelper.Insert(packed, color.Green, 8, 8);
packed = BitHelper.Insert(packed, color.Red, 0, 8);

return (uint)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode32(image, EncodeColor);
}


}