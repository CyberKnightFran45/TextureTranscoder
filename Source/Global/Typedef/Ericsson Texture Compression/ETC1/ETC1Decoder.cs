using static ETCBase;

using System;
using System.IO;
using SkiaSharp;

// Decodes ETC1 Images

public static class ETC1Decoder
{
// Extract Color from flags

private static int ExtractColor(ulong flags, int factor) => (int)BitHelper.Extract(flags, factor, 5);

// Update Color from flags

private static int UpdateColor(ulong flags, int factor)
{
var b = (int)BitHelper.Extract(flags, factor, 3);

return (b << 29) >> 29;
}

// Unpack Color bits

private static int UnpackBits(ulong flags, int factor)
{
var b = (int)BitHelper.Extract(flags, factor, 4);

return BitHelper.ExpandTo8(b, 4);
}

// Expand Color to 8-bits

private static int ExpandBits(int flags) => BitHelper.ExpandTo8(flags, 5);

// Get Table Index

private static int GetTableIndex(ulong flags, int factor) => (int)BitHelper.Extract(flags, factor, 3);

// Get Bits from 4x4 Planar

private static int ExtractPlanarBit(ulong flags, int x, int y, int baseShift = 0)
{
return (int)( (flags >> (baseShift + ( (y << 2) | x) ) ) & 0x1);
}

// Get Delta from Mod Table

private static int GetDelta(int index, int modifier, bool isNegative)
{
return MOD_TABLE[index, modifier] * (isNegative ? -1 : 1);
}

// Get Mode

private static bool GetMode(ulong flags, int pos) => ( (flags >> pos) & 1) == 1;

// Get FlipMode

private static bool GetFlipMode(ulong flags) => GetMode(flags, 32);

// Get DiffMode

private static bool GetDiffMode(ulong flags) => GetMode(flags, 33);

// Decode Pixels

internal static void DecodePixels(ulong flags, Span<TextureColor> result)
{
bool compareBits = GetDiffMode(flags);
bool flipBits = GetFlipMode(flags);

int r1, b1, g1, r2, g2, b2;

if(compareBits)
{
int r = ExtractColor(flags, 59);
int g = ExtractColor(flags, 51);
int b = ExtractColor(flags, 43);

r1 = ExpandBits(r);
g1 = ExpandBits(g);
b1 = ExpandBits(b);

r += UpdateColor(flags, 56);
g += UpdateColor(flags, 48);
b += UpdateColor(flags, 40);

r2 = ExpandBits(r);
g2 = ExpandBits(g);
b2 = ExpandBits(b);
}

else
{
r1 = UnpackBits(flags, 60);
g1 = UnpackBits(flags, 52);
b1 = UnpackBits(flags, 44);

r2 = UnpackBits(flags, 56);
g2 = UnpackBits(flags, 48);
b2 = UnpackBits(flags, 40);
}

int t1 = GetTableIndex(flags, 37);
int t2 = GetTableIndex(flags, 34);

for(int i = 0; i < TILE_SIZE; i++)
{

for(int j = 0; j < TILE_SIZE; j++)
{
int modifier = ExtractPlanarBit(flags, i, j);
bool isNegative = ExtractPlanarBit(flags, i, j, 16) == 1;

bool useTable1 = (flipBits && i < 2) || (!flipBits && j < 2);
int delta = GetDelta(useTable1 ? t1 : t2, modifier, isNegative);

result[(i << 2) | j] = TextureHelper.AddColorBias(r1, g1, b1, delta);
}

}

}

// Decode ETC1

public static SKBitmap Decode(Stream reader, int width, int height)
{
return ETCBase.Decode(reader, width, height, DecodePixels);
}

}