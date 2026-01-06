using System;
using System.IO;
using SkiaSharp;

// Parse DXT4 Images in RGBA Order

public static class DXT4_RGBA
{
// Extract Alpha from Color

private static byte ExtractAlpha(byte c, byte alpha)
{
int flags = (c << 8) / alpha;

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

private static void DecodeBlock(ReadOnlySpan<byte> alpha, Span<TextureColor> block)
{

for(int i = 0; i < 16; i++)
DecodeColor(alpha[i], ref block[i]);

}

// Decode Alpha

internal static void DecodeAlpha(ReadOnlySpan<ushort> encoded, Span<byte> plain)
{
var a0 = (byte)(encoded[0] & 0xFF);
var a1 = (byte)(encoded[0] >> 8);

ulong alphaBits = encoded[1] | ( (ulong)encoded[2] << 16) | ( (ulong)encoded[3] << 32);
Span<byte> alphaTable = stackalloc byte[8];

if(a0 > a1)
{
alphaTable[0] = a0;
alphaTable[1] = a1;
alphaTable[2] = (byte)( (6 * a0 + 1 * a1) / 7);
alphaTable[3] = (byte)( (5 * a0 + 2 * a1) / 7);
alphaTable[4] = (byte)( (4 * a0 + 3 * a1) / 7);
alphaTable[5] = (byte)( (3 * a0 + 4 * a1) / 7);
alphaTable[6] = (byte)( (2 * a0 + 5 * a1) / 7);
alphaTable[7] = (byte)( (1 * a0 + 6 * a1) / 7);
}

else
{
alphaTable[0] = a0;
alphaTable[1] = a1;
alphaTable[2] = (byte)( (4 * a0 + 1 * a1) / 5);
alphaTable[3] = (byte)( (3 * a0 + 2 * a1) / 5);
alphaTable[4] = (byte)( (2 * a0 + 3 * a1) / 5);
alphaTable[5] = (byte)( (1 * a0 + 4 * a1) / 5);
alphaTable[6] = 0;
alphaTable[7] = 255;
}

for(int i = 0; i < 16; i++)
{
int index = (int)(alphaBits & 0b111);
plain[i] = alphaTable[index];

alphaBits >>= 3;
}

}

// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• DXT4-RGBA Texture Decode:");
TraceLogger.WriteLine();

return DXT.Decode(reader, width, height, DecodeBlock, DecodeAlpha);
}

// Emit AlphaIndices

private static void EmitAlphaIndices(ReadOnlySpan<TextureColor> pixels, byte min, byte max,
                                     Span<byte> indices)
{

Span<byte> alphas =
[
max,
min,
(byte)( (6 * max + min) / 7),
(byte)( (5 * max + (min << 1) ) / 7),
(byte)( ( (max << 2) + 3 * min) / 7),
(byte)( (3 * max + (min << 2) ) / 7),
(byte)( ( (max << 1) + 5 * min) / 7),
(byte)( (max + 6 * min) / 7),
];

for(int i = 0; i < 16; i++)
{
byte a = pixels[i].Alpha;
int minDistance = int.MaxValue;

for(byte j = 0; j < 8; j++)
{
int dist = Math.Abs(a - alphas[j]);

if(dist < minDistance)
{
minDistance = dist;
indices[i] = j;
}

}

}

}

// Encode Alpha

internal static void EncodeAlpha(ReadOnlySpan<TextureColor> block, Span<ushort> alpha)
{
byte minAlpha = 255;
byte maxAlpha = 0;

for(int i = 0; i < 16; i++)
{
byte a = block[i].Alpha;

if(a > maxAlpha)
maxAlpha = a;

if(a < minAlpha)
minAlpha = a;

}

if(minAlpha == maxAlpha)
{
alpha[0] = (ushort)( (minAlpha << 8) | maxAlpha);
alpha[1] = alpha[2] = alpha[3] = 0;

return;
}

int diff = (maxAlpha - minAlpha) >> 4;

maxAlpha = TextureHelper.ColorClamp(maxAlpha - diff);
minAlpha = TextureHelper.ColorClamp(minAlpha + diff);

alpha[0] = (ushort)( (minAlpha << 8) | maxAlpha);

Span<byte> indices = stackalloc byte[16];
EmitAlphaIndices(block, minAlpha, maxAlpha, indices);

ulong packed = 0;
int pos = 0;

for(int i = 0; i < 16; i++)
{
packed |= (indices[i] & 0b111UL) << pos;

pos += 3;
}

alpha[1] = (ushort)(packed & 0xFFFF);
alpha[2] = (ushort)( (packed >> 16) & 0xFFFF);
alpha[3] = (ushort)( (packed >> 32) & 0xFFFF);
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• DXT4-RGBA Texture Encode:");
TraceLogger.WriteLine();

return DXT.Encode(writer, image, false, DXT2_RGBA.EncodeBlock, EncodeAlpha);
}

}