using static PVRBase;
using static PVR2Base;

using System;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encodes images with PVRTC2 (4bpp mode)

public static unsafe class PVRII_4BPP_Decoder
{
// Decode Block

private static void DecodeBlock(in PVR2Packet4BPP packet,
                                TextureColor* output,
                                int width,
                                int row,
                                int col,
                                bool useAlpha)
{
bool hardTransition = packet.HardTransition;

var colorA = packet.GetColorA(useAlpha);
var colorB = packet.GetColorB(useAlpha);

uint modData = packet.ModulationData;

DecodePx(colorA, colorB, hardTransition, false, modData, output, width, row, col, false, useAlpha);
}

// Decode Pixels

private static void DecodePixels(ReadOnlySpan<PVR2Packet4BPP> encoded,
                                 TextureColor* plain,
                                 int width,
                                 int height,
                                 bool useAlpha)

{
int blocksPerCol = width / BLOCK_WIDTH_4BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
{
int index = row * blocksPerCol + col;

DecodeBlock(encoded[index], plain, width, row, col, useAlpha);
}

}

// Decode PVR2

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, bool useAlpha)
{
PVR2Base.AdjustSize(ref width, ref height, false);

SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();

int blocksPerCol = width / BLOCK_WIDTH_4BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;
int bufferSize = totalBlocks * 8;

var packets = MemoryMarshal.Cast<byte, PVR2Packet4BPP>(source[.. bufferSize] );

DecodePixels(packets, pixels, width, height, useAlpha);

return image;
}

}