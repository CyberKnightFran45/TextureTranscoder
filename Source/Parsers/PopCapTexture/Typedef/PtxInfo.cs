using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.PopCapTexture
{
/// <summary> Stores info related to a Encoded PTX Image (used by the tool ONLY) </summary>

[StructLayout(LayoutKind.Explicit, Size = 28) ]

public struct PtxInfo
{
/// <summary> File identifier </summary>

[FieldOffset(0)]
public readonly uint Magic = 0x70747831;

/// <summary> Texture Width </summary>

[FieldOffset(4)]
public uint Width;

/// <summary> Texture Height </summary>

[FieldOffset(8)]
public uint Height;

/// <summary> Texture Pitch </summary>

[FieldOffset(12)]
public uint Pitch;

/// <summary> Texture Format </summary>

[FieldOffset(16)]
public PtxFormat Format;

/// <summary> Amount of bytes written in AlphaChannel (0 if no Alpha) </summary>

[FieldOffset(20)]
public uint AlphaSize;

/// <summary> Alpha Type. </summary>

[FieldOffset(24)]
public PtxAlphaChannel AlphaChannel;

// ctor

public PtxInfo(uint width, 
               uint height,
			   uint pitch,
			   PtxFormat format,
               uint alphaSize,
			   PtxAlphaChannel alphaChannel)
{
Width = width;
Height = height;

Pitch = pitch;
Format = format;

AlphaSize = alphaSize;
AlphaChannel = alphaChannel;
}

// ctor 2

public PtxInfo(uint width, uint height, PtxFormat format) : this(width, height, 0, format, 0, default)
{
}

// Read PtxInfo

public static PtxInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[28];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<PtxInfo>(rawData);
}

// Write PtxInfo

public void WriteBin(Stream writer, Endianness endian)
{
Span<byte> rawData = stackalloc byte[28];

if(endian == Endianness.BigEndian)
SwapEndian();

MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

// Reverse Endianness

public void SwapEndian()
{
Width = BinaryPrimitives.ReverseEndianness(Width);
Height = BinaryPrimitives.ReverseEndianness(Height);

Pitch = BinaryPrimitives.ReverseEndianness(Pitch);
Format = (PtxFormat)BinaryPrimitives.ReverseEndianness( (uint)Format);

AlphaSize = BinaryPrimitives.ReverseEndianness(AlphaSize);
AlphaChannel = (PtxAlphaChannel)BinaryPrimitives.ReverseEndianness( (uint)AlphaChannel);
}

}

}