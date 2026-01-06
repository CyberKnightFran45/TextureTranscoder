using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 8)]

// Represents a PVR Word

public struct PVRWord
{
// Modulation Data

[FieldOffset(0)]
public uint ModulationData;

// Color flags

[FieldOffset(4)]
public uint Flags;

// ctor

public PVRWord(uint mod, uint flags)
{
ModulationData = mod;
Flags = flags;
}

}