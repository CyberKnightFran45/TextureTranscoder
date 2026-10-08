using System.Runtime.InteropServices;

// Represents a PVR2 Packet (4bpp mode)

[StructLayout(LayoutKind.Explicit, Size = 8) ]

public struct PVR2Packet4BPP
{
// High word: ColorA, ColorB, Opacity, HardTransition

[FieldOffset(4)]
public uint Data0;

// Low word: Modulation data

[FieldOffset(0)]
public uint Data1;

// ctor

public PVR2Packet4BPP(in TextureColor128 colorA, in TextureColor128 colorB, uint modData,
                      bool hardTransition, bool useAlpha)
{
ModulationData = modData;
HardTransition = hardTransition;

ColorA = PVR2Base.EncodeColor(colorA, useAlpha, out bool opaqueA);
ColorB = PVR2Base.EncodeColor(colorB, useAlpha, out bool opaqueB);

Opacity = opaqueA && opaqueB;
}

// Modulation Data

public uint ModulationData
{

readonly get => Data1;
set => Data1 = value;

}

// Wheter to use Hard Transition

public bool HardTransition
{

readonly get => BitHelper.Extract(Data0, 31, 1) == 1;
set => Data0 = BitHelper.Insert(Data0, value ? 1u : 0u, 31, 1);
	
}

// Opacity (global)

public bool Opacity
{

readonly get => BitHelper.Extract(Data0, 15, 1) == 1;
set => Data0 = BitHelper.Insert(Data0, value ? 1u : 0u, 15, 1);

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

// Get ColorA

public readonly TextureColor128 GetColorA(bool useAlpha) => PVR2Base.DecodeColor(ColorA, Opacity, useAlpha);

// Get ColorB

public readonly TextureColor128 GetColorB(bool useAlpha) => PVR2Base.DecodeColor(ColorB, Opacity, useAlpha);
}