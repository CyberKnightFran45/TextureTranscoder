using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.UTexture
{
// <summary> Represents Info for a U-Texture. </summary>

[StructLayout(LayoutKind.Explicit, Size = 6) ]

public readonly struct UTexInfo
{
/** <summary> Gets or Sets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(0)]
public readonly ushort Width;

/** <summary> Gets or Sets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(2)]
public readonly ushort Height;

/** <summary> Gets or Sets the Texture Format. </summary>
<returns> The TextureFormat. </returns> */

[FieldOffset(4)]	
public readonly UTexFormat Format;

/// <summary> Creates a new Instance of the <c>UTexInfo</c>. </summary>

public UTexInfo(int width, int height, UTexFormat format)
{
Width = (ushort)width;
Height = (ushort)height;

Format = format;
}

// Read UTexInfo

public static UTexInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[6];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<UTexInfo>(rawData);
}

// Write XnbInfo

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[6];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}