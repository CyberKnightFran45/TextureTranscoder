using System;
using SkiaSharp;

public static class RGB565
{
// Decode color bits from  RGB565

internal static TextureColor DecodeColor(in ushort flags)
{
int r5 = BitHelper.Extract(flags, 11, 5);
int g6 = BitHelper.Extract(flags, 5, 6);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g6, 6);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode16(source, width, height, DecodeColor);
}

// Encode color bits

internal static ushort EncodeColor(in TextureColor color)
{
int packed = 0;

int r5 = color.Red >> 3;
int g6 = color.Green >> 2;
int b5 = color.Blue >> 3;

packed = BitHelper.Insert(packed, r5, 11, 5);
packed = BitHelper.Insert(packed, g6, 5, 6);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode16(image, EncodeColor);
}

}