using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.XnaGameStudio
{
/// <summary> Represents Info for a XNB Image. </summary>

[StructLayout(LayoutKind.Explicit, Size = 184) ]

public unsafe struct XnbInfo
{
// Defines the Texture Type

private static readonly byte[] TEXTURE_TYPE_2D = 
[
    0x01, 0x94, 0x01, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74, 0x2E,
	0x58, 0x6E, 0x61, 0x2E, 0x46, 0x72, 0x61, 0x6D, 0x65, 0x77, 0x6F, 0x72,
	0x6B, 0x2E, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x2E, 0x54, 0x65,
	0x78, 0x74, 0x75, 0x72, 0x65, 0x32, 0x44, 0x52, 0x65, 0x61, 0x64, 0x65,
	0x72, 0x2C, 0x20, 0x4D, 0x69, 0x63, 0x72, 0x6F, 0x73, 0x6F, 0x66, 0x74,
	0x2E, 0x58, 0x6E, 0x61, 0x2E, 0x46, 0x72, 0x61, 0x6D, 0x65, 0x77, 0x6F,
	0x72, 0x6B, 0x2E, 0x47, 0x72, 0x61, 0x70, 0x68, 0x69, 0x63, 0x73, 0x2C,
	0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x34, 0x2E, 0x30,
	0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20, 0x43, 0x75, 0x6C, 0x74, 0x75, 0x72,
	0x65, 0x3D, 0x6E, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C, 0x20, 0x50,
	0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B, 0x65, 0x79, 0x54, 0x6F, 0x6B, 0x65,
	0x6E, 0x3D, 0x38, 0x34, 0x32, 0x63, 0x66, 0x38, 0x62, 0x65, 0x31, 0x64,
	0x65, 0x35, 0x30, 0x35, 0x35, 0x33, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01
];

/** <summary> Gets the PlatformID of the XNB. </summary>
<returns> The PlatformID. </returns> */

[FieldOffset(0)]
public readonly XnbPlatform PlatformID;

/** <summary> Gets the XNB Version. </summary>
<returns> The File Version. </returns> */

[FieldOffset(1)]
public readonly byte Version = 5;

/** <summary> Gets some flag bits. </summary>
<returns> The Flags. </returns> */

[FieldOffset(2)]
public readonly byte Flags;

/** <summary> Gets the Size in bytes of the File Compressed. </summary>
<returns> The SizeCompressed. </returns> */

[FieldOffset(3)]
public readonly int SizeCompressed;

/** <summary> Gets an AssemblyInfo that describe the Texture Type. </summary>
<returns> The TextureType. </returns> */

[FieldOffset(7)]
public fixed byte TextureType[157];

/** <summary> Gets the Surface Format. </summary>
<returns> The Format. </returns> */

[FieldOffset(164)]
public readonly XnbFormat Format;

/** <summary> Gets the Texture Width. </summary>
<returns> The TextureWidth. </returns> */

[FieldOffset(168)]
public readonly int Width;

/** <summary> Gets the Texture Height. </summary>
<returns> The Texture Height. </returns> */

[FieldOffset(172)]
public readonly int Height;

/** <summary> Gets the MipCount. </summary>
<returns> The MipCount. </returns> */

[FieldOffset(176)]
public readonly int MipCount = 1;

/** <summary> Gets the Image size in bytes. </summary>
<returns> The TextureSize. </returns> */

[FieldOffset(180)]
public readonly int TextureSize;

/// <summary> Creates a new Instance of the <c>XnbInfo</c>. </summary>

public XnbInfo(int width, int height, XnbPlatform platform, XnbFormat format)
{
PlatformID = platform;

Width = width;
Height = height;

Format = format;
TextureSize = TextureHelper.ComputeSize(width, height, 2);

// Init Metadata

fixed(byte* dst = TextureType)

fixed(byte* src = TEXTURE_TYPE_2D)
Unsafe.CopyBlock(dst, src, (uint)TEXTURE_TYPE_2D.Length);

}

// Read XnbInfo

public static XnbInfo ReadBin(Stream reader)
{
Span<byte> rawData = stackalloc byte[184];
reader.ReadExactly(rawData);

return MemoryMarshal.Read<XnbInfo>(rawData);
}

// Write XnbInfo  

public readonly void WriteBin(Stream writer)
{
Span<byte> rawData = stackalloc byte[184];
MemoryMarshal.Write(rawData, this);

writer.Write(rawData);
}

}

}