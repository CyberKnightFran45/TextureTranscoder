using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Abstraccion of the ETC algorithm

public static unsafe class ETCBase
{
// Tile Size (4x4)

public const int TILE_SIZE = 4;

// ETC Modifiers

public static readonly int[,] MOD_TABLE =
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

#region ======================= PARSER =======================

// Encoder delegate

public delegate ulong ETCEncoder(ReadOnlySpan<TextureColor> block);

// Alpha writer

public delegate ulong EACEncoder(ReadOnlySpan<TextureColor> block);

// Generic encoder

public static int Encode(Stream writer, SKBitmap image, ETCEncoder encodeFunc,
                         EACEncoder alphaFunc = null)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

bool useAlpha = alphaFunc != null;

TraceLogger.WriteActionStart("Reading pixels...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int ulongsPerBlock = useAlpha ? 2 : 1;

int blockSize = totalBlocks * ulongsPerBlock;

using NativeMemoryOwner<ulong> cOwner = new(blockSize);
var colorInfo = cOwner.AsSpan();

Span<TextureColor> block = stackalloc TextureColor[16];

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{

for(int i = 0; i < TILE_SIZE; i++)

for(int j = 0; j < TILE_SIZE; j++)
{
int srcX = row * TILE_SIZE + j;
int srcY = col * TILE_SIZE + i;

bool shouldCopy = srcX < width && srcY < height;

block[i * TILE_SIZE + j] = shouldCopy ? pixels[srcY * width + srcX] : default;
}

int blockIndex = (col * blocksPerRow + row) * ulongsPerBlock;

if(useAlpha)
{
colorInfo[blockIndex] = alphaFunc(block);
colorInfo[blockIndex + 1] = encodeFunc(block);
}

else
colorInfo[blockIndex] = encodeFunc(block);

}

}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(colorInfo);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return blocksPerRow;
}

// Decoder delegate

public delegate void ETCDecoder(ulong flags, Span<TextureColor> block);

// Alpha reader

public delegate void EACDecoder(ulong flags, Span<byte> block);

// Generic decoder

public static SKBitmap Decode(Stream reader, int width, int height, ETCDecoder decodeFunc,
                              EACDecoder alphaFunc = null)
{
SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

bool useAlpha = alphaFunc != null;

TraceLogger.WriteActionStart("Reading raw data...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int ulongsPerBlock = useAlpha ? 2 : 1;

int bufferSize = totalBlocks * ulongsPerBlock * 8;
using var rOwner = reader.ReadPtr(bufferSize);

var rawBytes = rOwner.AsSpan();
var colorInfo = MemoryMarshal.Cast<byte, ulong>(rawBytes);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");

Span<TextureColor> block = stackalloc TextureColor[16];
Span<byte> alphas = useAlpha ? stackalloc byte[16] : default;

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = (col * blocksPerRow + row) * ulongsPerBlock;

if(useAlpha)
{
alphaFunc(colorInfo[blockIndex], alphas);
decodeFunc(colorInfo[blockIndex + 1], block);

for(int i = 0; i < 16; i++)
block[i].Alpha = alphas[i];

}

else
decodeFunc(colorInfo[blockIndex], block);

for(int i = 0; i < TILE_SIZE; i++)

for (int j = 0; j < TILE_SIZE; j++)
{
int dstX = row * TILE_SIZE + j;
int dstY = col * TILE_SIZE + i;

if(dstX < width && dstY < height)
pixels[dstY * width + dstX] = block[i * TILE_SIZE + j];

}

}

}

TraceLogger.WriteActionEnd();

return image;
}

#endregion


#region ======================= UTILITIES =======================

// Get Left Colors as 2x4 Tile

public static void GetLeftColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
TextureHelper.ExtractSubBlock(pixels, result, 0, 0, 2, 4, TILE_SIZE);
}

// Get Right Colors as 2x4 Tile

public static void GetRightColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
TextureHelper.ExtractSubBlock(pixels, result, 2, 0, 2, 4, TILE_SIZE);
}

// Get Top Colors as 4x2 Tile

public static void GetTopColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
TextureHelper.ExtractSubBlock(pixels, result, 0, 0, 4, 2, TILE_SIZE);
}

// Get Bottom Colors as 4x2 Tile

public static void GetBottomColors(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result)
{
TextureHelper.ExtractSubBlock(pixels, result, 0, 2, 4, 2, TILE_SIZE);
}

#endregion
}