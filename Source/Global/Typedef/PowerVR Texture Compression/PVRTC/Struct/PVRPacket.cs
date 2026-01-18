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

// Modulation Data

public uint ModulationData
{

readonly get => (uint)BitHelper.Extract(Word, 0, 32);
set => Word = BitHelper.Insert(Word, value, 0, 32);

}

// Color A

private int ColorA
{

readonly get => (int)BitHelper.Extract(Word, 33, 14);
set => Word = BitHelper.Insert(Word, (ulong)value, 33, 14);

}

// Opacity for ColorA

private bool OpacityA
{

readonly get => BitHelper.Extract(Word, 47, 1) == 1;
set => Word = BitHelper.Insert(Word, value ? 1uL : 0uL, 47, 1);

}


// ColorB

private int ColorB
{

readonly get => (int)BitHelper.Extract(Word, 48, 14);
set => Word = BitHelper.Insert(Word, (ulong)value, 48, 14);

}

// Opacity for ColorB

private bool OpacityB
{

readonly get => BitHelper.Extract(Word, 63, 1) == 1;
set => Word = BitHelper.Insert(Word, value ? 1uL : 0uL, 63, 1);

}

// Get Color A

public readonly TextureColor16 GetColorA(bool useAlpha)
{
int flags = ColorA;

byte a, r, g, b;

if(OpacityA)
{
a = 255;
r = BitHelper.ExtractAndExpandTo8(flags, 9, 5);
g = BitHelper.ExtractAndExpandTo8(flags, 4, 5);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 4);
}

else
{
a = useAlpha ? BitHelper.ExtractAndExpandTo8(flags, 11, 3) : (byte)255;
r = BitHelper.ExtractAndExpandTo8(flags, 7, 4);
g = BitHelper.ExtractAndExpandTo8(flags, 3, 4);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 3);
}

return new(r, g, b, a);
}

// Get Color B

public readonly TextureColor16 GetColorB(bool useAlpha)
{
int flags = ColorB;

byte a, r, g, b;

if(OpacityB)
{
a = 255;
r = BitHelper.ExtractAndExpandTo8(flags, 10, 5);
g = BitHelper.ExtractAndExpandTo8(flags, 5, 5);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 5);
}

else
{
a = useAlpha ? BitHelper.ExtractAndExpandTo8(flags, 12, 3) : (byte)255;
r = BitHelper.ExtractAndExpandTo8(flags, 8, 4);
g = BitHelper.ExtractAndExpandTo8(flags, 4, 4);
b = BitHelper.ExtractAndExpandTo8(flags, 0, 4);
}

return new(r, g, b, a);
}

// Set ColorA

public void SetColorA(in TextureColor16 color, bool useAlpha)
{
int a = useAlpha ? BitHelper.QuantizeFrom8(color.Alpha, 3) : 7;
int flags = 0;

if(a == 7)
{
int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g5 = BitHelper.QuantizeFrom8(color.Green, 5);
int b4 = BitHelper.QuantizeFrom8(color.Blue, 4);

flags = BitHelper.Insert(flags, r5, 9, 5);
flags = BitHelper.Insert(flags, g5, 4, 5);
flags = BitHelper.Insert(flags, b4, 0, 4);

OpacityA = true;
}

else
{
int r4 = BitHelper.QuantizeFrom8(color.Red, 4);
int g4 = BitHelper.QuantizeFrom8(color.Green, 4);
int b3 = BitHelper.QuantizeFrom8(color.Blue, 3);

flags = BitHelper.Insert(flags, a, 11, 3);
flags = BitHelper.Insert(flags, r4, 7, 4);
flags = BitHelper.Insert(flags, g4, 3, 4);
flags = BitHelper.Insert(flags, b3, 0, 3);

OpacityA = false;
}

ColorA = flags;
}

// Set ColorB

public void SetColorB(in TextureColor16 color, bool useAlpha)
{
int a = useAlpha ? BitHelper.QuantizeFrom8(color.Alpha, 3) : 7;
int flags = 0;

if(a == 7)
{
int r5 = BitHelper.QuantizeFrom8(color.Red, 5);
int g5 = BitHelper.QuantizeFrom8(color.Green, 5);
int b5 = BitHelper.QuantizeFrom8(color.Blue, 5);

flags = BitHelper.Insert(flags, r5, 10, 5);
flags = BitHelper.Insert(flags, g5, 5, 5);
flags = BitHelper.Insert(flags, b5, 0, 5);

OpacityB = true;
}

else
{
int r4 = BitHelper.QuantizeFrom8(color.Red, 4);
int g4 = BitHelper.QuantizeFrom8(color.Green, 4);
int b4 = BitHelper.QuantizeFrom8(color.Blue, 4);

flags = BitHelper.Insert(flags, a, 12, 3);
flags = BitHelper.Insert(flags, r4, 8, 4);
flags = BitHelper.Insert(flags, g4, 4, 4);
flags = BitHelper.Insert(flags, b4, 0, 4);

OpacityB = false;
}

ColorB = flags;
}

}