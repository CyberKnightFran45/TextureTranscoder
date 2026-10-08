using System;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.DirectDrawSurface
{
/// <summary> Represents Info for a DirectDraw Surface. </summary>

[StructLayout(LayoutKind.Explicit, Size = 124) ]

public unsafe struct DdsInfo
{
/// <summary> Header Section Length (in bytes) </summary>

[FieldOffset(0)]
public readonly uint SectionLength = 124;

/// <summary> DDSD Flags </summary>

[FieldOffset(4)]
public readonly uint DdsFlags = 528391;

/// <summary> Texture Height </summary>

[FieldOffset(8)]
public readonly int Height;

/// <summary> Texture Width </summary>

[FieldOffset(12)]
public readonly int Width;

/// <summary> Texture Pitch </summary>

[FieldOffset(16)]
public readonly int Pitch;

/// <summary> Some padding </summary>

[FieldOffset(20)]
private fixed int Padding[11];

/** <summary> Flags that describe the Tool used for Parsing the Texture. </summary>

<remarks> PS3 use NVidia Texture Tools (NVTT) </remarks> */

[FieldOffset(64)]
public readonly uint DwFlags = 0x5454564E;

/// <summary> DW Identifier. </summary>

[FieldOffset(68)]
public readonly uint DwIdentifier = 131080;

/// <summary> DW Size. </summary>

[FieldOffset(72)]
public readonly uint DwSize = 32;

/// <summary> Bits per Pixel </summary>

[FieldOffset(76)]
public readonly uint Bpp = 4;

/// <summary> Texture Format </summary>

[FieldOffset(80)]
public readonly DdsFormat Format;

/// <summary> Bitmask for Red Component (always 0) </summary>

[FieldOffset(84)]
public readonly int RedMask;

/// <summary> Bitmask for Green Component (always 0) </summary>

[FieldOffset(88)]
public readonly int GreenMask;

/// <summary> Bitmask for Blue Component (always 0) </summary>

[FieldOffset(92)]
public readonly int BlueMask;

/// <summary> Bitmask for Alpha Channel (always 0) </summary>

[FieldOffset(96)]
public readonly int AlphaMask;

/// <summary> Texture Depth (3D only) </summary>

[FieldOffset(100)]
public readonly int Depth;

/// <summary> Surface Width </summary>

[FieldOffset(104)]
public readonly int SurfaceWidth = 4096;

/// <summary> Primary DDS Caps (not used) </summary>

[FieldOffset(108)]
public readonly int DwCaps;

/// <summary> Secondary DDS Caps (not used) </summary>

[FieldOffset(112)]
public readonly int DwCaps2;

/// <summary> Reserved </summary>

[FieldOffset(116)]
public readonly int DwCaps3;

/// <summary> Reserved </summary>

[FieldOffset(120)]
public readonly int DwCaps4;

// ctor

public DdsInfo(int width, int height, DdsFormat format)
{
Width = width;
Height = height;

Format = format;
Pitch = TextureHelper.ComputeSize(width, height);
}

// Read DdsInfo

public static DdsInfo ReadBin(ReadOnlySpan<byte> source)
{
return MemoryMarshal.Read<DdsInfo>(source);
}

// Write DdsInfo

public readonly void WriteBin(Span<byte> destination)
{
MemoryMarshal.Write(destination, this);
}

}

}