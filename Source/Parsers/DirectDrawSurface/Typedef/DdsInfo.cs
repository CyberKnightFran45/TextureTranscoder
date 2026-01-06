using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.DirectDrawSurface
{
/// <summary> Represents Info for a DirectDraw Surface. </summary>

[StructLayout(LayoutKind.Explicit, Size = 124) ]

public unsafe struct DdsInfo
{
/** <summary> Gets the Size in bytes of Header Section. </summary>
<returns> The Section Length. </returns> */

[FieldOffset(0)]
public readonly uint SectionLength = 124;

/** <summary> Gets the DDSD Flags. </summary>
<returns> The DDDS Flags. </returns> */

[FieldOffset(4)]
public readonly uint DdsFlags = 528391;

/** <summary> Gets or Sets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(8)]
public readonly int Height;

/** <summary> Gets or Sets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(12)]
public readonly int Width;

/** <summary> Gets or Sets the Texture Pitch. </summary>
<returns> The TexturePitch. </returns> */

[FieldOffset(16)]
public readonly int Pitch;

/// <summary> Some padding </summary>

[FieldOffset(20)]
private fixed int Padding[11];

/** <summary> Gets some flags that describe the Tool used for Parsing the Texture. </summary>

<remarks> PS3 use NVidia Texture Tools (NVTT) </remarks>

<returns> The Texture flags. </returns> */

[FieldOffset(64)]
public readonly uint DwFlags = 0x5454564E;

/** <summary> Gets the DW Identifier. </summary>
<returns> The DW Identifier. </returns> */

[FieldOffset(68)]
public readonly uint DwIdentifier = 131080;

/** <summary> Gets the DW Size. </summary>
<returns> The DW Size. </returns> */

[FieldOffset(72)]
public readonly uint DwSize = 32;

/** <summary> Gets the amount of bits per Pixel. </summary>
<returns> The Bits per Pixel. </returns> */

[FieldOffset(76)]
public readonly uint Bpp = 4;

/** <summary> Gets or Sets the Texture Format. </summary>
<returns> The TextureFormat. </returns> */

[FieldOffset(80)]
public readonly DdsFormat Format;

/** <summary> Gets or Sets a Bitmask for Red Component (always 0). </summary>
<returns> The Red Bitmask. </returns> */

[FieldOffset(84)]
public readonly int RedMask;

/** <summary> Gets or Sets a Bitmask for Green Component (always 0). </summary>
<returns> The Green Bitmask. </returns> */

[FieldOffset(88)]
public readonly int GreenMask;

/** <summary> Gets or Sets a Bitmask for Blue Component (always 0). </summary>
<returns> The Blue Bitmask. </returns> */

[FieldOffset(92)]
public readonly int BlueMask;

/** <summary> Gets or Sets a Bitmask for Alpha Channel (always 0). </summary>
<returns> The Alpha Bitmask. </returns> */

[FieldOffset(96)]
public readonly int AlphaMask;

/** <summary> Gets the Texture Depth (3D only). </summary>
<returns> The TextureDepth. </returns> */

[FieldOffset(100)]
public readonly int Depth;

/** <summary> Gets the Surface Width. </summary>
<returns> The SurfaceWidth. </returns> */

[FieldOffset(104)]
public readonly int SurfaceWidth = 4096;

/** <summary> Gets the Primary DDS Caps (not used). </summary>
<returns> The Primary Caps. </returns> */

[FieldOffset(108)]
public readonly int DwCaps;

/** <summary> Gets the Secondary DDS Caps (not used). </summary>
<returns> The Primary Caps. </returns> */

[FieldOffset(112)]
public readonly int DwCaps2;

/// <summary> Reserved </summary>

[FieldOffset(116)]
public readonly int DwCaps3;

/// <summary> Reserved </summary>

[FieldOffset(120)]
public readonly int DwCaps4;

/// <summary> Creates a new Instance of the <c>DdsInfo</c>. </summary>

public DdsInfo(int width, int height, DdsFormat format)
{
Width = width;
Height = height;

Format = format;
Pitch = TextureHelper.ComputeSize(width, height);
}

// Read DdsInfo

public static DdsInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[124];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<DdsInfo>(rawData);
}

// Write DdsInfo

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[124];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}