using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Supports DirectX Texture Compression (DXT)

public static unsafe class DXT
{
// Tile Size (4x4)

private const int TILE_SIZE = 4;

#region ==========  ENCODER  ==========

// Encode block

public delegate void DXTEncoder(Span<TextureColor> block);

// Write alpha

public delegate void DXTAlphaEncoder(ReadOnlySpan<TextureColor> block, Span<ushort> alpha);

// Convert Color to 565 bits

private static ushort ColorTo565(in TextureColor color)
{
int packed = 0;

int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g6 = BitHelper.QuantizeFrom8(color.Green, 6);
int b5 = BitHelper.QuantizeFrom8(color.Blue, 5);

packed = BitHelper.Insert(packed, r5, 11, 5);
packed = BitHelper.Insert(packed, g6, 5, 6);
packed = BitHelper.Insert(packed, b5, 0, 5);

return (ushort)packed;
}

// Swap Colors

private static void SwapColors(ref TextureColor c1, ref TextureColor c2) => (c2, c1) = (c1, c2);

// Cuadratic Pow

private static int Pow(int x) => x * x;

// Use euclidean distance

private static int GetDistance(in TextureColor c1, in TextureColor c2)
{
var rDiff = Pow(c1.Red - c2.Red);
var gDiff = Pow(c1.Green - c2.Green);
var bDiff = Pow(c1.Blue - c2.Blue);

return rDiff + gDiff + bDiff;
}

// Euclidan distance for Color range

private static void EuclideanDistance(ReadOnlySpan<TextureColor> pixels, bool alphaEndpoints,
                                      out TextureColor min, out TextureColor max)
{
max = min = default;

int maxDistance = -1;

for(int i = 0; i < 15; i++)
{
var px = pixels[i];

if(alphaEndpoints && px.Alpha <= 128)
continue;

for(int j = i + 1; j < 16; j++)
{
var px2 = pixels[j];

if(alphaEndpoints && px2.Alpha <= 128)
continue;

int distance = GetDistance(px, px2);

if(distance > maxDistance)
{
maxDistance = distance;

min = px;
max = px2;
}

}

}

ushort min565 = ColorTo565(min);
ushort max565 = ColorTo565(max);

if(max565 < min565)
SwapColors(ref min, ref max);

}

// Combine Colors

private static int CombineColors(in TextureColor color, int r, int g, int b)
{
int rDiff = Math.Abs(r - color.Red);
int gDiff = Math.Abs(g - color.Green);
int bDiff = Math.Abs(b - color.Blue);

return rDiff + gDiff + bDiff;
}

// Simple interpolation

private static int Interp(int a, int b, int wa, int wb) => (a * wa + b * wb) / (wa + wb);

// Interpolate (Variant A)

private static int InterpA(int a, int b, bool alphaEndpoints)
{
return alphaEndpoints ? Interp(a, b, 1, 1) : Interp(a, b, 2, 1);
}

// Interpolate (Variant B)

private static int InterpB(int a, int b, bool alphaEndpoints) => alphaEndpoints ? 0 : Interp(a, b, 1, 2);

// Get Index Mask

private static int GetIdxMask(int a, int b) => a > b ? 1 : 0;

// Emit ColorIndices (only varies for DXT1-RGBA)

private static int EmitColorIndices(ReadOnlySpan<TextureColor> pixels,
                                    in TextureColor min,
									in TextureColor max,
									bool alphaEndpoints)
{
Span<int> colors = stackalloc int[16];
int indices = 0;

// Base colors

int r0 = BitHelper.ExpandChannel(max.Red, 5);
int g0 = BitHelper.ExpandChannel(max.Green, 6);
int b0 = BitHelper.ExpandChannel(max.Blue, 5);

int r1 = BitHelper.ExpandChannel(min.Red, 5);
int g1 = BitHelper.ExpandChannel(min.Green, 6);
int b1 = BitHelper.ExpandChannel(min.Blue, 5);

colors[0] = r0;
colors[1] = g0;
colors[2] = b0;
colors[4] = r1;
colors[5] = g1;
colors[6] = b1;

// Interpolations

int r2 = InterpA(r0, r1, alphaEndpoints);
int g2 = InterpA(g0, g1, alphaEndpoints);
int b2 = InterpA(b0, b1, alphaEndpoints);

int r3 = InterpB(r0, r1, alphaEndpoints);
int g3 = InterpB(g0, g1, alphaEndpoints);
int b3 = InterpB(b0, b1, alphaEndpoints);

colors[8] = r2;
colors[9] = g2;
colors[10] = b2;
colors[12] = r3;
colors[13] = g3;
colors[14] = b3;

for(int i = 15; i >= 0; i--)
{
var px = pixels[i];

if(alphaEndpoints && px.Alpha < 128)
{
indices |= (0b11) << (i << 1);

continue;
}

int d0 = CombineColors(px, r0, g0, b0);
int d1 = CombineColors(px, r1, g1, b1);
int d2 = CombineColors(px, r2, g2, b2);
int d3 = CombineColors(px, r3, g3, b3);

if(alphaEndpoints)
{
int mask = d0 > d2 && d1 > d2 ? 0b10 : 0b01;

indices |= (mask) << (i << 1);
}

else
{
int p0 = GetIdxMask(d0, d3);
int p1 = GetIdxMask(d1, d2);
int p2 = GetIdxMask(d0, d2);
int p3 = GetIdxMask(d1, d3);
int p4 = GetIdxMask(d2, d3);

int x0 = p1 & p2;
int x1 = p0 & p3;
int x2 = p0 & p4;

indices |= (x2 | ( (x0 | x1) << 1) ) << (i << 1);
}

}

return indices;
}

// Encode DXT

public static int Encode(Stream writer, SKBitmap image, bool alphaEndpoints,
                         DXTEncoder blockEncoder = null,
						 DXTAlphaEncoder alphaFunc = null)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();
 
int width  = image.Width;
int height = image.Height;

bool useAlpha = alphaFunc != null;

TraceLogger.WriteActionStart("Reading pixels...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int blockSize = useAlpha ? 8 : 4;

int bufferSize = totalBlocks * blockSize;

using NativeMemoryOwner<ushort> cOwner = new(bufferSize);
var colorInfo = cOwner.AsSpan();

Span<TextureColor> block = stackalloc TextureColor[16];
int colorOffset = useAlpha ? 4 : 0;

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = col * blocksPerRow + row;
var outBlock = colorInfo.Slice(blockIndex * blockSize, blockSize);

for(int i = 0; i < TILE_SIZE; i++)

for(int j = 0; j < TILE_SIZE; j++)
{
int srcX = row * TILE_SIZE + j;
int srcY = col * TILE_SIZE + i;

bool shouldCopy = srcX < width && srcY < height;

block[(i << 2) | j] = shouldCopy ? pixels[srcY * width + srcX] : default;
}

blockEncoder?.Invoke(block);

if(useAlpha)
alphaFunc.Invoke(block, outBlock[.. 4] );

EuclideanDistance(block, alphaEndpoints, out var min, out var max);

int indices = EmitColorIndices(block, min, max, alphaEndpoints);

ushort c0, c1;

if(alphaEndpoints)
{
c0 = ColorTo565(min);
c1 = ColorTo565(max);
}

else
{
c0 = ColorTo565(max);
c1 = ColorTo565(min);
}


var f0 = (ushort)(indices & 0xFFFF);
var f1 = (ushort)(indices >> 16);

outBlock[colorOffset] = c0;
outBlock[colorOffset + 1] = c1;
outBlock[colorOffset + 2] = f0;
outBlock[colorOffset + 3] = f1;
}

}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(colorInfo);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return blocksPerRow;
}

#endregion


#region ==========  DECODER  ==========

// Block decoder

public delegate void DXTDecoder(ReadOnlySpan<byte> alpha, Span<TextureColor> block);

// Alpha decoder (only used in DXT3-5)

public delegate void DXTAlphaDecoder(ReadOnlySpan<ushort> encoded, Span<byte> plain);

// Decode Color from 565-bits

private static TextureColor ColorFrom565(ushort flags)
{
int r5 = BitHelper.Extract(flags, 11, 5);
int g6 = BitHelper.Extract(flags, 5, 6);
int b5 = BitHelper.Extract(flags, 0, 5);

byte r = BitHelper.ExpandTo8(r5, 5);
byte g = BitHelper.ExpandTo8(g6, 6);
byte b = BitHelper.ExpandTo8(b5, 5);

return new(r, g, b);
}

// Compute Colors

private static void ComputeColors(ushort c0, ushort c1, Span<TextureColor> palette, bool hasAlpha)
{
bool shouldInterpolate = hasAlpha || c0 > c1;

var d0 = ColorFrom565(c0);
var d1 = ColorFrom565(c1);

TextureColor d2, d3;

if(shouldInterpolate)
{
d2 = TextureHelper.InterpolateColors(d0, d1, 2, 1, false);
d3 = TextureHelper.InterpolateColors(d0, d1, 1, 2, false);
}

else
{
d2 = TextureHelper.InterpolateColors(d0, d1, 1, 1, false);
d3 = default;
}

palette[0] = d0;
palette[1] = d1;
palette[2] = d2;
palette[3] = d3;
}

// Decode ColorIndices

private static void DecodeIndices(ReadOnlySpan<byte> indices, 
                                  ReadOnlySpan<TextureColor> palette,
                                  Span<TextureColor> block)
{

for(int i = 0; i < TILE_SIZE; i++)
{
byte row = indices[i];

for(int j = 0; j < TILE_SIZE; j++)
{
int blockIndex = (i << 2) | j;
int colorIndex = row & 0b11;

block[blockIndex] = palette[colorIndex];
row >>= 2;
}

}

}


// Decode DXT

public static SKBitmap Decode(Stream reader, int width, int height,
                              DXTDecoder blockDecoder = null,
                              DXTAlphaDecoder alphaFunc = null)
{
SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();
bool useAlpha = alphaFunc != null;

TraceLogger.WriteActionStart("Reading raw data...");

int blocksPerRow = TextureHelper.GetBlockDim(width, TILE_SIZE);
int blocksPerCol = TextureHelper.GetBlockDim(height, TILE_SIZE);

int totalBlocks = blocksPerRow * blocksPerCol;
int bytesPerBlock = useAlpha ? 16 : 8;

int bufferSize = totalBlocks * bytesPerBlock;

using var rOwner = reader.ReadPtr(bufferSize);
var rawBytes = rOwner.AsSpan();

TraceLogger.WriteActionEnd();

Span<TextureColor> block = stackalloc TextureColor[16];
Span<TextureColor> palette = stackalloc TextureColor[4];

Span<byte> colorIndices = stackalloc byte[4];
Span<byte> alphas = useAlpha ? stackalloc byte[16] : default;

TraceLogger.WriteActionStart("Writing pixels...");

int colorOffset = useAlpha ? 4 : 0;

for(int col = 0; col < blocksPerCol; col++)
{

for(int row = 0; row < blocksPerRow; row++)
{
int blockIndex = col * blocksPerRow + row;
var blockData = rawBytes.Slice(blockIndex * bytesPerBlock, bytesPerBlock);

var currBlock = MemoryMarshal.Cast<byte, ushort>(blockData);

if(useAlpha)
alphaFunc(currBlock[.. 4], alphas);

var colorInfo = currBlock.Slice(colorOffset, 4);

ushort c0 = colorInfo[0];
ushort c1 = colorInfo[1];
ushort f0 = colorInfo[2];
ushort f1 = colorInfo[3];

colorIndices[0] = (byte)(f0 & 0xFF);
colorIndices[1] = (byte)(f0 >> 8);
colorIndices[2] = (byte)(f1 & 0xFF);
colorIndices[3] = (byte)(f1 >> 8);

ComputeColors(c0, c1, palette, useAlpha);
DecodeIndices(colorIndices, palette, block);

blockDecoder?.Invoke(alphas, block);

for(int i = 0; i < TILE_SIZE; i++)
{
	
for(int j = 0; j < TILE_SIZE; j++)
{
int y = col * TILE_SIZE + i;
int x = row * TILE_SIZE + j;

if(x < width && y < height)
pixels[y * width + x] = block[(i << 2) | j];

}

}

}

}

TraceLogger.WriteActionEnd();

return image;
}

#endregion
}