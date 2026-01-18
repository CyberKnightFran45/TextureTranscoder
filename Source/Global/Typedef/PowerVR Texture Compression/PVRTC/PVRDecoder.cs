using static PVRBase;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Decodes PVR Images

public static unsafe class PVRDecoder
{
// Decode color

private static TextureColor DecodeColor(uint flags, bool opaque,
                                        int rShift, int rBits,
                                        int gShift, int gBits,
                                        int bShift, int bBits,
                                        int aShift, int aBits)
{
var r = BitHelper.ExtractAndExpandTo8(flags, rShift, rBits);
var g = BitHelper.ExtractAndExpandTo8(flags, gShift, gBits);
var b = BitHelper.ExtractAndExpandTo8(flags, bShift, bBits);
var a = opaque ? (byte)255 : BitHelper.ExtractAndExpandTo8(flags, aShift, aBits);

return new(r, g, b, a);
}

// Decode ColorA

private static TextureColor DecodeColorA(uint flags)
{
bool opaque = (flags & 0x8000) != 0;

if(opaque)
return DecodeColor(flags, true, 10, 5, 5, 5, 0, 5, 0, 0);

return DecodeColor(flags, false, 8, 4, 4, 4, 0, 4, 12, 3);
}

// Decode ColorB

private static TextureColor DecodeColorB(uint flags)
{
bool opaque = (flags & 0x80000000) != 0;

if(opaque)
return DecodeColor(flags, true, 26, 5, 21, 5, 16, 5, 0, 0);

return DecodeColor(flags, false, 24, 4, 20, 4, 16, 4, 28, 3);
}

// Extract Color as is (no interpolation)

private static TextureColor ExtractColor(in TextureColor colorA, in TextureColor colorB,
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

private static TextureColor GetInterpolated(in TextureColor colorA, in TextureColor colorB,
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

private static TextureColor InterpColors(in TextureColor colorA, in TextureColor colorB,
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

private static void DecodePx(in PVRWord word, TextureColor* output, int width,
                             int wordWidth, int row, int col,
							 bool is2BPP, bool useAlpha)
{
var flags = word.Flags;

var colorA = DecodeColorA(flags);
var colorB = DecodeColorB(flags);

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < wordWidth; x++)
{
ApplyMod(word.ModulationData, x, y, is2BPP, out uint modValue, out uint mode);

var raw = InterpColors(colorA, colorB, modValue, mode, useAlpha);
int pxOffset = TextureHelper.GetPxOffset(col, row, x, y, width, wordWidth, BLOCK_HEIGHT);

output[pxOffset] = raw;
}

}

// Decode Pixels

private static void DecodePixels(ReadOnlySpan<PVRWord> encoded, TextureColor* plain,
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

// Sort PVR Words from Morton to Linear

private static void SortWords(Span<PVRWord> words, int blocksPerCol, int blocksPerRow)
{
using NativeMemoryOwner<PVRWord> lOwner = new(words.Length);
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
var words = MemoryMarshal.Cast<byte, PVRWord>(rawBytes);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Sorting blocks...");
SortWords(words, blocksPerCol, blocksPerRow);

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");
DecodePixels(words, pixels, width, height, is2BPP, useAlpha);

TraceLogger.WriteActionEnd();

return image;
}

}