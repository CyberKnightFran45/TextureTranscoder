using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.PopCapTexture
{
/// <summary> Stores info related to a Encoded PTX Image (used by the Tool) </summary>

[StructLayout(LayoutKind.Explicit, Size = 28) ]

public struct PtxInfo
{
/** <summary> Gets a Identifier for this Struct </summary>
<returns> The Ptx Identifier. </returns> */

[FieldOffset(0)]
public readonly uint Magic = 0x70747831;

/** <summary> Gets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(4)]
public int Width;

/** <summary> Gets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(8)]
public int Height;

/** <summary> Gets the Texture Pitch. </summary>
<returns> The TexturePitch. </returns> */

[FieldOffset(12)]
public int Pitch;

/** <summary> Gets the Texture Format. </summary>
<returns> The TextureFormat. </returns> */

[FieldOffset(16)]
public PtxFormat Format;

/** <summary> Gets the amount of bytes written in AlphaChannel </summary>
<returns> The AlphaSize. </returns> */

[FieldOffset(20)]
public int AlphaSize;

/** <summary> Gets the type of Alpha used. </summary>
<returns> The AlphaChannel. </returns> */

[FieldOffset(24)]
public PtxAlphaChannel AlphaChannel;

// ctor

public PtxInfo(int width, int height, int pitch, PtxFormat format,
               int alphaSize, PtxAlphaChannel alphaChannel)
{
Width = width;
Height = height;

Pitch = pitch;
Format = format;

AlphaSize = alphaSize;
AlphaChannel = alphaChannel;
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