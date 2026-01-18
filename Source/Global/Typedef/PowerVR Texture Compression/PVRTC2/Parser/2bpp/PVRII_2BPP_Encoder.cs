using static PVRBase;
using static PVR2Base;

using System;
using System.IO;
using System.Runtime.InteropServices;
using SkiaSharp;

// Encodes images with PVRTC2 (2bpp mode)

public static unsafe class PVRII_2BPP_Encoder
{
// Encode Packet

private static PVR2Packet2BPP EncodePacket(TextureColor* pixels, int width, int blockX, int blockY,
                                           bool useAlpha)
{

EncodePx(pixels, width, blockX, blockY, true, useAlpha,
         out var colorA, out var colorB, out uint bestModData,
         out bool bestM, out bool bestH);

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

// Encode Color

private static NativeMemoryOwner<PVR2Packet2BPP> EncodeColor(TextureColor* pixels, int width, int height,
                                                             bool useAlpha)
{
int blocksPerCol = width / BLOCK_WIDTH_2BPP;
int blocksPerRow = height / BLOCK_HEIGHT;

int totalBlocks = blocksPerCol * blocksPerRow;

NativeMemoryOwner<PVR2Packet2BPP> pOwner = new(totalBlocks);
var packets = pOwner.AsSpan();

InitPackets(packets, pixels, width, blocksPerCol, blocksPerRow, useAlpha);

return pOwner;
}
 
// Encode PVR2

public static int Encode(Stream writer, ref SKBitmap image, bool useAlpha)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => PVR2Base.AdjustSize(ref w, ref h, true) );

int width = image.Width;
int height = image.Height;
     
var pixels = (TextureColor*)image.GetPixels().ToPointer();

TraceLogger.WriteActionStart("Reading pixels...");

using var pOwner = EncodeColor(pixels, width, height, useAlpha);
var packets = pOwner.AsSpan();

TraceLogger.WriteActionEnd();

TraceLogger.WriteActionStart("Writing raw data...");

var rawBytes = MemoryMarshal.AsBytes(packets);
writer.Write(rawBytes);

TraceLogger.WriteActionEnd();

return width;
}

}