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

/// <summary> Size in bytes of the Texture </summary>

[FieldOffset(4)]
public readonly int Size;

/// <summary> Index of the Palette </summary>

[FieldOffset(8)]
public readonly int PaletteIndex = -1;

/// <summary> Special flags (not used) </summary>

[FieldOffset(12)]
public readonly int Flags;

/// <summary> Texture type (defaults to Swizzled) </summary>

[FieldOffset(16)]
public readonly GxtTextureType Type;

/// <summary> Base format (default use DXT5) </summary>

[FieldOffset(20)]
public readonly GxtFormat Format;

/** <summary> Gets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(24)]	
public readonly ushort Width;

/** <summary> Gets the Texture Height. </summary>
<returns> The TextureHeight. </returns> */

[FieldOffset(26)]
public readonly ushort Height;

/// <summary> Amount of MipMaps. </summary>

[FieldOffset(28)]
public readonly uint MipCount = 1;

/// <summary> Creates a new Instance of the <c>GxtEntry</c>. </summary>

public GxtEntry(int width, int height, GxtFormat format, long offset, int size, GxtTextureType type)
{
Width = (ushort)width;
Height = (ushort)height;

Format = format;
Offset = (int)offset;

Size = size;
Type = type;
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