using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.XboxPackedTexture
{
/// <summary> Represents Info for a Xbox360 Packed Texture (PTX). </summary>

[StructLayout(LayoutKind.Explicit, Size = 12) ]

public struct PtxInfo 
{
/** <summary> Gets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(0)]
public int Width;

/** <summary> Gets or Sets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(4)]
public int Height;

/** <summary> Gets or Sets the Block Size with Padding. </summary>
<returns> The BlockSize. </returns> */

[FieldOffset(8)]
public int BlockSize;

/// <summary> Creates a new Instance of the <c>PtxInfo</c>. </summary>

public PtxInfo(int height, int width, int blockSize)
{
Height = height;
Width = width;

BlockSize = blockSize;
}

// Read PtxInfo

public static PtxInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[12];
reader.ReadExactly(rawData);

var info = MemoryMarshal.Read<PtxInfo>(rawData);
info.SwapEndian();

return info;
}

// Write Info to BinaryStream

public void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[12];

SwapEndian();
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

// Reverse Endianness

private void SwapEndian()
{
Width = BinaryPrimitives.ReverseEndianness(Width);
Height = BinaryPrimitives.ReverseEndianness(Height);

BlockSize = BinaryPrimitives.ReverseEndianness(BlockSize);
}

}

}