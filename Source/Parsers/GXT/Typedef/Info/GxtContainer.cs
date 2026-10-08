using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Represents a Container for GXT Images. </summary>

[StructLayout(LayoutKind.Explicit, Size = 28) ]

public readonly struct GxtContainer
{
/// <summary> File Version </summary>

[FieldOffset(0)]
public readonly uint Version = 0x10000003;

/// <summary> Amount of Files embedded. </summary>

[FieldOffset(4)]
public readonly int FileCount;

/// <summary> Offset to Texture Data. </summary>

[FieldOffset(8)]
public readonly int DataOffset = 64;

/// <summary> Total Size of all the Textures embedded. </summary>

[FieldOffset(12)]
public readonly int FileSize;

/// <summary> Number of 16-bits entry Palettes (P4) </summary>

[FieldOffset(16)]
public readonly int PaletteEntries4;

/// <summary> Number of 256-bits entry Palettes (P8) </summary>

[FieldOffset(20)]
public readonly int PaletteEntries8;

/// <summary> Amount of Padding used. </summary>

[FieldOffset(24)]
public readonly int Padding;

// ctor

public GxtContainer(int count, int fileSize)
{
FileCount = Math.Max(count, 0);
FileSize = fileSize;
}

// Read GxtContainer

public static GxtContainer ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[28];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<GxtContainer>(rawData);
}

// Read Entries

public static GxtEntry[] ReadEntries(Stream reader, int count)
{
count = Math.Max(count, 0);

GxtEntry[] entries = new GxtEntry[count];

for(int i = 0; i < count; i++)
entries[i] = GxtEntry.ReadBin(reader);

return entries;
}

// Write GxtInfo

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[28];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}