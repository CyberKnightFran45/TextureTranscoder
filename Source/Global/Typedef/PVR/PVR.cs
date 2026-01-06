using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Supports PowerVR Texture Compression (PVRTC)

public static unsafe class PVR
{
// Precomputed tables

private static readonly byte[] RepVals2BPP = new byte[16];
private static readonly byte[] RepVals4BPP = new byte[16];

private static readonly int[,] PxOffset2BPP = new int[8, 4];
private static readonly int[,] PxOffset4BPP = new int[4, 4];

// Bilinear factors

private static readonly byte[][] BILINEAR_FACTORS = 
[
    [ 4, 4, 4, 4 ],
	[ 2, 6, 2, 6 ],
    [ 8, 0, 8, 0 ],
    [ 6, 2, 6, 2 ],
	[ 2, 2, 6, 6 ],
	[ 1, 3, 3, 9 ],
    [ 4, 0, 12, 0 ],
	[ 3, 1, 9, 3 ],
	[ 8, 8, 0, 0 ],
	[ 4, 12, 0, 0 ],
	[ 16, 0, 0, 0 ],
	[ 12, 4, 0, 0 ],
	[ 6, 6, 2, 2 ],
	[ 3, 9, 1, 3 ],
	[ 12, 0, 4, 0 ],
	[ 9, 3, 3, 1 ], 

];

// Word Height

private const int WORD_HEIGHT = 4;

// Init Tables

static PVR()
{
// RepVals

for(byte i = 0; i < 16; i++)
{
RepVals2BPP[i] = (byte)( (i & 1) | ( (i & 2) << 1) | ( (i & 4) >> 1) | ( (i & 8) >> 2) );
RepVals4BPP[i] = i;
}

// Pixel Offsets (2BPP)

for(int y = 0; y < 4; y++)
{

for(int x = 0; x < 8; x++)
PxOffset2BPP[x, y] = 2 * (x & 3) + 8 * (y & 3);

}

// Pixel Offsets (4BPP)

for(int y = 0; y < 4; y++)
{

for(int x = 0; x < 4; x++)
PxOffset4BPP[x, y] = 4 * x + 16 * y;
 
}

}

// Adjust Image Dimensions

private static bool AdjustSize(ref int width, ref int height, bool is2BPP)
{
int minWidth = is2BPP ? 16 : 8;
int minHeight = 8;

width = Math.Max(width, minWidth);
height = Math.Max(height, minHeight);

return TextureHelper.AdjustSize(ref width, ref height);
}

#region ==========  ENCODER  ==========

// Calculate BoundingBox

private static void GetBounds(TextureColor* pixels, int width, int row, int col,
							  int blockWidth, out TextureColor16 min, out TextureColor16 max)
{
byte minR = 255, minG = minR, minB = minG, minA = minG;
byte maxR = 0, maxG = maxR, maxB = maxG, maxA = maxB;

int start = col * WORD_HEIGHT * width + (row * blockWidth);

for(int y = 0; y < WORD_HEIGHT; y++)
{

for(int x = 0; x < blockWidth; x++)
{
var px = pixels[start + y * width + x];

minR = Math.Min(minR, px.Red);
minG = Math.Min(minG, px.Green);
minB = Math.Min(minB, px.Blue);
minA = Math.Min(minA, px.Alpha);

maxR = Math.Max(maxR, px.Red);
maxG = Math.Max(maxG, px.Green);
maxB = Math.Max(maxB, px.Blue);
maxA = Math.Max(maxA, px.Alpha);
}

}

min = new(minR, minG, minB, minA);
max = new(maxR, maxG, maxB, maxA);
}



// Init Packets

private static void InitPackets(Span<PVRPacket> packets, TextureColor* pixels, int width, int blocksPerCol,
                                int blocksPerRow, int blockWidth, bool useAlpha)
{

for(int row = 0; row < blocksPerRow; row++)
{

for(int col = 0; col < blocksPerCol; col++)
{
GetBounds(pixels, width, col, row, blockWidth, out var min, out var max);

PVRPacket packet = new();

packet.SetColorA(min, useAlpha);
packet.SetColorB(max, useAlpha);

int mortonIdx = Morton.GetIndex(col, row);

packets[mortonIdx] = packet;
}

}

}

// Get Factor (Generic)

private static TextureColor16 GetFactor(in TextureColor16 c0, in TextureColor16 c1,
                                        in TextureColor16 c2, in TextureColor16 c3,
										ReadOnlySpan<byte> factors)
{
return c0 * factors[0] + c1 * factors[1] + c2 * factors[2] + c3 * factors[3];
}

// Get Factor A

private static TextureColor16 GetFactorA(in PVRPacket p0, in PVRPacket p1,
                                         in PVRPacket p2, in PVRPacket p3,
                                         ReadOnlySpan<byte> factors, bool useAlpha)
{
var c0 = p0.GetColorA(useAlpha);
var c1 = p1.GetColorA(useAlpha);
var c2 = p2.GetColorA(useAlpha);
var c3 = p3.GetColorA(useAlpha);

return GetFactor(c0, c1, c2, c3, factors);
}

// Get Factor B

private static TextureColor16 GetFactorB(in PVRPacket p0, in PVRPacket p1,
                                         in PVRPacket p2, in PVRPacket p3,
                                         ReadOnlySpan<byte> factors, bool useAlpha)
{
var c0 = p0.GetColorB(useAlpha);
var c1 = p1.GetColorB(useAlpha);
var c2 = p2.GetColorB(useAlpha);
var c3 = p3.GetColorB(useAlpha);

return GetFactor(c0, c1, c2, c3, factors);
}

// Get Pixel Modulation

private static void GetPxMod(Span<PVRPacket> packets, TextureColor* pixels, int width,
                             int x0, int x1, int y0, int y1,
							 int dataOffset, int pX, int pY,
							 int factorIndex, bool is2BPP, bool useAlpha,
							 ref uint modulationData)
{
var factors = BILINEAR_FACTORS[factorIndex];

int mortonIdx0 = Morton.GetIndex(x0, y0);
int mortonIdx1 = Morton.GetIndex(x1, y0);
int mortonIdx2 = Morton.GetIndex(x0, y1);
int mortonIdx3 = Morton.GetIndex(x1, y1);

var p0 = packets[mortonIdx0];
var p1 = packets[mortonIdx1];
var p2 = packets[mortonIdx2];
var p3 = packets[mortonIdx3];

var ca = GetFactorA(p0, p1, p2, p3, factors, useAlpha);
var cb = GetFactorB(p0, p1, p2, p3, factors, useAlpha);

var pxColor = pixels[dataOffset + pY * width + pX];

int pR = pxColor.Red << 4;
int pG = pxColor.Green << 4;
int pB = pxColor.Blue << 4;
int pA = useAlpha ? pxColor.Alpha << 4 : 255;

TextureColor16 p = new(pR, pG, pB, pA);

var d = cb - ca;
var v = p - ca;

int projection = (v % d) << 4;
int lengthSquared = d % d;

uint modValue = 0;

if(projection > 3 * lengthSquared)
modValue++;

if(projection > 8 * lengthSquared)
modValue++;

if(projection > 13 * lengthSquared)
modValue++;

int offset = is2BPP ? PxOffset2BPP[pX, pY] : PxOffset4BPP[pX, pY];
uint mask = is2BPP ? 0b11u : 0xFu;

modulationData |= (modValue & (mask) ) << offset;
}

// Calculate Block Modulation

private static uint GetBlockMod(Span<PVRPacket> packets, TextureColor* pixels, int width,
								int blocksPerCol, int blocksPerRow, int blockWidth,
								int bX, int bY, bool is2BPP, bool useAlpha)
{
uint modulationData = 0;
int factorIndex = 0;

int dataOffset = bY * WORD_HEIGHT * width + (bX * blockWidth);

for(int pY = 0; pY < WORD_HEIGHT; pY++)
{
int y0 = (bY + ( (pY < 2) ? -1 : 0) + blocksPerRow) % blocksPerRow;
int y1 = (y0 + 1) % blocksPerRow;

for(int pX = 0; pX < blockWidth; pX++)
{
int x0 = (bX + ( (pX < (blockWidth / 2) ) ? -1 : 0) + blocksPerCol) % blocksPerCol;
int x1 = (x0 + 1) % blocksPerCol;

GetPxMod(packets, pixels, width, x0, x1, y0, y1, dataOffset,
         pX, pY, factorIndex, is2BPP, useAlpha, ref modulationData);

factorIndex++;
}

}

return modulationData;
}

// Set Modulations

private static void SetModulations(Span<PVRPacket> packets, TextureColor* pixels, int width,
                                   int blocksPerCol, int blocksPerRow, int blockWidth,
								   bool is2BPP, bool useAlpha)
{

for(int row = 0; row < blocksPerRow; row++)
{

for(int col = 0; col < blocksPerCol; col++)
{
int mortonIdx = Morton.GetIndex(col, row);
ref var packet = ref packets[mortonIdx];

uint modValue = GetBlockMod(packets, pixels, width, blocksPerCol, blocksPerRow,
                            blockWidth, col, row, is2BPP, useAlpha);

packet.ModulationData = modValue;
}

}

}

// Encode Color

private static NativeMemoryOwner<PVRPacket> EncodeColor(TextureColor* pixels, int width, int height,
                                                        bool is2BPP, bool useAlpha)
{
int blockWidth = is2BPP ? 8 : 4;

int blocksPerCol = width / blockWidth;
int blocksPerRow = height / WORD_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;

NativeMemoryOwner<PVRPacket> pOwner = new(totalBlocks);
var packets = pOwner.AsSpan();

InitPackets(packets, pixels, width, blocksPerCol, blocksPerRow, blockWidth, useAlpha);
SetModulations(packets, pixels, width, blocksPerCol, blocksPerRow, blockWidth, is2BPP, useAlpha);

return pOwner;
}

// Encode PVR (Internal)

private static int Encode(Stream writer, ref SKBitmap image, bool is2BPP, bool useAlpha)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => AdjustSize(ref w, ref h, is2BPP) );

int width = image.Width;
int height = image.Height;

var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading pixels...");

using var pOwner = EncodeColor(pixels, width, height, is2BPP, useAlpha);
var packets = pOwner.AsSpan();

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(packets);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return is2BPP ? width : width * 2;
}

// Encode PVR (2BPP)

public static int Encode2bpp(Stream writer, ref SKBitmap image)
{
return Encode(writer, ref image, true, true);
}

// Encode PVR (4BPP)

public static int Encode4bpp(Stream writer, ref SKBitmap image, bool useAlpha)
{
return Encode(writer, ref image, false, useAlpha);
}

#endregion


#region ==========  DECODER  ==========

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

// Get ColorA

private static TextureColor GetColorA(uint flags)
{
bool opaque = (flags & 0x8000) != 0;

if(opaque)
return DecodeColor(flags, true, 10, 5, 5, 5, 0, 5, 0, 0);

return DecodeColor(flags, false, 8, 4, 4, 4, 0, 4, 12, 3);
}

// Get ColorB

private static TextureColor GetColorB(uint flags)
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
1 => TextureHelper.InterpolateColors(colorA, colorB, 5, 3, useAlpha),
2 => TextureHelper.InterpolateColors(colorA, colorB, 3, 5, useAlpha),
3 => colorB,
_ => colorA,
};

}

// Get Color Interpolated

private static TextureColor GetInterpolated(in TextureColor colorA, in TextureColor colorB, uint modValue,
                                            bool useAlpha)
{

return modValue switch
{
1 => TextureHelper.InterpolateColors(colorA, colorB, 3, 1, useAlpha),
2 => TextureHelper.InterpolateColors(colorA, colorB, 2, 2, useAlpha),
3 => TextureHelper.InterpolateColors(colorA, colorB, 1, 3, useAlpha),
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

int offset = is2BPP ? PxOffset2BPP[x, y] : PxOffset4BPP[x, y];
uint mask = is2BPP ? 0x3u : 0xFu;

modValue = (data >> offset) & mask;
}

// Decode PVR Color

private static void DecodeColor(ReadOnlySpan<PVRWord> encoded, TextureColor* plain, int width, int height,
                                bool is2BPP, bool useAlpha)
{
int wordWidth = is2BPP ? 8 : 4;

int blocksPerCol = width / wordWidth;
int blocksPerRow = height / WORD_HEIGHT;

for(int row = 0; row < blocksPerRow; row++)
{

for(int col = 0; col < blocksPerCol; col++)
{
int wordIndex = row * blocksPerCol + col;

var word = encoded[wordIndex];
var flags = word.Flags;

var colorA = GetColorA(flags);
var colorB = GetColorB(flags);

for(int y = 0; y < WORD_HEIGHT; y++)
{

for(int x = 0; x < wordWidth; x++)
{
ApplyMod(word.ModulationData, x, y, is2BPP, out uint modValue, out uint mode);

var finalColor = InterpColors(colorA, colorB, modValue, mode, useAlpha);
int pxOffset = (row * WORD_HEIGHT + y) * width + col * wordWidth + x;  

plain[pxOffset] = finalColor;
}

}

}

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

// Decode PVR (Internal)

private static SKBitmap Decode(Stream reader, int width, int height, bool is2BPP, bool useAlpha)
{
AdjustSize(ref width, ref height, is2BPP);

SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading raw data...");

int blockWidth = is2BPP ? 8 : 4;

int blocksPerCol = width / blockWidth;
int blocksPerRow = height / WORD_HEIGHT;

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
DecodeColor(words, pixels, width, height, is2BPP, useAlpha);

TraceLogger.WriteActionEnd();

return image;
}

// Decode PVR (2BPP)

public static SKBitmap Decode2bpp(Stream reader, int width, int height)
{
return Decode(reader, width, height, true, true);
}

// Decode PVR (4BPP)

public static SKBitmap Decode4bpp(Stream reader, int width, int height, bool useAlpha)
{
return Decode(reader, width, height, false, useAlpha);
}

#endregion
}