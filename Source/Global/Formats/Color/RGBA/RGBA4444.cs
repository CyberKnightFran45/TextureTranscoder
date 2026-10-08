using System;
using SkiaSharp;

public static class RGBA4444
{
// Decode color bits

internal static TextureColor DecodeColor(in ushort flags)
{
int r4 = BitHelper.Extract(flags, 12, 4);
int g4 = BitHelper.Extract(flags, 8, 4);
int b4 = BitHelper.Extract(flags, 4, 4);
int a4 = BitHelper.Extract(flags, 0, 4);

byte r = BitHelper.ExpandTo8(r4, 4);
byte g = BitHelper.ExpandTo8(g4, 4);
byte b = BitHelper.ExpandTo8(b4, 4);
byte a = BitHelper.ExpandTo8(a4, 4);

return new(r, g, b, a);
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

int r4 = color.Red >> 4;
int g4 = color.Green >> 4;
int b4 = color.Blue >> 4;
int a4 = color.Alpha >> 4;

packed = BitHelper.Insert(packed, r4, 12, 4);
packed = BitHelper.Insert(packed, g4, 8, 4);
packed = BitHelper.Insert(packed, b4, 4, 4);
packed = BitHelper.Insert(packed, a4, 0, 4);

return (ushort)packed;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode16(image, EncodeColor);
}

}