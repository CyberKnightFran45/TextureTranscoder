using System;
using System.Buffers.Binary;
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

// Reverse endianness

private static void ReverseEndian(ref ulong flags)
{
flags = BinaryPrimitives.ReverseEndianness(flags);
}

// Swap color bits

private static void SwapColors(Span<ulong> block, bool useAlpha)
{

if(useAlpha)
{
(block[0], block[1]) = (block[1], block[0]);

ReverseEndian(ref block[0] );
ReverseEndian(ref block[1] );
}

else
ReverseEndian(ref block[0] );

}

// Encode raw ETC1 block

private static void EncodeBlock(TextureColor* pixels,
                                Span<ulong> output,
                                int width, 
                                int height,
                                int blockX,
                                int blockY,
                                ETCEncoder encodeFunc,
                                EACEncoder alphaFunc,
								bool useAlpha,
                                bool swapBytes)
{
Span<TextureColor> block = stackalloc TextureColor[16];

for(int i = 0; i < TILE_SIZE; i++)
{

for(int j = 0; j < TILE_SIZE; j++)
{
int srcX = blockX * TILE_SIZE + j;
int srcY = blockY * TILE_SIZE + i;

bool shouldCopy = srcX < width && srcY < height;

block[i * TILE_SIZE + j] = shouldCopy ? pixels[srcY * width + srcX] : default;
}

}
 
if(useAlpha)
{
output[0] = alphaFunc(block);
output[1] = encodeFunc(block);
}

else
output[0] = encodeFunc(block);

if(swapBytes)
SwapColors(output, useAlpha);

}

// Encode ETC into span

internal static void Encode(SKBitmap image,
                                Span<byte> destination,       
                                ETCEncoder encodeFunc,
                                EACEncoder alphaFunc = null,
                                bool swapBytes = false)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

bool useAlpha = alphaFunc != null;

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int ulongsPerBlock = useAlpha ? 2 : 1;

int blockSize = totalBlocks * ulongsPerBlock;
int minSize = blockSize * 8;

if(destination.Length < minSize)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var colorInfo = MemoryMarshal.Cast<byte, ulong>(destination[.. minSize] );

for(int y = 0; y < blocksPerCol; y++)

for(int x = 0; x < blocksPerRow; x++)
{
int idx = (y * blocksPerRow + x) * ulongsPerBlock;
var block = colorInfo.Slice(idx, ulongsPerBlock);

EncodeBlock(pixels, block, width, height, x, y, encodeFunc, alphaFunc, useAlpha, swapBytes);
}

}

public static NativeBuffer Encode(SKBitmap image,
                                  ETCEncoder encodeFunc,
                                  EACEncoder alphaFunc = null,
                                  bool swapBytes = false)
{
int blocksPerRow = TextureHelper.GetBlockDim(image.Width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(image.Height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int ulongsPerBlock = alphaFunc != null ? 2 : 1;

int blockSize = totalBlocks * ulongsPerBlock;
int blockSizeInBytes = blockSize * 8;

NativeBuffer output = new(blockSizeInBytes);
Encode(image, output.AsSpan(), encodeFunc, alphaFunc, swapBytes);

return output;
}

// Decoder delegate

public delegate void ETCDecoder(ulong flags, Span<TextureColor> block);

// Alpha reader

public delegate void EACDecoder(ulong flags, Span<byte> block);

// Decode raw ETC1 block

private static void DecodeBlock(Span<byte> rawBlock, 
                                TextureColor* pixels,
                                int width,
                                int height,
                                int blockX,
                                int blockY,
                                ETCDecoder decodeFunc,
                                EACDecoder alphaFunc,
								bool useAlpha,
                                bool swapBytes)
{
var colorInfo = MemoryMarshal.Cast<byte, ulong>(rawBlock);

if(swapBytes)
SwapColors(colorInfo, useAlpha);

Span<TextureColor> block = stackalloc TextureColor[16];
Span<byte> alphas = useAlpha ? stackalloc byte[16] : default;

if(useAlpha)
{
alphaFunc(colorInfo[0], alphas);
decodeFunc(colorInfo[1], block);

for(int i = 0; i < 16; i++)
block[i].Alpha = alphas[i];

}

else
decodeFunc(colorInfo[0], block);

for(int i = 0; i < TILE_SIZE; i++)
{

for(int j = 0; j < TILE_SIZE; j++)
{
int dstX = blockX * TILE_SIZE + j;
int dstY = blockY * TILE_SIZE + i;

if(dstX < width && dstY < height)
pixels[dstY * width + dstX] = block[i * TILE_SIZE + j];

}

}

}

// Generic decoder

public static SKBitmap Decode(ReadOnlySpan<byte> source,
                              int width,
                              int height,
                              ETCDecoder decodeFunc,
                              EACDecoder alphaFunc = null,
                              bool swapBytes = false)
{
SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();
bool useAlpha = alphaFunc != null;

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int bytesPerBlock = useAlpha ? 16 : 8;

int bufferSize = totalBlocks * bytesPerBlock;

using NativeBuffer localSrc = new(source.Length);
localSrc.CopyFrom(source);

var rawBytes = localSrc.AsSpan(0, bufferSize);

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = col * blocksPerRow + row;
int byteOffset = blockIndex * bytesPerBlock;

var block = rawBytes.Slice(byteOffset, bytesPerBlock);

DecodeBlock(block, pixels, width, height, row, col, decodeFunc, alphaFunc, useAlpha, swapBytes);
}

}

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

// Compute compressed texture size

public static int ComputeETCSize(int width, int height)
{
int blocksPerCol = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerRow = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerCol * blocksPerRow;

return totalBlocks * 8;
}

#endregion
}