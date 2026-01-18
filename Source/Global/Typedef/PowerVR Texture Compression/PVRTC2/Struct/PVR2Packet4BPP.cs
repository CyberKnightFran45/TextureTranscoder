using System.Runtime.InteropServices;

// Represents a PVR2 Packet (4bpp mode)

[StructLayout(LayoutKind.Explicit, Size = 8)]

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

SetColorA(colorA, useAlpha, out bool opaqueA);
SetColorB(colorB, useAlpha, out bool opaqueB);

Opacity = opaqueA && opaqueB;
}

// Modulation Data

public uint ModulationData
{

readonly get => (uint)BitHelper.Extract(Data, 0, 60);
set => Data = BitHelper.Insert(Data, value, 0, 60);

}

// Wheter to use Modulated Interpolation

public bool InterpolatedModulation
{

readonly get => BitHelper.Extract(Data, 61, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 61, 1);

}

// Wheter to use Hard Transition or not

public bool HardTransition
{

readonly get => BitHelper.Extract(Data, 62, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 62, 1);

}

// Opacity (global)

public bool Opacity
{

readonly get =>	BitHelper.Extract(Data, 63, 1) == 1;
set => Data = BitHelper.Insert(Data, value ? 1uL : 0uL, 63, 1);
}

// ColorA getter

private readonly int ColorA_get(bool opacity)
{
var shift = opacity ? 41 : 44;
var bits = opacity ? 15 : 16;

return (int)BitHelper.Extract(Data, shift, bits);
}

// ColorA setter

private void ColorA_set(bool opacity, int val)
{
var shift = opacity ? 41 : 44;
var bits = opacity ? 15 : 16;

Data = BitHelper.Insert(Data, (ulong)val, shift, bits);
}

// Color A

private int ColorA
{

readonly get => ColorA_get(Opacity);
set => ColorA_set(Opacity, value);

}

// ColorB getter

private readonly int ColorB_get(bool opacity)
{
var shift = opacity ? 26 : 28;
var bits = opacity ? 15 : 16;

return (int)BitHelper.Extract(Data, shift, bits);
}

// ColorB setter

private void ColorB_set(bool opacity, int val)
{
var shift = opacity ? 26 : 28;
var bits = opacity ? 15 : 16;

Data = BitHelper.Insert(Data, (ulong)val, shift, bits);
}

// Color B

private int ColorB
{

readonly get => ColorB_get(Opacity);
set => ColorB_set(Opacity, value);

}

// Get ColorA

public readonly TextureColor16 GetColorA(bool useAlpha) => PVR2Base.DecodeColor(ColorA, Opacity, useAlpha);

// Get ColorB

public readonly TextureColor16 GetColorB(bool useAlpha) => PVR2Base.DecodeColor(ColorB, Opacity, useAlpha);

// Set ColorA

public void SetColorA(in TextureColor16 color, bool useAlpha, out bool opaque)
{
ColorA = PVR2Base.EncodeColor(color, useAlpha, out opaque);
}

// Set ColorB

public void SetColorB(in TextureColor16 color, bool useAlpha, out bool opaque)
{
ColorB = PVR2Base.EncodeColor(color, useAlpha, out opaque);
}

}