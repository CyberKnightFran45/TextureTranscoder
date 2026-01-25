using static PVRBase;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encodes images with PVR

public static unsafe class PVREncoder
{
// Calculate BoundingBox

private static void GetBounds(TextureColor* pixels, int width, 
                              int row, int col, int blockWidth,
							  out TextureColor16 min, out TextureColor16 max)
{
byte minR = 255, minG = minR, minB = minG, minA = minG;
byte maxR = 0, maxG = maxR, maxB = maxG, maxA = maxB;

int start = (row * BLOCK_HEIGHT * width) + (col * blockWidth);

for(int y = 0; y < BLOCK_HEIGHT; y++)
{

for(int x = 0; x < blockWidth; x++)
{
int offset = start + y * width + x;
var px = pixels[offset];

byte r = px.Red;
byte g = px.Green;
byte b = px.Blue;
byte a = px.Alpha;

minR = Math.Min(minR, r);
minG = Math.Min(minG, g);
minB = Math.Min(minB, b);
minA = Math.Min(minA, a);

maxR = Math.Max(maxR, r);
maxG = Math.Max(maxG, g);
maxB = Math.Max(maxB, b);
maxA = Math.Max(maxA, a);
}

}

min = new(minR, minG, minB, minA);
max = new(maxR, maxG, maxB, maxA);
}

// Encode PVR Packet

private static PVRPacket EncodePacket(TextureColor* pixels, int width, int col, int row,
                                      int blockWidth, bool useAlpha)
{
GetBounds(pixels, width, row, col, blockWidth, out var min, out var max);

PVRPacket packet = new();

packet.SetColorA(min, useAlpha);
packet.SetColorB(max, useAlpha);

return packet;
}

// Init Packets

private static void InitPackets(Span<PVRPacket> packets, TextureColor* pixels, int width,
                                int blocksPerCol, int blocksPerRow, int blockWidth,
                                bool useAlpha)
{

for(int row = 0; row < blocksPerRow; row++)
{

for(int col = 0; col < blocksPerCol; col++)
{
var packet = EncodePacket(pixels, width, col, row, blockWidth, useAlpha);
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
int pA = useAlpha ? pxColor.Alpha << 4 : 0xFF0;

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

int offset = is2BPP ? PX_OFFSETS_2BPP[pY][pX] : PX_OFFSETS_4BPP[pY][pX];
uint mask = is2BPP ? MASK_2BPP : MASK_4BPP;

modulationData |= (modValue & mask) << offset;
}

// Calculate Block Modulation

private static uint GetBlockMod(Span<PVRPacket> packets, TextureColor* pixels, int width,
								int blocksPerCol, int blocksPerRow, int blockWidth,
								int bX, int bY, bool is2BPP, bool useAlpha)
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

// Encode Pixels

private static NativeMemoryOwner<PVRPacket> EncodePixels(TextureColor* pixels, int width, int height,
                                                         bool is2BPP, bool useAlpha)
{
int blockWidth = is2BPP ? BLOCK_WIDTH_2BPP : BLOCK_WIDTH_4BPP;

int blocksPerCol = width / blockWidth;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;

NativeMemoryOwner<PVRPacket> pOwner = new(totalBlocks);
var packets = pOwner.AsSpan();

InitPackets(packets, pixels, width, blocksPerCol, blocksPerRow, blockWidth, useAlpha);
SetModulations(packets, pixels, width, blocksPerCol, blocksPerRow, blockWidth, is2BPP, useAlpha);

return pOwner;
}

// Encode PVR (Internal)

public static int Encode(Stream writer, ref SKBitmap image, bool is2BPP, bool useAlpha)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => AdjustSize(ref w, ref h, is2BPP) );

int width = image.Width;
int height = image.Height;

var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading pixels...");

using var pOwner = EncodePixels(pixels, width, height, is2BPP, useAlpha);
var packets = pOwner.AsSpan();

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(packets);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return is2BPP ? width : width * 2;
}

}