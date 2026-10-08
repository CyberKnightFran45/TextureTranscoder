using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Represents Info for a XNB Image. </summary>

[StructLayout(LayoutKind.Explicit, Size = 20) ]

public readonly struct XnbTextureInfo
{
/// <summary> Surface Format </summary>

[FieldOffset(0)]
public readonly XnbFormat Format;

/// <summary> Texture Width </summary>

[FieldOffset(4)]
public readonly int Width;

/// <summary> Texture Height </summary>

[FieldOffset(8)]
public readonly int Height;

/// <summary> Mipmaps count </summary>

[FieldOffset(12)]
public readonly int MipCount = 1;

/// <summary> Mip size </summary>

[FieldOffset(16)]
public readonly int DataSize;

// ctor

public XnbTextureInfo(int width, int height, XnbFormat format, int dataSize)
{
Width = width;
Height = height;

Format = format;
DataSize = dataSize;
}

// Read XnbInfo

public static XnbTextureInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[20];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<XnbTextureInfo>(rawData);
}

// Write XnbInfo  

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[20];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}