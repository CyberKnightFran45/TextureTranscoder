using static PVRBase;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Decodes PVR Images

public static unsafe class PVRDecoder
{
// Extract Color as is (no interpolation)

private static TextureColor16 ExtractColor(in TextureColor16 colorA, in TextureColor16 colorB,
                                           uint modValue, bool useAlpha)
{

return modValue switch
{
1 => TextureHelper.LerpColors(colorA, colorB, 5, 3, useAlpha),
2 => TextureHelper.LerpColors(colorA, colorB, 3, 5, useAlpha),
3 => colorB,
_ => colorA,
};

}

// Get Color Interpolated

private static TextureColor16 GetInterpolated(in TextureColor16 colorA, in TextureColor16 colorB,
                                              uint modValue, bool useAlpha)
{

return modValue switch
{
1 => TextureHelper.LerpColors(colorA, colorB, 3, 1, useAlpha),
2 => TextureHelper.LerpColors(colorA, colorB, 2, 2, useAlpha),
3 => TextureHelper.LerpColors(colorA, colorB, 1, 3, useAlpha),
_ => colorA,
};

}

// Interpolate Colors

private static TextureColor16 InterpColors(in TextureColor16 colorA, in TextureColor16 colorB,
                                         uint modValue, uint mode, bool useAlpha)
{

if(mode == 0)
return ExtractColor(colorA, colorB, modValue, useAlpha);

return GetInterpolated(colorA, colorB, modValue, useAlpha);
}

// Apply Modulation

private static void ApplyMod(uint data, int x, int y, bool is2BPP,
                             out uint modValue, out uint mode)
{
mode = is2BPP ? (data >> 25) & 1 : (data >> 29) & 1;

int offset = is2BPP ? PX_OFFSETS_2BPP[y][x] : PX_OFFSETS_4BPP[y][x];
uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;

modValue = (data >> offset) & mask;
}

// Decode single Pixel

private static void DecodePx(in PVRPacket packet, TextureColor* output, int width,
                             int wordWidth, int row, int col,
							 bool is2BPP, bool useAlpha)
{
var colorA = packet.GetColorA(useAlpha);
var colorB = packet.GetColorB(useAlpha);

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < wordWidth; x++)
{
ApplyMod(packet.ModulationData, x, y, is2BPP, out uint modValue, out uint mode);

var raw = InterpColors(colorA, colorB, modValue, mode, useAlpha);
int pxOffset = TextureHelper.GetPxOffset(col, row, x, y, width, wordWidth, BLOCK_HEIGHT);

output[pxOffset] = new(raw);
}

}

// Decode Pixels

private static void DecodePixels(ReadOnlySpan<PVRPacket> encoded, TextureColor* plain,
                                 int width, int height,
                                 bool is2BPP, bool useAlpha)
{
int wordWidth = is2BPP ? 8 : 4;

int blocksPerCol = width / wordWidth;
int blocksPerRow = height / BLOCK_HEIGHT;

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
{
int wordIndex = row * blocksPerCol + col;

DecodePx(encoded[wordIndex], plain, width, wordWidth, row, col, is2BPP, useAlpha);
}

}

// Sort PVR Packets from Morton to Linear

private static void SortPackets(Span<PVRPacket> words, int blocksPerCol, int blocksPerRow)
{
using NativeMemoryOwner<PVRPacket> lOwner = new(words.Length);
var linearWords = lOwner.AsSpan();

for(int row = 0; row < blocksPerRow; row++)
	
for(int col = 0; col < blocksPerCol; col++)
{
int mortonIdx = Morton.GetIndex(col, row);
int linearIdx = row * blocksPerCol + col;

linearWords[linearIdx] = words[mortonIdx];
}

linearWords.CopyTo(words);
}

// Decode PVR

public static SKBitmap Decode(Stream reader, int width, int height, bool is2BPP, bool useAlpha)
{
AdjustSize(ref width, ref height, is2BPP);

SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading raw data...");

int blockWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

int blocksPerCol = width / blockWidth;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;

int bufferSize = totalBlocks * 8;
using var rOwner = reader.ReadPtr(bufferSize);

var rawBytes = rOwner.AsSpan();
var packets = MemoryMarshal.Cast<byte, PVRPacket>(rawBytes);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Sorting blocks...");
SortPackets(packets, blocksPerCol, blocksPerRow);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");
DecodePixels(packets, pixels, width, height, is2BPP, useAlpha);

TraceLogger.WriteActionEnd();

return image;
}

}