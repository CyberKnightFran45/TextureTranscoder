using System;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.UTexture
{
/// <summary> Represents Info for a U-Texture. </summary>

[StructLayout(LayoutKind.Explicit, Size = 6) ]

public readonly struct UTexInfo
{
/// <summary> Texture Width </summary>

[FieldOffset(0)]
public readonly ushort Width;

/// <summary> Texture Height </summary>

[FieldOffset(2)]
public readonly ushort Height;

/// <summary> Texture Format </summary>

[FieldOffset(4)]	
public readonly UTexFormat Format;

// ctor

public UTexInfo(int width, int height, UTexFormat format)
{
Width = (ushort)width;
Height = (ushort)height;

Format = format;
}

// Read UTexInfo

public static UTexInfo ReadBin(ReadOnlySpan<byte> source) => MemoryMarshal.Read<UTexInfo>(source);

// Write UTexInfo

public readonly void WriteBin(Span<byte> destination) => MemoryMarshal.Write(destination, this);
}

}