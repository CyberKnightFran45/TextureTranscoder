using static PVRBase;

using System;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encodes images with PVR

public static unsafe class PVREncoder
{
// Calculate bounds

private static void GetBounds(TextureColor* pixels,
                              int width,
                              int row,
                              int col,
                              int blockWidth,
                              out TextureColor16 min,
							  out TextureColor16 max)
{
long sumLowR = 0, sumLowG = 0, sumLowB = 0,  sumLowA = 0,  countLow = 0;
long sumHighR = 0, sumHighG = 0, sumHighB = 0, sumHighA = 0, countHigh = 0;

int start = (row * BLOCK_HEIGHT * width) + (col * blockWidth);

int totalPx = blockWidth * BLOCK_HEIGHT;
long totalLum = 0;

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < blockWidth; x++)
{
var px = pixels[start + y * width + x];

totalLum += px.Red * 77 + px.Green * 150 + px.Blue * 29;
}

long avgLum = totalLum / totalPx;

for(int y = 0; y < BLOCK_HEIGHT; y++)

for(int x = 0; x < blockWidth; x++)
{
var px = pixels[start + y * width + x];
long lum = px.Red * 77 + px.Green * 150 + px.Blue * 29;

if(lum <= avgLum)
{
sumLowR += px.Red;
sumLowG += px.Green;
sumLowB += px.Blue;
sumLowA += px.Alpha;

countLow++;
}

else
{
sumHighR += px.Red;
sumHighG += px.Green;
sumHighB += px.Blue;
sumHighA += px.Alpha;

countHigh++;
}

}

if(countLow == 0)
{
sumLowR = sumHighR / countHigh;
sumLowG = sumHighG / countHigh; 
sumLowB = sumHighB / countHigh;
sumLowA = sumHighA / countHigh;

countLow = 1;
}

if(countHigh == 0)
{
sumHighR = sumLowR / countLow;
sumHighG = sumLowG / countLow;
sumHighB = sumLowB / countLow;
sumHighA = sumLowA / countLow;

countHigh = 1;
}

var minR = (byte)(sumLowR / countLow);
var minG = (byte)(sumLowG / countLow);
var minB = (byte)(sumLowB / countLow);
var minA = (byte)(sumLowA / countLow);

min = new(minR, minG, minB, minA);

var maxR = (byte)(sumHighR / countHigh);
var maxG = (byte)(sumHighG / countHigh);
var maxB = (byte)(sumHighB / countHigh);
var maxA = (byte)(sumHighA / countHigh);

max = new(maxR, maxG, maxB, maxA);
}

// Init Packets

private static void InitPackets(Span<PVRPacket> packets,
                                Span<PVRColors> rawColors,
                                TextureColor* pixels,
                                int width,
                                int blocksPerCol,
                                int blocksPerRow,
                                int blockWidth,
                                bool useAlpha)
{

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
{
GetBounds(pixels, width, row, col, blockWidth, out var minC, out var maxC);

int mortonIdx = Morton.GetIndex(col, row);

rawColors[mortonIdx] = new(minC, maxC);
packets[mortonIdx] = new(minC, maxC, useAlpha);
}

}

// Get Factor (Generic)

private static TextureColor16 GetFactor(in TextureColor16 c0,
                                        in TextureColor16 c1,
                                        in TextureColor16 c2,
                                        in TextureColor16 c3,
										ReadOnlySpan<byte> factors)
{
return c0 * factors[0] + c1 * factors[1] + c2 * factors[2] + c3 * factors[3];
}

// Get Pixel Modulation

private static void GetPxMod(Span<PVRPacket> packets, 
                             Span<PVRColors> rawColors,
                             TextureColor* pixels,
                             int width,
                             int x0,
                             int x1,
                             int y0,
                             int y1,
							 int dataOffset,
                             int pX,
                             int pY,
							 int factorIndex,
                             bool is2BPP,
                             bool useAlpha,
							 ref uint modulationData)
{
var factors = is2BPP ? BILINEAR_FACTORS_2BPP[factorIndex] : BILINEAR_FACTORS_4BPP[factorIndex];

int idx0 = Morton.GetIndex(x0, y0);
int idx1 = Morton.GetIndex(x1, y0);
int idx2 = Morton.GetIndex(x0, y1);
int idx3 = Morton.GetIndex(x1, y1);

var c0 = rawColors[idx0];
var c1 = rawColors[idx1];
var c2 = rawColors[idx2];
var c3 = rawColors[idx3];

var cA = GetFactor(c0.RawA, c1.RawA, c2.RawA, c3.RawA, factors);
var cB = GetFactor(c0.RawB, c1.RawB, c2.RawB, c3.RawB, factors);

var pxColor = pixels[dataOffset + pY * width + pX];

int pR = pxColor.Red << 4;
int pG = pxColor.Green << 4;
int pB = pxColor.Blue << 4;
int pA = useAlpha ? pxColor.Alpha << 4 : 0xFF0;

TextureColor16 p = new(pR, pG, pB, pA);

var d = cB - cA;
var v = p - cA;

int projection = (v % d) << 4;
int lengthSquared = d % d;

uint modValue = 0;

if(projection > 3 * lengthSquared)
modValue++;

if(projection > 8 * lengthSquared)
modValue++;

if(projection > 13 * lengthSquared)
modValue++;

int offset = is2BPP ? PX_OFFSETS_2BPP[pY][pX] : PX_OFFSETS_4BPP[pY][pX];
uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;

modulationData |= (modValue & mask) << offset;
}

// Calculate Block Modulation

private static uint GetBlockMod(Span<PVRPacket> packets,
                                Span<PVRColors> rawColors,
                                TextureColor* pixels,
                                int width,
								int blocksPerCol,
                                int blocksPerRow,
                                int blockWidth,
								int bX,
                                int bY,
                                bool is2BPP,
                                bool useAlpha)
{
uint modulationData = 0;
int factorIndex = 0;

int dataOffset = bY * BLOCK_HEIGHT * width + (bX * blockWidth);

for(int pY = 0; pY < BLOCK_HEIGHT; pY++)
{
int y0 = (bY + ( (pY < 2) ? -1 : 0) + blocksPerRow) % blocksPerRow;
int y1 = (y0 + 1) % blocksPerRow;

for(int pX = 0; pX < blockWidth; pX++)
{
int x0 = (bX + ( (pX < (blockWidth / 2) ) ? -1 : 0) + blocksPerCol) % blocksPerCol;
int x1 = (x0 + 1) % blocksPerCol;

GetPxMod(packets, rawColors, pixels, width,
         x0, x1, y0, y1, dataOffset,
         pX, pY, factorIndex, is2BPP, useAlpha, ref modulationData);

factorIndex++;
}

}

return modulationData;
}

// Set Modulations

private static void SetModulations(Span<PVRPacket> packets,
                                   Span<PVRColors> rawColors,
                                   TextureColor* pixels,
                                   int width,
                                   int blocksPerCol,
                                   int blocksPerRow,
                                   int blockWidth,
                                   bool is2BPP,
                                   bool useAlpha)
{

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
{
int mortonIdx = Morton.GetIndex(col, row);
ref var packet = ref packets[mortonIdx];

uint modValue = GetBlockMod(packets, rawColors, pixels, width,
                            blocksPerCol, blocksPerRow,
                            blockWidth, col, row, is2BPP, useAlpha);

packet.ModulationData = modValue;
}

}

// Encode Pixels

private static void EncodePixels(Span<PVRPacket> packets,
                                 TextureColor* pixels,
                                 int width,
                                 int height,
                                 bool is2BPP,
                                 bool useAlpha)
{
int blockWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

int blocksPerCol = width  / blockWidth;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks  = blocksPerCol * blocksPerRow;

using NativeMemoryOwner<PVRColors> cOwner = new(totalBlocks);

var rawColors = cOwner.AsSpan();

InitPackets(packets, rawColors, pixels, width, blocksPerCol, blocksPerRow,
            blockWidth, useAlpha);

SetModulations(packets, rawColors, pixels, width, blocksPerCol, blocksPerRow,
               blockWidth, is2BPP, useAlpha);

}

// Init args for encoding (returns buffer size)

private static int InitArgs(ref SKBitmap image,
                            bool is2BPP,
                            out int width,
                            out int height,
                            out TextureColor* pixels)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => AdjustSize(ref w, ref h, is2BPP) );

width = image.Width;
height = image.Height;

pixels = (TextureColor*)image.GetPixels().ToPointer();

return ComputePVRSize(width, height, is2BPP);
}

// Encode PVR (Internal)

internal static void Encode(ref SKBitmap image, Span<byte> destination, bool is2BPP, bool useAlpha)
{
int minSize = InitArgs(ref image, is2BPP, out var width, out var height, out var pixels);

if(destination.Length < minSize)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var packets = MemoryMarshal.Cast<byte, PVRPacket>(destination[ .. minSize] );

EncodePixels(packets, pixels, width, height, is2BPP, useAlpha);
}

// Encode PVR

public static NativeBuffer Encode(ref SKBitmap image, bool is2BPP, bool useAlpha)
{
int bufferSize = InitArgs(ref image, is2BPP, out var width, out var height, out var pixels);

NativeBuffer output = new(bufferSize);

var packets = MemoryMarshal.Cast<byte, PVRPacket>(output.AsSpan() );
EncodePixels(packets, pixels, width, height, is2BPP, useAlpha);

return output;
}

}