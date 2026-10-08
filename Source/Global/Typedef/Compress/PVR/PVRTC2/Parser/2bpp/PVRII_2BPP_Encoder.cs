using static PVRBase;
using static PVR2Base;

using System;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encode images with PVRTC2 (2bpp mode)

public static unsafe class PVRII_2BPP_Encoder
{
// Encode Packet

private static PVR2Packet2BPP EncodePacket(TextureColor* pixels,
                                           int width, 
                                           int blockX,
                                           int blockY,
                                           bool useAlpha)
{

EncodePx(pixels,
         width,
         blockX,
         blockY,
         true,
         useAlpha,
         out var colorA,
         out var colorB,
         out uint bestModData,
         out bool bestM,
         out bool bestH);

return new(colorA, colorB, bestModData, bestM, bestH, useAlpha);
}

// Init Packets

private static void InitPackets(Span<PVR2Packet2BPP> packets, TextureColor* pixels, int width,
                                int blocksPerCol, int blocksPerRow, bool useAlpha)
{
int index = 0;

for(int row = 0; row < blocksPerRow; row++)

for(int col = 0; col < blocksPerCol; col++)
packets[index++] = EncodePacket(pixels, width, col, row, useAlpha);
  
}

// Encode Color bits

private static void EncodeColor(Span<PVR2Packet2BPP> packets, TextureColor* pixels, int width, int height,
                                bool useAlpha)
{
int blocksPerCol = width / BLOCK_WIDTH_2BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

InitPackets(packets, pixels, width, blocksPerCol, blocksPerRow, useAlpha);
}

// Init args for encoding (returns buffer size)

private static int InitArgs(ref SKBitmap image,
                            out int width,
                            out int height,
                            out TextureColor* pixels)
{
return PVR2Base.InitArgs(ref image, true, out width, out height, out pixels);
}
 
// Encode PVR2 (to span)

internal static void Encode(ref SKBitmap image, Span<byte> destination, bool useAlpha)
{
int minSize = InitArgs(ref image, out var width, out var height, out var pixels);

if(destination.Length < minSize)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var packets = MemoryMarshal.Cast<byte, PVR2Packet2BPP>(destination[.. minSize] );

EncodeColor(packets, pixels, width, height, useAlpha);
}

// Encode PVR2

public static NativeBuffer Encode(ref SKBitmap image, bool useAlpha)
{
int bufferSize = InitArgs(ref image, out var width, out var height, out var pixels);

NativeBuffer output = new(bufferSize);

var packets = MemoryMarshal.Cast<byte, PVR2Packet2BPP>(output.AsSpan() );
EncodeColor(packets, pixels, width, height, useAlpha);

return output;
}

}