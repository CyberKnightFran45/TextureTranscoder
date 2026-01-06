using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Abstraccion of the ETC algorithm

public static unsafe class ETC
{
// Tile Size (4x4)

public const int TILE_SIZE = 4;

#region ======================= PARSER =======================

// Encoder delegate

public delegate ulong ETCEncoder(ReadOnlySpan<TextureColor> block);

// Generic encoder

public static int Encode(Stream writer, SKBitmap image, ETCEncoder encodeFunc)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

TraceLogger.WriteActionStart("Reading pixels...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;

using NativeMemoryOwner<ulong> cOwner = new(totalBlocks);
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

colorInfo[col * blocksPerRow + row] = encodeFunc(block);
}

}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(colorInfo);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return totalBlocks * 8;
}

// Decoder delegate

public delegate void ETCDecoder(ulong flags, Span<TextureColor> block);

// Generic decoder

public static SKBitmap Decode(Stream reader, int width, int height, ETCDecoder decodeFunc)
{
SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading raw data...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int bufferSize = totalBlocks * 8;

using var rOwner = reader.ReadPtr(bufferSize);
var rawBytes = rOwner.AsSpan();

var colorInfo = MemoryMarshal.Cast<byte, ulong>(rawBytes);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");

Span<TextureColor> block = stackalloc TextureColor[16];

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = col * blocksPerRow + row;
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

// Get Mean

public static int GetMean(int r, int g, int b) => (r + g + b) / 3;

// Interpolate Color

public static byte InterpolateColor(byte min, byte max, float ratio, float ratio2)
{
return TextureHelper.ColorClamp(min * ratio + max * ratio2);
}

// Get AverageColor

public static byte AverageColor(byte min, byte max) => (byte)( (min + max) >> 1);

// Get ColorDiff

public static int GetColorDiff(int a, int b) => (b - a) / 8;

// Check ColorFlags

private static bool CheckColorDiff(int flags) => flags > -4 && flags < 3;

// Check if a Color is Different

public static bool IsDiffColor(int r, int g, int b)
{
return CheckColorDiff(r) && CheckColorDiff(g) && CheckColorDiff(b);
}

// Get SubBlock from Tile

public static void ExtractSubBlock(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result,
                                   int startX, int startY, int width, int height)
{

for(int row = 0; row < height; row++)
{

for(int col = 0; col < width; col++)
result[row * width + col] = pixels[(startY + row) * TILE_SIZE + startX + col];
  
}

}

#endregion
}