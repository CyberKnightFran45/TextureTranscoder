using static DXTCommon;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Supports DirectX Texture Compression (DXT)

public static unsafe class DXTDecoder
{
// Decode Color from 565-bits

private static TextureColor ColorFrom565(ushort flags)
{
int r5 = BitHelper.Extract(flags, 11, 5);
int g6 = BitHelper.Extract(flags, 5, 6);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g6, 6);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Compute Colors

private static void ComputeColors(ushort c0, ushort c1, Span<TextureColor> palette, bool hasAlpha)
{
bool shouldInterpolate = hasAlpha || c0 > c1;

var d0 = ColorFrom565(c0);
var d1 = ColorFrom565(c1);

TextureColor d2, d3;

if(shouldInterpolate)
{
d2 = TextureHelper.LerpColors(d0, d1, 2, 1, false);
d3 = TextureHelper.LerpColors(d0, d1, 1, 2, false);
}

else
{
d2 = TextureHelper.LerpColors(d0, d1, 1, 1, false);
d3 = default;
}

palette[0] = d0;
palette[1] = d1;
palette[2] = d2;
palette[3] = d3;
}

// Decode ColorIndices

private static void DecodeIndices(ReadOnlySpan<byte> indices, 
                                  ReadOnlySpan<TextureColor> palette,
                                  Span<TextureColor> block)
{

for(int i = 0; i < TILE_SIZE; i++)
{
byte row = indices[i];

for(int j = 0; j < TILE_SIZE; j++)
{
int blockIndex = (i << 2) | j;
int colorIndex = row & 0b11;

block[blockIndex] = palette[colorIndex];
row >>= 2;
}

}

}


// Decode DXT

public static SKBitmap Decode(Stream reader, int width, int height,
                              DXTBlockDecoder blockDecoder = null,
                              DXTAlphaDecoder alphaFunc = null)
{
SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();
bool useAlpha = alphaFunc != null;

TraceLogger.WriteActionStart("Reading raw data...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int bytesPerBlock = useAlpha ? 16 : 8;

int bufferSize = totalBlocks * bytesPerBlock;

using var rOwner = reader.ReadPtr(bufferSize);
var rawBytes = rOwner.AsSpan();

TraceLogger.WriteActionEnd();

Span<TextureColor> block = stackalloc TextureColor[16];
Span<TextureColor> palette = stackalloc TextureColor[4];

Span<byte> colorIndices = stackalloc byte[4];
Span<byte> alphas = useAlpha ? stackalloc byte[16] : default;

TraceLogger.WriteActionStart("Writing pixels...");

int colorOffset = useAlpha ? 4 : 0;

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = col * blocksPerRow + row;
var blockData = rawBytes.Slice(blockIndex * bytesPerBlock, bytesPerBlock);

var currBlock = MemoryMarshal.Cast<byte, ushort>(blockData);

if(useAlpha)
alphaFunc(currBlock[.. 4], alphas);

var colorInfo = currBlock.Slice(colorOffset, 4);

ushort c0 = colorInfo[0];
ushort c1 = colorInfo[1];
ushort f0 = colorInfo[2];
ushort f1 = colorInfo[3];

colorIndices[0] = (byte)(f0 & 0xFF);
colorIndices[1] = (byte)(f0 >> 8);
colorIndices[2] = (byte)(f1 & 0xFF);
colorIndices[3] = (byte)(f1 >> 8);

ComputeColors(c0, c1, palette, useAlpha);
DecodeIndices(colorIndices, palette, block);

blockDecoder?.Invoke(alphas, block);

for(int i = 0; i < TILE_SIZE; i++)
{
	
for(int j = 0; j < TILE_SIZE; j++)
{
int y = col * TILE_SIZE + i;
int x = row * TILE_SIZE + j;

if(x < width && y < height)
pixels[y * width + x] = block[(i << 2) | j];

}

}

}

}

TraceLogger.WriteActionEnd();

return image;
}

}