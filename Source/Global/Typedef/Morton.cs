using System;
using SkiaSharp;

// Sort Images using Linear or Z-Order from G. Morton

public static unsafe class Morton
{
// Lookup Table

private static readonly ushort[] MortonLookup = new ushort[256];

// Init Lookup

static Morton()
{
// Simple interleave for Morton 8-bit

for(int b = 0; b < 256; b++)
{
ushort val = 0;

for (int i = 0; i < 8; i++)
{

if( (b & (1 << i)) != 0)
val |= (ushort)(1 << (i * 2) );

}

MortonLookup[b] = val;
}

}

// Get Morton Number

public static int GetIndex(int x, int y)
{
int low = MortonLookup[x & 0xFF] | (MortonLookup[y & 0xFF] << 1);

int high = (MortonLookup[(x >> 8) & 0xFF] |
           (MortonLookup[(y >> 8) & 0xFF] << 1)) << 16;

return (low | high) >> 1;
}

#region ============ MORTON MAP ==============

// Morton Sequence

private static readonly int[] MORTON_MAP = [ 0, 2, 8, 10, 1, 3, 9, 11, 4, 6, 12, 14, 5, 7, 13, 15 ];

// Combine index

private static int CombineIndex(int highBits, int x, int y, int minDimension, int logMin)
{
int mask = minDimension - 1;

return highBits | ( (x & mask) << logMin) | (y & mask);
}

// Calculate Linear index

private static int GetLinearIndex(int mortonIndex, int minDimension, int logMin, bool isWide, int width)
{
int mortonX = mortonIndex & 0x55555555;
var mortonY = (int)( (mortonIndex & 0xAAAAAAAA) >> 1);

int mortonBase = logMin * 2;
int highBits = mortonIndex >> mortonBase << mortonBase;

int combinedIndex, x , y;

if(isWide)
{
combinedIndex = CombineIndex(highBits, mortonY, mortonX, minDimension, logMin);

x = combinedIndex / minDimension;
y = combinedIndex % minDimension;
}

else
{
combinedIndex = CombineIndex(highBits, mortonX, mortonY, minDimension, logMin);

x = combinedIndex % minDimension;
y = combinedIndex / minDimension;
}

return y * width + x;
}

// Convert from Linear to Morton

public static void ToMap(SKBitmap image)
{
int width = image.Width;
int height = image.Height;

int minDim = Math.Min(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

var minLog = (int)Math.Log(minDim, 2);
bool isWide = width > height;

Span<TextureColor> block = stackalloc TextureColor[16];
int blockOffset = 0;

for(int row = 0; row < height; row += 4)
{

for(int col = 0; col < width; col += 4)
{

for(int i = 0; i < 16; i++)
{
int idx = GetLinearIndex(blockOffset + i, minDim, minLog, isWide, width);

block[i] = pixels[idx];
}

for(int j = 0; j < 16; j++)
{
int idx2 = GetLinearIndex(blockOffset + MORTON_MAP[j], minDim, minLog, isWide, width);

pixels[idx2] = block[j];
}

blockOffset += 16;
}

}

}

// Convert from Morton Map to Linear

public static void FromMap(SKBitmap image)
{
int width = image.Width;
int height = image.Height;

int minDim = Math.Min(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

var minLog = (int)Math.Log(minDim, 2);
bool isWide = width > height;

Span<TextureColor> block = stackalloc TextureColor[16];
int blockOffset = 0;

for(int row = 0; row < height; row += 4)
{

for(int col = 0; col < width; col += 4)
{

for(int i = 0; i < 16; i++)
{
int idx = GetLinearIndex(blockOffset + MORTON_MAP[i], minDim, minLog, isWide, width);

block[i] = pixels[idx];
}

for(int j = 0; j < 16; j++)
{
int idx2 = GetLinearIndex(blockOffset + j, minDim, minLog, isWide, width);

pixels[idx2] = block[j];
}

blockOffset += 16;
}

}

}

#endregion


#region ============== Z-Curve ===============

// X Coordinates

private static readonly int[] MORTON_OFFSETS_X =
[
     0,  4,  0,  4,  8, 12,  8, 12,  0,  4,  0,  4,  8, 12,  8, 12,
    16, 20, 16, 20, 24, 28, 24, 28, 16, 20, 16, 20, 24, 28, 24, 28,
     0,  4,  0,  4,  8, 12,  8, 12,  0,  4,  0,  4,  8, 12,  8, 12,
    16, 20, 16, 20, 24, 28, 24, 28, 16, 20, 16, 20, 24, 28, 24, 28
];

// Y Coordinates

private static readonly int[] MORTON_OFFSETS_Y =
[
    0,  0,  4,  4,  0,  0,  4,  4,  8,  8, 12, 12,  8,  8, 12, 12,
    0,  0,  4,  4,  0,  0,  4,  4,  8,  8, 12, 12,  8,  8, 12, 12,
   16, 16, 20, 20, 16, 16, 20, 20, 24, 24, 28, 28, 24, 24, 28, 28,
   16, 16, 20, 20, 16, 16, 20, 20, 24, 24, 28, 28, 24, 24, 28, 28
];

// Small Morton Blocks (4x4)

private static void ToMortonSmall(TextureColor* pixels, int width, int height, Span<TextureColor> block)
{
int maxBlocks = (width * width) >> 4;

for(int i = 0; i < maxBlocks; i++)
{
int baseX = MORTON_OFFSETS_X[i];
int baseY = MORTON_OFFSETS_Y[i];

int blockStart = i * 16;

for(int row = 0; row < 4; row++)
{
int y = baseY + row;

if(y >= height)
break;

int rowOffset = y * width;

for(int col = 0; col < 4; col++)
{
int x = baseX + col;

if(x >= width)
break;

block[blockStart + (row << 2) + col] = pixels[rowOffset + x];
}

}

}

}

// Big Morton Blocks (32x32)

private static void ToMortonBig(TextureColor* pixels, int width, int height, Span<TextureColor> block)
{
int blockIndex = 0;

for(int macroY = 0; macroY < height; macroY += 32)
{

for(int macroX = 0; macroX < width; macroX += 32)
{

for(int subBlock = 0; subBlock < 64; subBlock++, blockIndex++)
{
int baseX = macroX + MORTON_OFFSETS_X[subBlock];
int baseY = macroY + MORTON_OFFSETS_Y[subBlock];

int blockStart = blockIndex * 16;

for(int row = 0; row < 4; row++)
{
int y = baseY + row;

if(y >= height)
break;

int rowOffset = y * width;

for(int col = 0; col < 4; col++)
{
int x = baseX + col;

if(x >= width)
break;

block[blockStart + (row << 2) + col] = pixels[rowOffset + x];
}

}

}

}

}

}

// Convert Linear to Morton

public static void ToCurve(SKBitmap image)
{
int width = image.Width;
int height = image.Height;

int square = width * height;
var pixels = (TextureColor*)image.GetPixels().ToPointer();

using NativeMemoryOwner<TextureColor> mOwner = new(square);
var mortonBlock = mOwner.AsSpan();

int maxDim = Math.Max(width, height);

if(maxDim < 32)
ToMortonSmall(pixels, width, height, mortonBlock);

else
ToMortonBig(pixels, width, height, mortonBlock);

for(int i = 0; i < square; i++)
pixels[i] = mortonBlock[i];

}

// Small Morton Blocks (4x4)

private static void FromMortonSmall(TextureColor* pixels, int width, int height, Span<TextureColor> block)
{
int maxBlocks = (width * width) >> 4;

for(int i = 0; i < maxBlocks; i++)
{
int baseX = MORTON_OFFSETS_X[i];
int baseY = MORTON_OFFSETS_Y[i];

int blockStart = i * 16;

for(int row = 0; row < 4; row++)
{
int y = baseY + row;

if(y >= height)
break;

int rowOffset = y * width;

for(int col = 0; col < 4; col++)
{
int x = baseX + col;

if(x >= width)
break;

pixels[rowOffset + x] = block[blockStart + (row << 2) + col];
}

}

}

}

// Big Morton Blocks (32x32)

private static void FromMortonBig(TextureColor* pixels, int width, int height, Span<TextureColor> block)
{
int blockIndex = 0;

for(int macroY = 0; macroY < height; macroY += 32)
{

for(int macroX = 0; macroX < width; macroX += 32)
{

for(int subBlock = 0; subBlock < 64; subBlock++, blockIndex++)
{
int baseX = macroX + MORTON_OFFSETS_X[subBlock];
int baseY = macroY + MORTON_OFFSETS_Y[subBlock];

int blockStart = blockIndex * 16;

for(int row = 0; row < 4; row++)
{
int y = baseY + row;

if(y >= height)
break;

int rowOffset = y * width;

for(int col = 0; col < 4; col++)
{
int x = baseX + col;

if(x >= width)
break;

pixels[rowOffset + x] = block[blockStart + (row << 2) + col];
}

}

}

}

}

}

// Convert from Morton to Linear

public static void FromCurve(SKBitmap image)
{
int width = image.Width;
int height = image.Height;

int square = width * height;
var pixels = (TextureColor*)image.GetPixels().ToPointer();

using NativeMemoryOwner<TextureColor> mOwner = new(square);
var mortonBlock = mOwner.AsSpan();

for(int i = 0; i < square; i++)
mortonBlock[i] = pixels[i];

int maxDim = Math.Max(width, height);

if(maxDim < 32)
FromMortonSmall(pixels, width, height, mortonBlock);

else
FromMortonBig(pixels, width, height, mortonBlock);

}

#endregion
}