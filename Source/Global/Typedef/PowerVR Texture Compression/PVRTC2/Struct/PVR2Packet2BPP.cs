using System.Runtime.InteropServices;

// Represents a PVR2 Packet (2bpp mode)

[StructLayout(LayoutKind.Explicit, Size = 8) ]

public struct PVR2Packet2BPP
{
// Color flags (A, B)

[FieldOffset(0)]
public uint Data0;

// Modulation flags (Mode, Data)

[FieldOffset(4)]
public uint Data1;

// ctor

public PVR2Packet2BPP(in TextureColor16 colorA, in TextureColor16 colorB, uint modData,
                      bool modInterpolate, bool hardTransition, bool useAlpha)
{
ModulationData = modData;

InterpolatedModulation = modInterpolate;
HardTransition = hardTransition;

int a = PVR2Base.EncodeColor(colorA, useAlpha, out bool opaqueA);
int b = PVR2Base.EncodeColor(colorB, useAlpha, out bool opaqueB);

Opacity = opaqueA && opaqueB;

ColorA = a;
ColorB = b;
}

// Modulation Data

public uint ModulationData
{

readonly get => (uint)BitHelper.Extract(Data1, 0, 29);
set => Data1 = BitHelper.Insert(Data1, value, 0, 29);

}

// Wheter to use Modulated Interpolation

public bool InterpolatedModulation
{

readonly get => BitHelper.Extract(Data1, 30, 1) == 1;
set => Data1 = BitHelper.Insert(Data1, value ? 1u : 0u, 30, 1);

}

// Wheter to use Hard Transition or not

public bool HardTransition
{

readonly get => BitHelper.Extract(Data1, 31, 1) == 1;
set => Data1 = BitHelper.Insert(Data1, value ? 1u : 0u, 31, 1);

}

// Color A

private int ColorA
{

readonly get => BitHelper.Extract(Data0, 16, 15);
set => Data0 = BitHelper.Insert(Data0, (uint)value, 16, 15);

}

// Color B

private int ColorB
{

readonly get => BitHelper.Extract(Data0, 0, 15);
set => Data0 = BitHelper.Insert(Data0, (uint)value, 0, 15);

}

// Opacity (Global)

private bool Opacity
{

readonly get => BitHelper.Extract(Data0, 31, 1) == 1;
set => Data0 = BitHelper.Insert(Data0, value ? 1u : 0u, 31, 1);

}

// Get ColorA

public readonly TextureColor16 GetColorA(bool useAlpha) => PVR2Base.DecodeColor(ColorA, Opacity, useAlpha);

// Get ColorB

public readonly TextureColor16 GetColorB(bool useAlpha) => PVR2Base.DecodeColor(ColorB, Opacity, useAlpha);
}