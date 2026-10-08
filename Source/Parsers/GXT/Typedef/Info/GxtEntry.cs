using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Represents a Entry for a GXT Texture. </summary>

[StructLayout(LayoutKind.Explicit, Size = 32) ]

public readonly struct GxtEntry
{
/// <summary> An Offset to this Texture. </summary>

[FieldOffset(0)]
public readonly int Offset;

/// <summary> Texture Size (in bytes) </summary>

[FieldOffset(4)]
public readonly int Size;

/// <summary> Index of the Palette </summary>

[FieldOffset(8)]
public readonly int PaletteIndex;

/// <summary> Special flags (not used) </summary>

[FieldOffset(12)]
public readonly int Flags;

/// <summary> Texture type (defaults to Swizzled) </summary>

[FieldOffset(16)]
public readonly GxtTextureType Type;

/// <summary> Base format (default use DXT5) </summary>

[FieldOffset(20)]
public readonly GxtFormat Format;

/// <summary> Texture Width </summary>

[FieldOffset(24)]	
public readonly ushort Width;

/// <summary> Texture Height. </summary>

[FieldOffset(26)]
public readonly ushort Height;

/// <summary> Amount of MipMaps. </summary>

[FieldOffset(28)]
public readonly uint MipCount;

// ctor

public GxtEntry(int width, int height, long offset, int size, GxtImageParams info)
{
Offset = (int)offset;
Size = size;

PaletteIndex = info.PaletteIndex;
Flags = info.Flags;

Type = info.Type;
Format = info.Format;

Width = (ushort)width;
Height = (ushort)height;

MipCount = info.MipCount;
}

// Read GxtEntry

public static GxtEntry ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[32];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<GxtEntry>(rawData);
}

// Write GxtEntry

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[32];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}