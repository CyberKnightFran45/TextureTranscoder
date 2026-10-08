using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.SexyTexture
{
/// <summary> Represents Info for a SexyTexture. </summary>

[StructLayout(LayoutKind.Explicit, Size = 40) ]

public readonly struct SexyTexInfo
{
/// <summary> File Version </summary>

[FieldOffset(0)]
public readonly uint Version;

/// <summary> Texture Width </summary>

[FieldOffset(4)]
public readonly int Width;

/// <summary> Texture Height </summary>

[FieldOffset(8)]
public readonly int Height;

/// <summary> Texture Format </summary>

[FieldOffset(12)]
public readonly SexyTexFormat Format;

/// <summary> Type of Compression used </summary>

[FieldOffset(16)]
public readonly CompressionFlags CompressionType;

/// <summary> Mipmaps Count </summary>

[FieldOffset(20)]
public readonly int MipCount = 1;

/// <summary> Texture size after Compression </summary>

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

// ctor

public SexyTexInfo(int width, int height, SexyTexFormat format, CompressionFlags flags)
{
Width = width;
Height = height;

Format = format;
CompressionType = flags;
}

// Read SexyTexInfo

public static SexyTexInfo ReadBin(ReadOnlySpan<byte> source) => MemoryMarshal.Read<SexyTexInfo>(source);

// Write SexyTexInfo

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[40];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}
 
}

}