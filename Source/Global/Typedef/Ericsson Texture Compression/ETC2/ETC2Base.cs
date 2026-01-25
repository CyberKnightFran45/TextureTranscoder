using System;

// Base class for ETC2 compression

public static class ETC2Base
{
// Generate colors for a sub-block

public static void GenerateSubBlockColors(in TextureColor baseColor, int table, 
                                          Span<TextureColor> colors)
{
int modLow = ETCBase.MOD_TABLE[table, 0];
int modHigh = ETCBase.MOD_TABLE[table, 1];

int r = baseColor.Red;
int g = baseColor.Green;
int b = baseColor.Blue;

colors[0] = TextureHelper.AddColorBias(r, g, b, -modHigh);
colors[1] = TextureHelper.AddColorBias(r, g, b, -modLow);
colors[2] = TextureHelper.AddColorBias(r, g, b, modLow);
colors[3] = TextureHelper.AddColorBias(r, g, b, modHigh);
}

// Get base Color

private static bool TryCalculateBaseColor(ReadOnlySpan<TextureColor> subBlock, int modifier,
                                          out TextureColor baseColor, out int error)
{
baseColor = default;
error = 0;

for(int i = 0; i < 8; i++)
baseColor.Sum(subBlock[i]);

baseColor.Divide(8);

int modLow = ETCBase.MOD_TABLE[modifier, 0];
int modHigh = ETCBase.MOD_TABLE[modifier, 1];

Span<TextureColor> palette = stackalloc TextureColor[4];

palette[0] = baseColor - modHigh;
palette[1] = baseColor - modLow;
palette[2] = baseColor + modLow;
palette[3] = baseColor + modHigh;

for(int i = 0; i < 8; i++)
{
int bestError = int.MaxValue;
int curError = 0;

for(int p = 0; p < 4; p++)
{
curError += Math.Abs(subBlock[i].Red - palette[p].Red);
curError += Math.Abs(subBlock[i].Green - palette[p].Green);
curError += Math.Abs(subBlock[i].Blue  - palette[p].Blue);

if(curError < bestError)
bestError = curError;

}

error += bestError;
}

return true;
}

// Find best modifier for a 4x2 sub-block
	
public static int FindBestModifier(ReadOnlySpan<TextureColor> subBlock, 
                                   out TextureColor baseColor, 
                                   out int error)
{
int bestMod = 0;
int bestError = int.MaxValue;

TextureColor bestColor = default;

for(int mod = 0; mod < 8; mod++)
{

if(TryCalculateBaseColor(subBlock, mod, out var color, out var curError) )
{

if(curError < bestError)
{
bestError = curError;

bestMod = mod;
bestColor = color;
}

}

}

baseColor = bestColor;
error = bestError;

return bestMod;
}

// Get ChanelStep

private static int GetChannelStep(int src, int dst) => TextureHelper.GetChannelStep(src, dst, 4);

// Encode base colors into block data

public static void EncodeBaseColors(ref ulong blockData, in TextureColor c1, in TextureColor c2, 
                                    bool diffMode, bool punchThrough)
{
int r1 = c1.Red;
int g1 = c1.Green;
int b1 = c1.Blue;

int r2 = c2.Red;
int g2 = c2.Green;
int b2 = c2.Blue;

if(diffMode)
{
r1 >>= 3;
g1 >>= 3;
b1 >>= 3;

int rDiff = GetChannelStep(r1, r2);
int gDiff = GetChannelStep(g1, g2);
int bDiff = GetChannelStep(b1, b2);

bool nonEndpoint = TextureHelper.IsColorInsideRange(rDiff, gDiff, bDiff, -4, 4);

if(nonEndpoint)
{
blockData = BitHelper.Insert(blockData, (ulong)(r1 + rDiff), 59, 5);
blockData = BitHelper.Insert(blockData, (ulong)(g1 + gDiff), 51, 5);
blockData = BitHelper.Insert(blockData, (ulong)(b1 + bDiff), 43, 5);

blockData = BitHelper.Insert(blockData, (ulong)r1, 63, 5);
blockData = BitHelper.Insert(blockData, (ulong)g1, 55, 5);
blockData = BitHelper.Insert(blockData, (ulong)b1, 47, 5);
}

}
	
else
{
r1 >>= 4;
g1 >>= 4;
b1 >>= 4;

r2 >>= 4;
g2 >>= 4;
b2 >>= 4;

blockData = BitHelper.Insert(blockData, (ulong)r1, 60, 4);
blockData = BitHelper.Insert(blockData, (ulong)g1, 52, 4);
blockData = BitHelper.Insert(blockData, (ulong)b1, 44, 4);
            
blockData = BitHelper.Insert(blockData, (ulong)r2, 56, 4);
blockData = BitHelper.Insert(blockData, (ulong)g2, 48, 4);
blockData = BitHelper.Insert(blockData, (ulong)b2, 40, 4);
}

}

// Decode base colors from block data
	
public static void DecodeBaseColors(ulong blockData, bool diff, out TextureColor c1, out TextureColor c2)
{
byte r1, g1, b1, r2, g2, b2;

if(diff)
{
var r1_b5 = (int)BitHelper.Extract(blockData, 63, 5);
var g1_b5 = (int)BitHelper.Extract(blockData, 55, 5);
var b1_b5 = (int)BitHelper.Extract(blockData, 47, 5);
 
var dR = (int)BitHelper.Extract(blockData, 59, 3);
var dG = (int)BitHelper.Extract(blockData, 51, 3);
var dB = (int)BitHelper.Extract(blockData, 43, 3);

dR = (dR << 29) >> 29;
dG = (dG << 29) >> 29;
dB = (dB << 29) >> 29;

int r2_b3 = r1_b5 + dR;
int g2_b3 = g1_b5 + dG;
int b2_b3 = b1_b5 + dB;

r1 = BitHelper.ExpandTo8(r1_b5, 5);
g1 = BitHelper.ExpandTo8(g1_b5, 5);
b1 = BitHelper.ExpandTo8(b1_b5, 5);

r2 = BitHelper.ExpandTo8(r2_b3, 5);
g2 = BitHelper.ExpandTo8(g2_b3, 5);
b2 = BitHelper.ExpandTo8(b2_b3, 5);
}

else
{
var r1_b4 = (int)BitHelper.Extract(blockData, 60, 4);
var g1_b4 = (int)BitHelper.Extract(blockData, 52, 4);
var b1_b4 = (int)BitHelper.Extract(blockData, 44, 4);

var r2_b4 = (int)BitHelper.Extract(blockData, 56, 4);
var g2_b4 = (int)BitHelper.Extract(blockData, 48, 4);
var b2_b4 = (int)BitHelper.Extract(blockData, 40, 4);
 
r1 = BitHelper.ExpandTo8(r1_b4, 4);
g1 = BitHelper.ExpandTo8(g1_b4, 5);
b1 = BitHelper.ExpandTo8(b1_b4, 4);

r2 = BitHelper.ExpandTo8(r2_b4, 4);
g2 = BitHelper.ExpandTo8(g2_b4, 4);
b2 = BitHelper.ExpandTo8(b2_b4, 4);
}

c1 = new(r1, g1, b1);
c2 = new(r2, g2, b2); 
}

// Encode pixel indices

public static void EncodePixelIndices(ref ulong blockData, ReadOnlySpan<TextureColor> subBlock,
                                      in TextureColor baseColor, int modifier,
                                      int startX, int startY)
{
int modLow = ETCBase.MOD_TABLE[modifier, 0];
int modHigh = ETCBase.MOD_TABLE[modifier, 1];
        
for(int i = 0; i < 8; i++)
{
int x = startX + (i % 2);
int y = startY + (i / 2);

int pixelIndex = (y * 4) + x;
int bitPos = pixelIndex;
int msbPos = bitPos + 16;

int dist = TextureHelper.GetColorRangeMean(baseColor, subBlock[i] );        
int index;

if(dist < -modHigh)
index = 0;

else if(dist < -modLow)
index = 1;

else if(dist < modLow)
index = 2;

else
index = 3;

int lsb = index & 1;
int msb = (index >> 1) & 1;

if(lsb == 1)
blockData |= 1UL << bitPos;

if(msb == 1)
blockData |= 1UL << msbPos;

}

}

// Encode planar colors
	
public static ulong EncodePlanarColors(in TextureColor cO, in TextureColor cH, in TextureColor cV)
{
ulong blockData = 0;

blockData = BitHelper.Insert(blockData, 2uL, 60, 4);

var rO = (ulong)(cO.Red >> 2);
var gO = (ulong)(cO.Green >> 2);
var bO = (ulong)(cO.Blue >> 2);

blockData = BitHelper.Insert(blockData, rO , 54, 6);
blockData = BitHelper.Insert(blockData, gO, 48, 6);
blockData = BitHelper.Insert(blockData, bO, 42, 6);

var rH = (ulong)(cH.Red >> 2);
var gH = (ulong)(cH.Green >> 2);
var bH = (ulong)(cH.Blue >> 2);
  
blockData = BitHelper.Insert(blockData, rH, 36, 6);
blockData = BitHelper.Insert(blockData, gH, 30, 6);
blockData = BitHelper.Insert(blockData, bH, 24, 6);

var rV = (ulong)(cV.Red >> 2);
var gV = (ulong)(cV.Green >> 2);
var bV = (ulong)(cV.Blue >> 2);

blockData = BitHelper.Insert(blockData, rV, 18, 6);
blockData = BitHelper.Insert(blockData, gV, 12, 6);
blockData = BitHelper.Insert(blockData, bV, 6, 6);
        
return blockData;
}

}