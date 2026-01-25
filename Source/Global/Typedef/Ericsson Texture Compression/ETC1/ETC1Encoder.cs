using static ETCBase;

using System;
using System.IO;
using SkiaSharp;

// Encodes images with ETC1

public static class ETC1Encoder
{
// Interpolate Color by weights

private static byte WeightedBlend(byte min, byte max, float ratio, float ratio2)
{
return TextureHelper.ColorClamp(min * ratio + max * ratio2);
}

// Get ETC1 Modifier

private static int GenModifier(ReadOnlySpan<TextureColor> pixels, out TextureColor baseColor)
{
var minColor = TextureColor.MinValue;
var maxColor = TextureColor.MaxValue;

int minY = int.MaxValue;
int maxY = int.MinValue;

for(int i = 0; i < 8; i++)
{
var px = pixels[i];

if(px.Alpha == 0)
continue;

int y = TextureHelper.GetColorMean(px.Red, px.Green, px.Blue);

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

int diffMean = TextureHelper.GetColorRangeMean(minColor, maxColor);
int modDiff = int.MaxValue;

int modifier = -1;
int mode = -1;

for(int i = 0; i < 8; i++)
{
int modLow = MOD_TABLE[i, 0];
int modHigh = MOD_TABLE[i, 1];

int deltaL = TextureHelper.ColorClamp(modLow * 2);
int deltaM = TextureHelper.ColorClamp(modLow + modHigh);
int deltaH = TextureHelper.ColorClamp(modHigh * 2);

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

mR = WeightedBlend(minColor.Red, maxColor.Red, ratio, ratio2);
mG = WeightedBlend(minColor.Green, maxColor.Green, ratio, ratio2);
mB = WeightedBlend(minColor.Blue, maxColor.Blue, ratio, ratio2);
}

else
{
mR = TextureHelper.Midpoint(minColor.Red, maxColor.Red);
mG = TextureHelper.Midpoint(minColor.Green, maxColor.Green);
mB = TextureHelper.Midpoint(minColor.Blue, maxColor.Blue);
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
int baseMean = TextureHelper.GetColorMean(baseColor.Red, baseColor.Green, baseColor.Blue);
int i = 0;

for(int row = startY; row < endY; row++)
{

for(int col = startX; col < endX; col++)
{
int currMean = TextureHelper.GetColorMean(pixels[i].Red, pixels[i].Green, pixels[i].Blue);
int diff = currMean - baseMean;

if(diff < 0)
data |= 1uL << (col * TILE_SIZE + row + 16);

int tDiff1 = Math.Abs(diff) - MOD_TABLE[modifier, 0];
int tDiff2 = Math.Abs(diff) - MOD_TABLE[modifier, 1];

if(Math.Abs(tDiff1) < Math.Abs(tDiff2) )
data |= 1uL << (col * TILE_SIZE + row);

i++;
}

}

}

// Get ChannelStep

private static int GetChannelStep(int src, int dst) => TextureHelper.GetChannelStep(src, dst, 8);

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

int rDiff = GetChannelStep(r1, r2);
int gDiff = GetChannelStep(g1, g2);
int bDiff = GetChannelStep(b1, b2);

bool nonEndpoint = TextureHelper.IsColorInsideRange(rDiff, gDiff, bDiff, -4, 3);

if(nonEndpoint)
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

// Encode Pixels

private static ulong EncodePixels(ReadOnlySpan<TextureColor> pixels)
{
ulong x = GetX(pixels);
ulong y = GetY(pixels);

Span<TextureColor> rawX = stackalloc TextureColor[16];
ETC1Decoder.DecodePixels(x, rawX);

int scoreX = TextureHelper.BlockDistanceL1(pixels, rawX, 4, 4, false);

Span<TextureColor> rawY = stackalloc TextureColor[16];
ETC1Decoder.DecodePixels(y, rawY);

int scoreY = TextureHelper.BlockDistanceL1(pixels, rawY, 4, 4, false);

return scoreX < scoreY ? x : y;
}

// Encode ETC1

public static int Encode(Stream writer, SKBitmap image)
{
return ETCBase.Encode(writer, image, EncodePixels);
}

}