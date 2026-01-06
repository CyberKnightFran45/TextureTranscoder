using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Supports Ericsson Texture Compression (ETC1)

public static unsafe class ETC1
{
// ETC1 Modifiers

private static readonly int[,] MOD_TABLE =
{

{ 2, 8 },
{ 5, 17 },
{ 9, 29 },
{ 13, 42 },
{ 18, 60 },
{ 24, 80 },
{ 33, 106 },
{ 47, 183 }

};

#region ==========  ENCODER  ==========

// Get Left Colors as 2x4 Tile

private static void GetLeftColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
ETC.ExtractSubBlock(pixels, result, 0, 0, 2, 4);
}

// Get Right Colors as 2x4 Tile

private static void GetRightColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
ETC.ExtractSubBlock(pixels, result, 2, 0, 2, 4);
}

// Get Top Colors as 4x2 Tile

private static void GetTopColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
ETC.ExtractSubBlock(pixels, result, 0, 0, 4, 2);
}

// Get Bottom Colors as 4x2 Tile

private static void GetBottomColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
ETC.ExtractSubBlock(pixels, result, 0, 2, 4, 2);
}

// Get DiffMean

private static int GetDiffMean(in TextureColor min, in TextureColor max)
{
int rDiff = max.Red - min.Red;
int gDiff = max.Green - min.Green;
int bDiff = max.Blue - min.Blue;

return ETC.GetMean(rDiff, gDiff, bDiff);
}

// Get ETC1 Modifier

private static int GenModifier(ReadOnlySpan<TextureColor> pixels, out TextureColor baseColor)
{
TextureColor minColor = new(0, 0, 0);       // Black
TextureColor maxColor = new(255, 255, 255); // White

int minY = int.MaxValue;
int maxY = int.MinValue;

for(int i = 0; i < 8; i++)
{
var px = pixels[i];

if(px.Alpha == 0)
continue;

int y = ETC.GetMean(px.Red, px.Green, px.Blue);

if(y > maxY)
{
maxY = y;
maxColor = px;
}

if(y < minY)
{
minY = y;
minColor = px;
}

}

int diffMean = GetDiffMean(minColor, maxColor);
int modDiff = int.MaxValue;

int modifier = -1;
int mode = -1;

for(int i = 0; i < 8; i++)
{
int low = MOD_TABLE[i, 0];
int high = MOD_TABLE[i, 1];

int deltaL = TextureHelper.ColorClamp(low * 2);
int deltaM = TextureHelper.ColorClamp(low + high);
int deltaH = TextureHelper.ColorClamp(high * 2);

if(Math.Abs(diffMean - deltaL) < modDiff)
{
modDiff = Math.Abs(diffMean - deltaL);
modifier = i;

mode = 0;
}

if(Math.Abs(diffMean - deltaM) < modDiff)
{
modDiff = Math.Abs(diffMean - deltaM);
modifier = i;

mode = 1;
}

if(Math.Abs(diffMean - deltaH) < modDiff)
{
modDiff = Math.Abs(diffMean - deltaH);
modifier = i;

mode = 2;
}

}

byte mR, mG, mB;

if(mode == 1)
{
float ratio = MOD_TABLE[modifier, 0] / (float)MOD_TABLE[modifier, 1];
float ratio2 = 1F - ratio;

mR = ETC.InterpolateColor(minColor.Red, maxColor.Red, ratio, ratio2);
mG = ETC.InterpolateColor(minColor.Green, maxColor.Green, ratio, ratio2);
mB = ETC.InterpolateColor(minColor.Blue, maxColor.Blue, ratio, ratio2);
}

else
{
mR = ETC.AverageColor(minColor.Red, maxColor.Red);
mG = ETC.AverageColor(minColor.Green, maxColor.Green);
mB = ETC.AverageColor(minColor.Blue, maxColor.Blue);
}

baseColor = new(mR, mG, mB);

return modifier;
}

// Set Mode

private static void SetMode(ref ulong data, bool mode, int pos)
{
data = BitHelper.Insert(data, mode ? 1uL : 0uL, pos, 1);
}

// Set FlipMode

private static void SetFlipMode(ref ulong data, bool mode) => SetMode(ref data, mode, 32);

// Set DiffMode

private static void SetDiffMode(ref ulong diff, bool mode) => SetMode(ref diff, mode, 33);

// Set Table (Generic)

private static void SetTable(ref ulong data, int table, int pos)
{
data = BitHelper.Insert(data, (ulong)table, pos, 3);
}

// Set Table1

private static void SetTable1(ref ulong data, int table) => SetTable(ref data, table, 37);

// Set Table2

private static void SetTable2(ref ulong data, int table) => SetTable(ref data, table, 34);

// Get Diff between pixels

private static void GenPixDiff(ref ulong data, ReadOnlySpan<TextureColor> pixels,
                               in TextureColor baseColor, int modifier,
							   int startX, int endX, int startY, int endY)
{
int baseMean = ETC.GetMean(baseColor.Red, baseColor.Green, baseColor.Blue);
int i = 0;

for(int row = startY; row < endY; row++)
{

for(int col = startX; col < endX; col++)
{
int currMean = ETC.GetMean(pixels[i].Red, pixels[i].Green, pixels[i].Blue);
int diff = currMean - baseMean;

if(diff < 0)
data |= 1uL << (col * ETC.TILE_SIZE + row + 16);

int tDiff1 = Math.Abs(diff) - MOD_TABLE[modifier, 0];
int tDiff2 = Math.Abs(diff) - MOD_TABLE[modifier, 1];

if(Math.Abs(tDiff1) < Math.Abs(tDiff2) )
data |= 1uL << (col * ETC.TILE_SIZE + row);

i++;
}

}

}

// Get ColorDiff

private static int GetColorDiff(int a, int b) => (b - a) / 8;

// Check Color Diff

private static bool CheckColorDiff(int flags) => flags > -4 && flags < 3;

// Check if all Colors are Different

private static bool IsDiffColor(int r, int g, int b)
{
return CheckColorDiff(r) && CheckColorDiff(g) && CheckColorDiff(b);
}

// Pack Color bits

private static ulong PackBits(int color, int shift)
{
int field = color / 0x11;

return (ulong)BitHelper.Insert(0, field, shift, 4);
}

// Set Table Index

private static ulong SetTableIndex(int flags, int factor) => (ulong)BitHelper.Insert(0, flags, factor, 3);

// Set BaseColors

private static void SetBaseColors(ref ulong data, in TextureColor c1, in TextureColor c2)
{
int r1 = c1.Red;
int g1 = c1.Green;
int b1 = c1.Blue;

int r2 = c2.Red;
int g2 = c2.Green;
int b2 = c2.Blue;

int rDiff = GetColorDiff(r1, r2);
int gDiff = GetColorDiff(g1, g2);
int bDiff = GetColorDiff(b1, b2);

bool hasDiff = IsDiffColor(rDiff, gDiff, bDiff);

if(hasDiff)
{
SetDiffMode(ref data, true);

r1 /= 8;
g1 /= 8;
b1 /= 8;

data |= (ulong)r1 << 59;
data |= (ulong)g1 << 51;
data |= (ulong)b1 << 43;

data |= SetTableIndex(rDiff, 56);
data |= SetTableIndex(gDiff, 48);
data |= SetTableIndex(bDiff, 40);
}

else
{
data |= PackBits(r1, 60);
data |= PackBits(g1, 52);
data |= PackBits(b1, 44);

data |= PackBits(r2, 56);
data |= PackBits(g2, 48);
data |= PackBits(b2, 40);
}

}

// Get X Component (Horizontal)

private static ulong GetX(ReadOnlySpan<TextureColor> pixels)
{
ulong data = 0;
SetFlipMode(ref data, false);

Span<TextureColor> left = stackalloc TextureColor[8];
GetLeftColors(pixels, left);

int mod = GenModifier(left, out TextureColor baseColor);
SetTable1(ref data, mod);

GenPixDiff(ref data, left, baseColor, mod, 0, 2, 0, 4);

Span<TextureColor> right = stackalloc TextureColor[8];
GetRightColors(pixels, right);

int mod2 = GenModifier(right, out TextureColor baseColor2);
SetTable2(ref data, mod2);

GenPixDiff(ref data, right, baseColor2, mod2, 2, 4, 0, 4);

SetBaseColors(ref data, baseColor, baseColor2);

return data;
}

// Get Y Component (Vertical)

private static ulong GetY(ReadOnlySpan<TextureColor> pixels)
{
ulong data = 0;
SetFlipMode(ref data, true);

Span<TextureColor> top = stackalloc TextureColor[8];
GetTopColors(pixels, top);

int mod = GenModifier(top, out TextureColor baseColor);
SetTable1(ref data, mod);

GenPixDiff(ref data, top, baseColor, mod, 0, 4, 0, 2);

Span<TextureColor> bottom = stackalloc TextureColor[8];
GetBottomColors(pixels, bottom);

int mod2 = GenModifier(bottom, out TextureColor baseColor2);
SetTable2(ref data, mod2);

GenPixDiff(ref data, bottom, baseColor2, mod2, 0, 4, 2, 4);

SetBaseColors(ref data, baseColor, baseColor2);

return data;
}

// Get Score between two Colors

private static int GetScore(ReadOnlySpan<TextureColor> plain, ReadOnlySpan<TextureColor> encoded)
{
int diff = 0;

for(int i = 0; i < 16; i++)
{
var px = plain[i];
var px2 = encoded[i];

diff += Math.Abs(px2.Red - px.Red);
diff += Math.Abs(px2.Green - px.Green);
diff += Math.Abs(px2.Blue - px.Blue);
}

return diff;
}

// Encode Color

private static ulong EncodeColor(ReadOnlySpan<TextureColor> pixels)
{
ulong x = GetX(pixels);
ulong y = GetY(pixels);

Span<TextureColor> rawX = stackalloc TextureColor[16];
DecodeColor(x, rawX);

int scoreX = GetScore(pixels, rawX);

Span<TextureColor> rawY = stackalloc TextureColor[16];
DecodeColor(y, rawY);

int scoreY = GetScore(pixels, rawY);

return scoreX < scoreY ? x : y;
}

// Encode ETC1

public static int Encode(Stream writer, SKBitmap image)
{
return ETC.Encode(writer, image, EncodeColor);
}

#endregion


#region ==========  DECODER  ==========

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

// Apply Delta to Colors

private static TextureColor ApplyDelta(int r, int g, int b, int delta)
{
byte dR = TextureHelper.ColorClamp(r + delta);
byte dG = TextureHelper.ColorClamp(g + delta);
byte dB = TextureHelper.ColorClamp(b + delta);

return new(dR, dG, dB);
}

// Get Mode

private static bool GetMode(ulong flags, int pos) => ( (flags >> pos) & 1) == 1;

// Get FlipMode

private static bool GetFlipMode(ulong flags) => GetMode(flags, 32);

// Get DiffMode

private static bool GetDiffMode(ulong flags) => GetMode(flags, 33);

// Decode Color

private static void DecodeColor(ulong flags, Span<TextureColor> result)
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

for(int i = 0; i < ETC.TILE_SIZE; i++)
{

for(int j = 0; j < ETC.TILE_SIZE; j++)
{
int modifier = ExtractPlanarBit(flags, i, j);
bool isNegative = ExtractPlanarBit(flags, i, j, 16) == 1;

bool useTable1 = (flipBits && i < 2) || (!flipBits && j < 2);
int delta = GetDelta(useTable1 ? t1 : t2, modifier, isNegative);

result[(i << 2) | j] = ApplyDelta(r1, g1, b1, delta);
}

}

}

// Decode ETC1

public static SKBitmap Decode(Stream reader, int width, int height)
{
return ETC.Decode(reader, width, height, DecodeColor);
}

#endregion
}