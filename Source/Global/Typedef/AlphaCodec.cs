using System;
using System.IO;
using SkiaSharp;

// Handles Alpha Channel

public static unsafe class AlphaCodec
{
// Palette Indices for 4-bits mode

private static readonly byte[] PALETTE_INDICES_4BITS =
[
    16,
    0,  1,   2,   3,   4,   5,   6,  7,  8,
	9,  10,  11,  12,  13,  14,  15
];

// Encode A8

public static void Encode8(Stream writer, SKBitmap image)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading alpha channel...");

using NativeMemoryOwner<byte> aOwner = new(square);
var alphaInfo = aOwner.AsSpan();

for(int i = 0; i < square; i++)
alphaInfo[i] = pixels[i].Alpha;

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing A8...");
writer.Write(alphaInfo);

TraceLogger.WriteActionEnd();
}

// Encode Palette (4-bits)

public static int EncodePalette4(Stream writer, SKBitmap image)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading alpha channel...");

int bufferSize = (square + 1) >> 1;

using NativeMemoryOwner<byte> aOwner = new(bufferSize);
var alphaInfo = aOwner.AsSpan();

for(int i = 0; i < bufferSize; i++)
{
int paletteIdx = i << 1;
int paletteIdx2 = paletteIdx + 1;

int a1 = pixels[paletteIdx].Alpha >> 4;
int a2 = paletteIdx2 < square ? (pixels[paletteIdx2].Alpha >> 4) : 0;

alphaInfo[i] = (byte)( (a1 << 4) | a2);
}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing A-Palette...");

writer.Write(PALETTE_INDICES_4BITS);
writer.Write(alphaInfo);

TraceLogger.WriteActionEnd();

return bufferSize + 17;
}

// Decode A8

public static void Decode8(Stream reader, SKBitmap image)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading A8...");

using var aOwner = reader.ReadPtr(square);
var alphaInfo = aOwner.AsSpan();

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Fixing alpha channel...");

for(int i = 0; i < square; i++)
pixels[i].Alpha = alphaInfo[i];

TraceLogger.WriteActionEnd();
}

// Decode Palette (4-bits)

public static void DecodePalette4(Stream reader, SKBitmap image)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading A-Palette...");

byte maxSize = 16;
byte paletteSize = Math.Min(reader.ReadUInt8(), maxSize);

Span<byte> aPalette = stackalloc byte[paletteSize == 0 ? 2 : paletteSize];

int bitsPerIndex;

if(paletteSize == 0)
{
bitsPerIndex = 1;

aPalette[0] = 0;
aPalette[1] = 255;
}

else
{
bitsPerIndex = paletteSize == 1 ? 1 : Math.ILogB(paletteSize - 1) + 1;

Span<byte> rawBytes = stackalloc byte[paletteSize];
reader.ReadExactly(rawBytes);

for(int i = 0; i < paletteSize; i++)
{
var rawEntry = rawBytes[i];

aPalette[i] = (byte)(rawEntry * 255 / 15);
}

}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Fixing alpha channel...");

using BitStream bitsReader = new(reader);

for(int i = 0; i < square; i++)
{
int index = bitsReader.ReadBits(bitsPerIndex);
pixels[i].Alpha = aPalette[index];
}

TraceLogger.WriteActionEnd();
}

}