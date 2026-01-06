using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Represents a Container for GXT Images. </summary>

[StructLayout(LayoutKind.Explicit, Size = 28) ]

public readonly struct GxtContainer
{
/** <summary> Gets the File Version. </summary>
<returns> The FileVersion. </returns> */

[FieldOffset(0)]
public readonly uint Version = 0x10000003;

/** <summary> Gets the amount of Files embedded. </summary>
<returns> The Files embedded. </returns> */

[FieldOffset(4)]
public readonly int FileCount;

/** <summary> Gets a Offset to the Texture Data. </summary>
<returns> The Data Offset. </returns> */

[FieldOffset(8)]
public readonly int DataOffset = 64;

/** <summary> Gets the total Size of all the Textures embedded. </summary>
<returns> The FileSize. </returns> */

[FieldOffset(12)]
public readonly int FileSize;

/** <summary> Gets the number of 16-bits entry Palettes (P4) </summary>
<returns> The Palette Entries (P4). </returns> */

[FieldOffset(16)]
public readonly int PaletteEntries4;

/** <summary> Gets the number of 256-bits entry Palettes (P8) </summary>
<returns> The Palette Entries (P8). </returns> */

[FieldOffset(20)]
public readonly int PaletteEntries8;

/** <summary> Gets the amount of Padding used. </summary>
<returns> The Padding. </returns> */

[FieldOffset(24)]
public readonly int Padding;

/// <summary> Creates a new Instance of the <c>GxtInfo</c>. </summary>

public GxtContainer(int count)
{
FileCount = Math.Max(count, 0);
}

// Read GxtContainer

public static GxtContainer ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[28];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<GxtContainer>(rawData);
}

// Read Entries

public static List<GxtEntry> ReadEntries(Stream reader, int count)
{
count = Math.Max(count, 0);

List<GxtEntry> entries = new(count);

for(int i = 0; i < count; i++)
{
var entry = GxtEntry.ReadBin(reader);

entries.Add(entry);
}

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