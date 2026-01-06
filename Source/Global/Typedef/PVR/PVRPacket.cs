using System.Runtime.InteropServices;

// Represents a PVR Packet

[StructLayout(LayoutKind.Explicit, Size = 8)]

public struct PVRPacket
{
// PVR Word

[FieldOffset(0)]
public ulong Word;

// ctor

public PVRPacket(ulong word)
{
Word = word;
}

// Get Mod Value

private static uint GetModValue(ulong word) => (uint)(word & 0xFFFFFFFF);

// Set Mod Value

private static ulong SetModValue(ulong word, uint mod) => (word & 0xFFFFFFFF) | mod;

// Modulation Data

public readonly uint ModulationData
{

get => GetModValue(Word);
set => SetModValue(Word, value);

}

// Get Alpha Mode

private static bool GetAlphaMode(ulong word) => ( (word >> 32) & 0b1) == 1;

// Set Alpha Mode

private static ulong SetAlphaMode(ulong word, bool mode)
{
return mode ? word | (1UL << 32) : word & ~(1UL << 32);
}

// Wheter to use Alpha Mask

public bool UseAlphaMask
{

readonly get => GetAlphaMode(Word);
set => Word = SetAlphaMode(Word, value);

}

private static int GetFlags(ulong word, int factor) => (int)( (word >> factor) & 0x3FFF);

// Set Color Flags

private static ulong SetFlags(ulong word, int flags, int factor)
{
return (word & ~(0x3FFFuL << factor)) | ((ulong)(flags & 0x3FFF) << factor);
}

// Color A

public int ColorA
{

readonly get => GetFlags(Word, 33);
set => Word = SetFlags(Word, value, 33);

}

// Get Opaque Mode

private static bool GetOpaqueMode(ulong word, int factor) => ( (word >> factor) & 0b1) == 1;

// Set Opaque Mode

private static ulong SetOpaqueMode(ulong word, bool mode, int factor)
{
return mode ? word | (1UL << factor) : word & ~(1UL << factor);
}

// ColorA Opaque

public bool OpaqueModeA
{

readonly get => GetOpaqueMode(Word, 47);
set => Word = SetOpaqueMode(Word, value, 47);

}

// ColorB

public int ColorB
{

readonly get => GetFlags(Word, 48);
set => Word = SetFlags(Word, value, 48);

}

// ColorB Opaque

public bool OpaqueModeB
{

readonly get => GetOpaqueMode(Word, 63);
set => Word = SetOpaqueMode(Word, value, 63);

}

// Unpack Bits from Mask

private static byte UnpackBits(int mask) => (byte)( (mask << 4) | mask);

// Expand Color

private static byte ExpandBits(int mask) => (byte)( (mask << 3) | (mask >> 2) );

// Replicate bits

private static byte ReplicateBits(int mask) => (byte)( (mask << 5) | (mask << 2) | (mask >> 1) );

// Get Color A

public readonly TextureColor16 GetColorA(bool useAlpha)
{
int flags = ColorA;

byte r, g, b, a;

if(OpaqueModeA)
{
int redMask = flags >> 9;
int greenMask = (flags >> 4) & 0x1F;
int blueMask = flags & 0xF;

r = ExpandBits(redMask);
g = ExpandBits(greenMask);
b = UnpackBits(blueMask);
a = 255;
}

else
{
int redMask = (flags >> 7) & 0xF;
int greenMask = (flags >> 3) & 0xF;
int blueMask = flags & 0x7;
int alphaMask = useAlpha ? (flags >> 11) & 0x7 : 31;

r = UnpackBits(redMask);
g = UnpackBits(greenMask);
b = ReplicateBits(blueMask);
a = ReplicateBits(alphaMask);
}

return new(r, g, b, a);
}

// Get Color B

public readonly TextureColor16 GetColorB(bool useAlpha)
{
int flags = ColorB;

byte r, g, b, a;

if(OpaqueModeB)
{
int redMask = flags >> 10;
r = ExpandBits(redMask);

int greenMask = (flags >> 5) & 0x1F;
g = ExpandBits(greenMask);

int blueMask = flags & 0x1F;
b = ExpandBits(blueMask);

a = 255;
}

else
{
int redMask = (flags >> 8) & 0xF;
r = UnpackBits(redMask);

int greenMask = (flags >> 4) & 0xF;
g = UnpackBits(greenMask);

int blueMask = flags & 0xF;
b = UnpackBits(blueMask);

int alphaMask = useAlpha ? (flags >> 12) & 0x7 : 31;
a = ReplicateBits(alphaMask);
}

return new(r, g, b, a);
}

// Set ColorA

public void SetColorA(TextureColor16 color, bool useAlpha)
{
int a = useAlpha ? color.Alpha >> 5 : 0x7;
	
if(a == 0x7)
{
int r = color.Red >> 3;
int g = color.Green >> 3;
int b = color.Blue >> 4;

ColorA = r << 9 | g << 4 | b;
OpaqueModeA = true;
}

else
{
int r = color.Red >> 4;
int g = color.Green >> 4;
int b = color.Blue >> 5;

ColorA = a << 11 | r << 7 | g << 3 | b;
OpaqueModeA = false;
}

}

// Set ColorB

public void SetColorB(TextureColor16 color, bool useAlpha)
{
int a = useAlpha ? color.Alpha >> 5 : 0x7;

if(a == 0x7)
{
int r = color.Red >> 3;
int g = color.Green >> 3;
int b = color.Blue >> 3;

ColorB = r << 10 | g << 5 | b;
OpaqueModeB = true;
}

else
{
int r = color.Red >> 4;
int g = color.Green >> 4;
int b = color.Blue >> 4;

ColorB = a << 12 | r << 8 | g << 4 | b;
OpaqueModeB = false;
}

}

}