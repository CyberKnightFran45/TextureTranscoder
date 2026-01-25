using System.Runtime.InteropServices;

// Represents a PVR2 Packet (4bpp mode)

[StructLayout(LayoutKind.Explicit, Size = 8) ]

public struct PVR2Packet4BPP
{
// Packet data
	
[FieldOffset(0)]
public ulong Data;

// ctor

public PVR2Packet4BPP(in TextureColor16 colorA, in TextureColor16 colorB, uint modData,
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

readonly get => (uint)BitHelper.Extract(Data, 0, 32);
set => Data = BitHelper.Insert(Data, value, 0, 32);

}

// Wheter to use Modulated Interpolation

public bool InterpolatedModulation
{

readonly get => BitHelper.Extract(Data, 32, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 32, 1);

}

// Wheter to use Hard Transition or not

public bool HardTransition
{

readonly get => BitHelper.Extract(Data, 33, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 33, 1);

}

// Opacity (global)

public bool Opacity
{

readonly get =>	BitHelper.Extract(Data, 34, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 34, 1);
}

// Color A

private int ColorA
{

readonly get => (int)BitHelper.Extract(Data, 51, 16);
set => Data = BitHelper.Insert(Data, (ulong)value, 51, 16);
	
}

// Color B

private int ColorB
{

readonly get => (int)BitHelper.Extract(Data, 35, 16);
set => Data = BitHelper.Insert(Data, (ulong)value, 35, 16);

}

// Get ColorA

public readonly TextureColor16 GetColorA(bool useAlpha) => PVR2Base.DecodeColor(ColorA, Opacity, useAlpha);

// Get ColorB

public readonly TextureColor16 GetColorB(bool useAlpha) => PVR2Base.DecodeColor(ColorB, Opacity, useAlpha);
}