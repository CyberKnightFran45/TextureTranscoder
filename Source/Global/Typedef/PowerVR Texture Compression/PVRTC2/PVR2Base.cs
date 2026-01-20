using static PVRBase;

using System;

// Base class for PVRTC2

public static unsafe class PVR2Base
{
// Modulation Values

public static readonly byte[] MOD_VALUES_2BPP = [ 0, 3, 5, 8 ];
public static readonly byte[] MOD_VALUES_4BPP = [ 0, 2, 4, 5, 6, 8 ];

// Modulation bit pos (2bbp)

public static readonly byte[][] MOD_POS_2BPP =
[
    [ 0,  2,  8, 10, 16, 18, 24, 26 ],
    [ 4,  6, 12, 14, 20, 22, 28, 30 ],
    [ 1,  3,  9, 11, 17, 19, 25, 27 ],
    [ 5,  7, 13, 15, 21, 23, 29, 31 ]
];

// Modulation bit pos (4bpp)

public static readonly byte[][] MOD_POS_4BPP =
[
    [  0,  4,  8, 12, 16, 20, 24, 28 ],
    [  1,  5,  9, 13, 17, 21, 25, 29 ],
    [  2,  6, 10, 14, 18, 22, 26, 30 ],
    [  3,  7, 11, 15, 19, 23, 27, 31 ]
];

// Align dimension to block Size

private static int AlignToBlock(int dim, int blockSize)
{
return (dim + blockSize - 1) / blockSize * blockSize;
}

// Adjust Image Dimensions
 
public static bool AdjustSize(ref int width, ref int height, bool is2BPP)
{
int originalWidth = width;
int originalHeight = height;

int minWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

width = Math.Max(width, minWidth);
height = Math.Max(height, BLOCK_HEIGHT);

width = AlignToBlock(width, minWidth);
height = AlignToBlock(height, BLOCK_HEIGHT);

return width != originalWidth || height != originalHeight;
}

// Evaluate Color

private static TextureColor16 EvaluateColor(in TextureColor16 colorA, in TextureColor16 colorB, uint mod,
                                            bool hardTransition, bool is2BPP, bool useAlpha)
{

if(hardTransition)
return mod < 2 ? colorA : colorB;

int wB = is2BPP ? MOD_VALUES_2BPP[mod] : MOD_VALUES_4BPP[mod];
int wA = 8 - wB;

return TextureHelper.LerpColors(colorA, colorB, wA, wB, useAlpha);
}

#region ========================  ENCODER  ========================

// Get Color Variance

private static long GetVarianceColor(byte a, byte b)
{
var diff = b - a;

return diff * diff;
}

// Expand Color range

private static void ExpandColor(long variance, int pixelCount, ref byte min, ref byte max)
{
const int MAX_VARIANCE = 100;

if(variance <= pixelCount * MAX_VARIANCE)
return;

min = (byte)Math.Max(0, min - 4);
max = (byte)Math.Min(255, max + 4);
}

// Calculate BoundingBox

private static void GetBounds(TextureColor* pixels, int width, int startX, int startY, int blockWidth,
                              out TextureColor16 min, out TextureColor16 max)
{
byte minR = 255, minG = minR, minB = minG, minA = minG;
byte maxR = 0, maxG = maxR, maxB = maxG, maxA = maxB;

for(int y = 0; y < BLOCK_HEIGHT; y++)
{

for(int x = 0; x < blockWidth; x++)
{
int offset = (startY + y) * width + startX + x;
var px = pixels[offset];

byte r = px.Red;
byte g = px.Green;
byte b = px.Blue;
byte a = px.Alpha;

minR = Math.Min(minR, r);
minG = Math.Min(minG, g);
minB = Math.Min(minB, b);
minA = Math.Min(minA, a);

maxR = Math.Max(maxR, r);
maxG = Math.Max(maxG, g);
maxB = Math.Max(maxB, b);
maxA = Math.Max(maxA, a);
}

}

long varR = 0, varG = varR, varB = varG, varA = varB;

byte avgR = TextureHelper.Midpoint(minR, maxR);
byte avgG = TextureHelper.Midpoint(minG, maxG);
byte avgB = TextureHelper.Midpoint(minB, maxB);
byte avgA = TextureHelper.Midpoint(minA, maxA);
    
for(int y = 0; y < BLOCK_HEIGHT; y++)
{

for(int x = 0; x < blockWidth; x++)
{
int offset = (startY + y) * width + startX + x;
var px = pixels[offset];

varR += GetVarianceColor(avgR, px.Red);
varG += GetVarianceColor(avgG, px.Green);
varB += GetVarianceColor(avgB, px.Blue);
varA += GetVarianceColor(avgA, px.Alpha);
}

}

int totalPx = blockWidth * BLOCK_HEIGHT;

ExpandColor(varR, totalPx, ref minR, ref maxR);
ExpandColor(varG, totalPx, ref minG, ref maxG);
ExpandColor(varB, totalPx, ref minB, ref maxB);
ExpandColor(varA, totalPx, ref minA, ref maxA);  

min = new(minR, minG, minB, minA);
max = new(maxR, maxG, maxB, maxA);
}

// Get Pixel Modulation

private static uint GetPxMod(in TextureColor16 colorA, in TextureColor16 colorB, in TextureColor pixel,
                             bool hardTransition, bool is2BPP, bool useAlpha)
{
uint bestMod = 0;
int bestError = int.MaxValue;

uint maxMod = is2BPP ? 3u : 5u;

for(uint mod = 0; mod < maxMod; mod++)
{
var c = EvaluateColor(colorA, colorB, mod, hardTransition, is2BPP, useAlpha);
int error = TextureHelper.ColorDistanceL1(pixel, c, useAlpha);

if(error < bestError)
{
bestError = error;
bestMod = mod;
}

}

return bestMod;
}

// Get Block Modulation

private static uint GetBlockMod(TextureColor* pixels, int width, int startX, int startY,
                                in TextureColor16 colorA, in TextureColor16 colorB,
							    bool hardTransition, int blockWidth, bool is2BPP,
							    bool useAlpha)
{
uint data = 0;

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < blockWidth; x++)
{
int pxOffset = (startY + y) * width + startX + x;
var px = pixels[pxOffset];

uint best = GetPxMod(colorA, colorB, px, hardTransition, is2BPP, useAlpha);

uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;
int bitPos = is2BPP ? MOD_POS_2BPP[y][x] : MOD_POS_4BPP[y][x];

data |= (best & mask) << bitPos;
}

return data;
}

// Evaluate Block

private static int EvaluateBlock(TextureColor* pixels, int width, int startX, int startY,
                                 in TextureColor16 colorA, in TextureColor16 colorB,
                                 uint modData, bool hardTransition, bool modInterpolate,
								 int blockWidth, bool is2BPP, bool useAlpha)
{
int error = 0;

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < blockWidth; x++)
{
int pxOffset = (startY + y) * width + startX + x;
var src = pixels[pxOffset];

int bitPos = is2BPP ? MOD_POS_2BPP[y][x] : MOD_POS_4BPP[y][x];
uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;

uint mod = (modData >> bitPos) & mask;

TextureColor16 diff;

if(modInterpolate)
diff = TextureHelper.LerpColors(colorA, colorB, 4, 4, useAlpha);

else
diff = EvaluateColor(colorA, colorB, mod, hardTransition, is2BPP, useAlpha);

error += TextureHelper.ColorDistanceL1(src, diff, useAlpha);
}

return error;
}

// Encode Packet

public static void EncodePx(TextureColor* pixels, int width, int blockX, int blockY, bool is2BPP,
                            bool useAlpha, out TextureColor16 colorA, out TextureColor16 colorB,
                            out uint bestModData, out bool bestH, out bool bestM)
{
int blockWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

int startX = blockX * blockWidth;
int startY = blockY * BLOCK_HEIGHT;

GetBounds(pixels, width, startX, startY, blockWidth, out colorA, out colorB);

bestModData = 0;

bestH = false;
bestM = false;

int bestError = int.MaxValue;

for(int h = 0; h <= 1; h++)
{
bool hard = h == 1;

uint modData = GetBlockMod(pixels, width, startX, startY, colorA, colorB,
                           hard, blockWidth, is2BPP, useAlpha);

for(int m = 0; m <= 1; m++)
{
bool interp = m == 1;

int err = EvaluateBlock(pixels, width, startX, startY, colorA, colorB, modData,
                        hard, interp, blockWidth, is2BPP, useAlpha);

if(err < bestError)
{
bestError = err;
bestH = hard;

bestM = interp;
bestModData = modData;
}

}

}

}

// Encode Color

public static int EncodeColor(in TextureColor16 color, bool useAlpha, out bool opaque)
{
int a = useAlpha ? BitHelper.QuantizeFrom8(color.Alpha, 3) : 7;
int flags = 0;

if(a == 7)
{
int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g5 = BitHelper.QuantizeFrom8(color.Green, 5);
int b5 = BitHelper.QuantizeFrom8(color.Blue, 5);

flags = BitHelper.Insert(flags, r5, 10, 5);
flags = BitHelper.Insert(flags, g5, 5, 5);
flags = BitHelper.Insert(flags, b5, 0, 5);

opaque = true;
}

else
{
int r4 = BitHelper.QuantizeFrom8(color.Red, 4);
int g4 = BitHelper.QuantizeFrom8(color.Green, 4);
int b4 = BitHelper.QuantizeFrom8(color.Blue, 4);

flags = BitHelper.Insert(flags, a, 12, 3);
flags = BitHelper.Insert(flags, r4, 8, 4);
flags = BitHelper.Insert(flags, g4, 4, 4);
flags = BitHelper.Insert(flags, b4, 0, 4);

opaque = false;
}

return flags;
}

#endregion


#region ========================  DECODER  ========================

// Decode single Pixel

public static void DecodePx(in TextureColor16 colorA, in TextureColor16 colorB, bool hardTransition,
                            bool modInterpolate, uint modData, TextureColor* output, int width,
                            int row, int col, bool is2BPP, bool useAlpha)
{
int blockWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

for(int y = 0; y < BLOCK_HEIGHT; y++)
	
for(int x = 0; x < blockWidth; x++)
{
TextureColor16 final;

if(modInterpolate)
final = TextureHelper.LerpColors(colorA, colorB, 4, 4, useAlpha);

else
{
int bitPos = is2BPP ? MOD_POS_2BPP[y][x] : MOD_POS_4BPP[y][x];
uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;

uint mod = (modData >> bitPos) & mask;

final = EvaluateColor(colorA, colorB, mod, hardTransition, is2BPP, useAlpha);
}

int pxOffset = TextureHelper.GetPxOffset(col, row, x, y, width, blockWidth, BLOCK_HEIGHT);

output[pxOffset] = new(final);
}

}

// Decode Color

public static TextureColor16 DecodeColor(int flags, bool opaque, bool useAlpha)
{
byte a, r, g, b;

if(opaque)
{
a = 255;
r = BitHelper.ExtractAndExpandTo8(flags, 10, 5);
g = BitHelper.ExtractAndExpandTo8(flags, 5, 5);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 5);
}

else
{
a = useAlpha ? BitHelper.ExtractAndExpandTo8(flags, 12, 3) : (byte)255;
r = BitHelper.ExtractAndExpandTo8(flags, 8, 4);
g = BitHelper.ExtractAndExpandTo8(flags, 4, 4);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 4);
}

return new(r, g, b, a);
}

#endregion
}