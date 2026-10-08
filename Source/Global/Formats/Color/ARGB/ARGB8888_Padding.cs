using System;
using System.Runtime.InteropServices;
using SkiaSharp;

public static unsafe class ARGB8888_Padding
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height, int blockSize)
{
SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();
int pixelsPerRow = width * 4;

for(int i = 0; i < height; i++)
{
int offset = i * blockSize;
var row = MemoryMarshal.Cast<byte, uint>(source.Slice(offset, pixelsPerRow) );

for(int j = 0; j < width; j++)
pixels[i * width + j] = ARGB8888.DecodeColor(row[j] );

}

return image;
}

// Encode image

public static NativeBuffer Encode(SKBitmap image, int blockSize)
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

int pixelsPerRow = width * 4;

NativeBuffer output = new (blockSize * height);
Span<byte> rawBytes = output.AsSpan();

for(int i = 0; i < height; i++)
{
var row = MemoryMarshal.Cast<byte, uint>(rawBytes.Slice(i * blockSize, pixelsPerRow) );

for(int j = 0; j < width; j++)
row[j] = ARGB8888.EncodeColor(pixels[i * width + j] );

}

return output;
}

}