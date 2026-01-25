// Helper used for manipulating bits, mostly for unpacking Colors

public static class BitHelper
{
#region ==========  BIT FIELDS  ==========

// Extract bit from Int32

public static int Extract(int val, int shift, int bits)
{
int mask = (1 << bits) - 1;

return (val >> shift) & mask;
}

// Extract bit from UInt32

public static int Extract(uint val, int shift, int bits) => Extract( (int)val, shift, bits);

// Extract bit from UInt64

public static ulong Extract(ulong val, int shift, int bits)
{
ulong mask = (1uL << bits) - 1;

return (val >> shift) & mask;
}

// Insert field to Int32

public static int Insert(int val, int field, int shift, int bits)
{
int mask = ( (1 << bits) - 1) << shift;

return (val & ~mask) | ( (field << shift) & mask);
}

// Insert field to UInt32

public static uint Insert(uint val, uint field, int shift, int bits)
{
uint mask = ( (1u << bits) - 1) << shift;

return (val & ~mask) | ( (field << shift) & mask);
}

// Insert field to UInt64

public static ulong Insert(ulong val, ulong field, int shift, int bits)
{
ulong mask = ( (1uL << bits) - 1) << shift;

return (val & ~mask) | ( (field << shift) & mask);
}

#endregion


#region ==========  BIT EXPANDER  ==========

// Expand from N-bits to 8-bits

public static byte ExpandTo8(int val, int bits)
{

if(bits == 8)
return (byte)val;

int max = (1 << bits) - 1;

return (byte)( (val * 255 + (max >> 1) ) / max);
}

// Expand to 8-bits (ULong)

public static byte ExpandTo8(ulong val, int bits)
{

if(bits == 8)
return (byte)val;

ulong max = (1uL << bits) - 1;

return (byte)( (val * 255 + (max >> 1) ) / max);
}

// Quantize 8-bit value to N bits

public static int QuantizeFrom8(int val, int bits)
{

if(bits == 8)
return val;

int max = (1 << bits) - 1;

return (val * max + 127) / 255;
}

// Quantize 8-bit value to N bits

public static int QuantizeFrom8(byte val, int bits) => QuantizeFrom8( (int)val, bits);

// Expand channel

public static int ExpandChannel(byte val, int bits)
{
int q = QuantizeFrom8(val, bits);

return ExpandTo8(q, bits);
}

// Extract field and expand to 8-bits

public static byte ExtractAndExpandTo8(int val, int shift, int bits)
{
int field = Extract(val, shift, bits);

return ExpandTo8(field, bits);
}

// Extract and expand (Uint)

public static byte ExtractAndExpandTo8(uint val, int shift, int bits)
{
int field = Extract(val, shift, bits);

return ExpandTo8(field, bits);
}

// Extract and expand (ULong)

public static byte ExtractAndExpandTo8(ulong val, int shift, int bits)
{
ulong field = Extract(val, shift, bits);

return ExpandTo8(field, bits);
}

#endregion
}