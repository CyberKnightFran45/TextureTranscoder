using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Header of a XNB file. </summary>

[StructLayout(LayoutKind.Explicit, Size = 7) ]

public readonly struct XnbHeader
{
/// <summary> Platform ID </summary>

[FieldOffset(0)]
public readonly XnbPlatform PlatformID;

/// <summary> File Version (V5) </summary>

[FieldOffset(1)]
public readonly byte Version = 5;

/// <summary> Special flags </summary>

[FieldOffset(2)]
public readonly byte Flags;

/// <summary> Texture size after Compression </summary>

[FieldOffset(3)]
public readonly int SizeCompressed;

// ctor

public XnbHeader(XnbPlatform platform)
{
PlatformID = platform;
}

public XnbHeader(XnbPlatform platform, byte version, byte flags, int sizeCompressed)
{
PlatformID = platform;
Version = version;

Flags = flags;
SizeCompressed = sizeCompressed;
}

// Read XnbInfo

public static XnbHeader ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[7];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<XnbHeader>(rawData);
}

// Write XnbInfo  

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[7];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}