using System;
using SkiaSharp;

// Parse DXT5 Images in RGBA Order (with Padding)

public static class DXT5_RGBA_Padding
{
// Compute block count

private static int ComputeBlockCount(int dim) => (dim + 3) / 4;

// Adjust width

private static int AdjustWidth(int width) => ComputeBlockCount(width) * 4;

// Remove padding from texture

private static NativeBuffer RemovePadding(ReadOnlySpan<byte> source, int width, int height, int blockSize)
{
int blockRows = ComputeBlockCount(height);

int rowSize = AdjustWidth(width) * 4;
int paddingPerRow = blockSize - rowSize;

int outputLen = rowSize * blockRows;

NativeBuffer rawTexture = new(outputLen);
int srcOffset = 0;

for(int row = 0; row < blockRows; row++)
{
var dstOffset = (ulong)(row * rowSize);
rawTexture.CopyFrom(source, srcOffset, dstOffset, rowSize);

srcOffset += rowSize + paddingPerRow;
}

return rawTexture;
}

// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, int blockSize)
{
using var raw = RemovePadding(source, width, height, blockSize);

return DXTDecoder.Decode(raw.GetView(), width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);
}

// Add padding to texture

private static NativeBuffer AddPadding(ReadOnlySpan<byte> source, int width, int height, int blockSize)
{
int blockRows = ComputeBlockCount(height); // DXT data is stored in rows of 4 pixels
int totalSize = blockSize * blockRows;

NativeBuffer padded = new(totalSize);
padded.Fill(0xCD);

int newWidth = AdjustWidth(width);
int rowSize = newWidth * 4; // Bytes per row of blocks (DXT5: 4x4 block - 16 bytes per block)

for(int row = 0; row < blockRows; row++)
{
int srcOffset = row * rowSize;
var dstOffset = (ulong)(row * blockSize);

padded.CopyFrom(source, srcOffset, dstOffset, rowSize);
}

return padded;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, int blockSize)
{
using var encoded = DXTEncoder.Encode(image, false, null, DXT4_RGBA.EncodeAlpha);

return AddPadding(encoded.GetView(), image.Width, image.Height, blockSize);
}

}