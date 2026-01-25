using System.IO;
using SkiaSharp;

// Parse DXT5 Images in RGBA Order (with Padding)

public static class DXT5_RGBA_Padding
{
// Adjust Width to Multiple of 4

private static int AdjustWidth(int width) => (width + 3) / 4 * 4;

// Remove padding

private static void RemovePadding(ref Stream source, int width, int height, int blockSize)
{
using var inOwner = source.ReadPtr();
var input = inOwner.AsSpan();

int newWidth = AdjustWidth(width);

int rowSize = newWidth / 4;
int paddingPerRow = blockSize - rowSize;

int outputLen = rowSize * height;
ChunkedMemoryStream resizedStream = new(outputLen);

int srcOffset = 0;

for(int row = 0; row < height; row++)
{
resizedStream.Write(input.Slice(srcOffset, rowSize) );

srcOffset += rowSize + paddingPerRow;
}

source.Dispose();
resizedStream.Seek(0, SeekOrigin.Begin);

source = resizedStream;
}

// Read bitmap

public static SKBitmap Read(Stream reader, int width, int height, int blockSize)
{
TraceLogger.WriteLine("• DXT5-RGBA-Padding Texture Decode:");
TraceLogger.WriteLine();

TraceLogger.WriteActionStart("Removing padding...");
RemovePadding(ref reader, width, height, blockSize);

TraceLogger.WriteActionEnd();

return DXTDecoder.Decode(reader, width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);
}

// Add Padding

private static void AddPadding(Stream source, Stream target, int width, int height, int blockSize)
{
using var inOwner = source.ReadPtr();
var input = inOwner.AsSpan();

int totalSize = blockSize * height;
using NativeMemoryOwner<byte> outOwner = new(totalSize);

var output = outOwner.AsSpan();
output.Fill(0xCD);

int newWidth = AdjustWidth(width);
int rowSize = newWidth * 4;

int paddingPerRow = blockSize - rowSize;

for(int row = 0; row < height; row++)
{
int srcOffset = row * rowSize; 
int dstOffset = row * blockSize;

input.Slice(srcOffset, rowSize).CopyTo(output.Slice(dstOffset, rowSize) );
}

target.Write(output);
}

// Write pixels

public static int Write(Stream writer, SKBitmap image, int blockSize)
{
TraceLogger.WriteLine("• DXT5-RGBA-Padding Texture Encode:");
TraceLogger.WriteLine();

using ChunkedMemoryStream encodedStream = new();
_ = DXTEncoder.Encode(encodedStream, image, false, null, DXT4_RGBA.EncodeAlpha);

TraceLogger.WriteActionStart("Padding image...");
AddPadding(encodedStream, writer, image.Width, image.Height, blockSize);

TraceLogger.WriteActionEnd();

return blockSize / 4;
}

}