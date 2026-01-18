using static PVRBase;
using static PVR2Base;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encodes images with PVRTC2 (2bpp mode)

public static unsafe class PVRII_2BPP_Decoder
{
// Decode Block

private static void DecodeBlock(in PVR2Packet2BPP packet, TextureColor* output, int width,
                                int row, int col, bool useAlpha)
{
bool hard = packet.HardTransition;
bool interp = packet.InterpolatedModulation;

var colorA = packet.GetColorA(useAlpha);
var colorB = packet.GetColorB(useAlpha);

uint modData = packet.ModulationData;

DecodePx(colorA, colorB, hard, interp, modData, output, width, row, col, true, useAlpha);
}

// Decode Pixels

private static void DecodePixels(ReadOnlySpan<PVR2Packet2BPP> encoded, TextureColor* plain,
                                 int width, int height, bool useAlpha)

{
int blocksPerCol = width / BLOCK_WIDTH_2BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
{
int index = row * blocksPerCol + col;

DecodeBlock(encoded[index], plain, width, row, col, useAlpha);
}

}

// Decode PVR2

public static SKBitmap Decode(Stream reader, int width, int height, bool useAlpha)
{
PVR2Base.AdjustSize(ref width, ref height, true);

SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading raw data...");

int blocksPerCol = width / BLOCK_WIDTH_2BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;

int bufferSize = totalBlocks * 8;
using var rOwner = reader.ReadPtr(bufferSize);

var rawBytes = rOwner.AsSpan();
var packets = MemoryMarshal.Cast<byte, PVR2Packet2BPP>(rawBytes);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");
DecodePixels(packets, pixels, width, height, useAlpha);

TraceLogger.WriteActionEnd();

return image;
}

}