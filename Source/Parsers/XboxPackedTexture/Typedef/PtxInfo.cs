using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.XboxPackedTexture
{
/// <summary> Represents Info for a Xbox360 Packed Texture (PTX) </summary>

[StructLayout(LayoutKind.Explicit, Size = 12) ]

public struct PtxInfo 
{
/// <summary> Texture Width </summary>

[FieldOffset(0)]
public int Width;

/// <summary> Texture Height </summary>

[FieldOffset(4)]
public int Height;

/// <summary> Block Size in terms of uints (including Padding) </summary>

[FieldOffset(8)]
public int BlockSize;

// ctor

public PtxInfo(int width, int height, int blockSize)
{
Width = width;
Height = height;

BlockSize = blockSize;
}

// Read PtxInfo

public static PtxInfo ReadBin(ReadOnlySpan<byte> source)
{
var info = MemoryMarshal.Read<PtxInfo>(source);
info.SwapEndian();

return info;
}

// Write PtxInfo

public void WriteBin(Span<byte> destination)
{
SwapEndian();

MemoryMarshal.Write(destination, this);
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