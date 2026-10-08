using System;
using SkiaSharp;

// Handles Alpha Channel

public static unsafe class AlphaCodec
{
// Max palette size

private const byte MAX_PALETTE_SIZE = 16;

// Palette Indices for 4-bits mode

private static readonly byte[] PALETTE_INDICES_4BITS =
[
    MAX_PALETTE_SIZE,
    0,  1,   2,   3,   4,   5,   6,   7,  8,
    9,  10,  11,  12,  13,  14,  15
];

// Encode A8 into span

public static void Encode8(SKBitmap image, Span<byte> destination)
{
int square = image.GetSquare();

if(destination.Length < square)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var pixels = (TextureColor*)image.GetPixels().ToPointer();
var alphas = destination[.. square];

for(int i = 0; i < square; i++)
alphas[i] = pixels[i].Alpha;

}

// Encode Palette into span (4-bits)

public static void EncodePalette4(SKBitmap image, Span<byte> destination)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int bufferSize = (square + 1) >> 1;
int minSize = bufferSize + 17;

if(destination.Length < minSize)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var output = destination[.. minSize];
PALETTE_INDICES_4BITS.CopyTo(output[.. 17 ] );

var alphas = output[ 17 .. ] ;

for(int i = 0; i < bufferSize; i++)
{
int paletteIdx = i << 1;
int paletteIdx2 = paletteIdx + 1;

int a1 = pixels[paletteIdx].Alpha >> 4;
int a2 = paletteIdx2 < square ? (pixels[paletteIdx2].Alpha >> 4) : 0;

alphas[i] = (byte)((a1 << 4) | a2);
}

}

// Decode A8

public static void Decode8(ReadOnlySpan<byte> source, SKBitmap image)
{
int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

for(int i = 0; i < square; i++)
pixels[i].Alpha = source[i];

}

// Decode Palette (4-bits)

public static void DecodePalette4(ReadOnlySpan<byte> source, SKBitmap image)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();

byte rawLength = source[0];
byte minSize = Math.Min(rawLength, MAX_PALETTE_SIZE);

int paletteSize = minSize == 0 ? 2 : minSize;
Span<byte> aPalette = stackalloc byte[paletteSize];

int bitsPerIndex;
int dataOffset = 1;

if(paletteSize == 0)
{
bitsPerIndex = 1;

aPalette[0] = 0;
aPalette[1] = 255;
}

else
{
bitsPerIndex = paletteSize == 1 ? 1 : Math.ILogB(paletteSize - 1) + 1;

ReadOnlySpan<byte> rawBytes = source.Slice(dataOffset, paletteSize);

for(int i = 0; i < paletteSize; i++)
{
var rawEntry = rawBytes[i];

aPalette[i] = (byte)(rawEntry * 255 / 15);
}

dataOffset += paletteSize;
}

BitReader bitsReader = new(source[dataOffset..] );
int square = image.GetSquare();

for(int i = 0; i < square; i++)
{
int index = bitsReader.ReadBits(bitsPerIndex);

pixels[i].Alpha = aPalette[index];
}

}

}