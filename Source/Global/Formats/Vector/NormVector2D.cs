using System;
using SkiaSharp;

// Normalize Bytes for 2D Vectors 

public static class NormVector2D
{
// Decode color bits

private static TextureColor DecodeColor(in ushort flags)
{
var r = (byte)BitHelper.Extract(flags, 8, 8);
var g = (byte)BitHelper.Extract(flags, 0, 8);

return new(r, g, 0);
}

// Decode vector

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode16(source, width, height, DecodeColor);
}

// Encode Color bits

private static ushort EncodeColor(in TextureColor color)
{
int packed = 0;

packed = BitHelper.Insert(packed, color.Red, 8, 8);
packed = BitHelper.Insert(packed, color.Green, 0, 8);

return (ushort)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode16(image, EncodeColor);
}

}