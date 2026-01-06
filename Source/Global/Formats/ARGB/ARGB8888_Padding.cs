using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Parse Image in the ARGB format (8 bits per Color, with Padding)

public static unsafe class ARGB8888_Padding
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height, int blockSize)
{
TraceLogger.WriteLine("• Padded-ARGB-8888 Texture Decode:");
TraceLogger.WriteLine();

SKBitmap image = new(width, height);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading raw data...");

int bufferSize = height * blockSize;

using var rOwner = reader.ReadPtr(bufferSize);
var rawBytes = rOwner.AsSpan();

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing pixels...");

int pixelsPerRow = width * 4;

for(int i = 0; i < height; i++)
{
int offset = i * blockSize;
var row = MemoryMarshal.Cast<byte, uint>(rawBytes.Slice(offset, pixelsPerRow) );

for(int j = 0; j < width; j++)
pixels[i * width + j] = ARGB8888.DecodeColor(row[j]);

}

TraceLogger.WriteActionEnd();
    
return image;
}

// Write pixels

public static int Write(Stream writer, SKBitmap image, int blockSize)
{
TraceLogger.WriteLine("• Padded-ARGB-8888 Texture Encode:");
TraceLogger.WriteLine();

var pixels = (TextureColor*)image.GetPixels().ToPointer();

int width = image.Width;
int height = image.Height;

int pixelsPerRow = width * 4;

TraceLogger.WriteActionStart("Reading pixels...");

int bufferSize = blockSize * height;
using NativeMemoryOwner<byte> bOwner = new(bufferSize);

var rawBytes = bOwner.AsSpan();
rawBytes.Clear();

for(int i = 0; i < height; i++)
{
var chunks = rawBytes.Slice(i * blockSize, pixelsPerRow);
var row = MemoryMarshal.Cast<byte, uint>(chunks);

for(int j = 0; j < width; j++)
row[j] = ARGB8888.EncodeColor(pixels[i * width + j]);

}

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return blockSize;
}

}