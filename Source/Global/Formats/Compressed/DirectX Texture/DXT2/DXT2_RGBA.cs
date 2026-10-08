using System;
using SkiaSharp;

// Parse DXT2 Images in RGBA Order

public static class DXT2_RGBA
{
// Extract Alpha from Color

private static byte ExtractAlpha(byte c, byte alpha)
{

if(alpha == 0)
return 0;

int flags = c * 255 / alpha;

return TextureHelper.ColorClamp(flags);
}

// Decode Color

private static void DecodeColor(byte alpha, ref TextureColor color)
{
byte r = ExtractAlpha(color.Red, alpha);
byte g = ExtractAlpha(color.Green, alpha);
byte b = ExtractAlpha(color.Blue, alpha);

color.Red = r;
color.Green = g;
color.Blue = b;
}

// Decode Block

internal static void DecodeBlock(ReadOnlySpan<byte> alpha, Span<TextureColor> block)
{

for(int i = 0; i < 16; i++)
DecodeColor(alpha[i], ref block[i] );

}

// Decode Alpha

internal static void DecodeAlpha(ReadOnlySpan<ushort> encoded, Span<byte> plain)
{

for(int j = 0; j < 4; j++)
{
ushort row = encoded[j];

for(int i = 0; i < 4; i++)
{
plain[j * 4 + i] = BitHelper.ExtractAndExpandTo8(row, 0, 4);

row >>= 4;
}

}

}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
return DXTDecoder.Decode(source, width, height, DecodeBlock, DecodeAlpha);
}

// Apply Alpha to Color

private static byte ApplyAlpha(byte c, byte alpha)
{
int flags = (c * alpha) >> 8;

return TextureHelper.ColorClamp(flags);
}

// Encode Color

private static void EncodeColor(ref TextureColor color)
{
byte alpha = color.Alpha;

byte r = ApplyAlpha(color.Red, alpha);
byte g = ApplyAlpha(color.Green, alpha);
byte b = ApplyAlpha(color.Blue, alpha);

color.Red = r;
color.Green = g;
color.Blue = b;
}

// Encode ColorBlock

internal static void EncodeBlock(Span<TextureColor> block)
{

for(int i = 0; i < 16; i++)
EncodeColor(ref block[i]);

}

// Encode Alpha

internal static void EncodeAlpha(ReadOnlySpan<TextureColor> block, Span<ushort> alpha)
{

for(int row = 0; row < 4; row++)
{
ushort alphaRow = 0;

for(int col = 0; col < 4; col++)
{
int idx = row * 4 + col;
int aRow = (block[idx].Alpha >> 4) << (col * 4);

alphaRow |= (ushort)aRow;
}

alpha[row] = alphaRow;
}

}

// Encode image

public static NativeBuffer Encode(SKBitmap image)
{
return DXTEncoder.Encode(image, false, EncodeBlock, EncodeAlpha);
}

}