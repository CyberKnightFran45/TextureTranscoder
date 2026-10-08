using System;
using SkiaSharp;

public static class RGB555
{
// Decode color bits from  RGB555

private static TextureColor DecodeColor(in ushort flags)
{
int r5 = BitHelper.Extract(flags, 10, 5);
int g5 = BitHelper.Extract(flags, 5, 5);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g5, 5);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return RGB.Decode16(source, width, height, DecodeColor);
}

// Encode color bits

private static ushort EncodeColor(in TextureColor color)
{
int packed = 0;

int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g5 = BitHelper.QuantizeFrom8(color.Green, 5);
int b5 = BitHelper.QuantizeFrom8(color.Blue, 5);

packed = BitHelper.Insert(packed, r5, 10, 5);
packed = BitHelper.Insert(packed, g5, 5, 5);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Encode image
	
public static NativeBuffer Encode(SKBitmap image)
{
return RGB.Encode16(image, EncodeColor);
}

}