using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.SexyTexture
{
/// <summary> Represents Info for a SexyTexture. </summary>

[StructLayout(LayoutKind.Explicit, Size = 40) ]

public readonly struct SexyTexInfo
{
/** <summary> Gets the SexyTex Version. </summary>
<returns> The File Version. </returns> */

[FieldOffset(0)]
public readonly uint Version;

/** <summary> Gets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(4)]
public readonly int Width;

/** <summary> Gets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(8)]
public readonly int Height;

/** <summary> Gets the Texture Format. </summary>
<returns> The TextureFormat. </returns> */

[FieldOffset(12)]
public readonly SexyTexFormat Format;

/** <summary> Gets the Type of Compression used in the SexyTexture. </summary>
<returns> The Compression Type. </returns> */

[FieldOffset(16)]
public readonly CompressionFlags CompressionType;

/** <summary> Gets the MipCount. </summary>
<returns> The MipCount. </returns> */

[FieldOffset(20)]
public readonly int MipCount = 1;

/** <summary> Gets the Size of the Texture after Compression. </summary>
<returns> The Size Compressed. </returns> */

[FieldOffset(24)]
public readonly int SizeCompressed;

/// <summary> Unknown field (always 0) </summary>

[FieldOffset(28)]
private readonly int Reserved;

/// <summary> Unknown field (always 0) </summary>

[FieldOffset(32)]
private readonly int Reserved2;

/// <summary> Unknown field (always 0) </summary>

[FieldOffset(36)]
private readonly int Reserved3;

/// <summary> Creates a new Instance of the <c>SexyTexInfo</c>. </summary>

public SexyTexInfo(int width, int height, SexyTexFormat format, CompressionFlags flags)
{
Width = width;
Height = height;

Format = format;
CompressionType = flags;
}

// Read SexyTexInfo

public static SexyTexInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[40];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<SexyTexInfo>(rawData);
}

// Write SexyTexInfo

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[40];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}
 
}

}