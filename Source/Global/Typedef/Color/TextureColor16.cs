using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 16) ]

// Represents a 128-bits TextureColor. Order must be BGRA for Compatibility with SKiaSharp

public unsafe struct TextureColor16
{
// Default Color

public static readonly TextureColor16 Default = new(0, 0, 0, 0);

[FieldOffset(0)]
public int Red;

[FieldOffset(4)]
public int Green;

[FieldOffset(8)]
public int Blue;

[FieldOffset(12)]
public int Alpha;

// ctor

public TextureColor16(int r, int g, int b, int a)
{
Red = r;
Green = g;
Blue = b;
Alpha = a;
}

// ctor 2

public TextureColor16(int r, int g, int b) : this(r, g, b, 255)
{
}

// ctor 3

public TextureColor16(in TextureColor c) : this(c.Red, c.Green, c.Blue, c.Alpha)
{
}

// Convert from Hex String

public static TextureColor16 FromString(ReadOnlySpan<char> str)
{

if(str.IsEmpty)
return new();

if(str[0] == '#')
str = str[1..]; // Skip '#' at the beginning
	
if(str.Length != 32)
throw new FormatException("Invalid RGBA16 length. Expected Length: 32-chars");

using var rOwner = BinaryHelper.FromHex(str);
var rawBytes = rOwner.AsSpan();

return MemoryMarshal.Read<TextureColor16>(rawBytes);
}

// Convert to Hex String

public override readonly string ToString() => $"#{Red:x8}{Green:x8}{Blue:x8}{Alpha:x8}";

// Get Sum of two Colors

public static TextureColor16 operator +(in TextureColor16 c1, in TextureColor16 c2)
{
int r = c1.Red + c2.Red;
int g = c1.Green + c2.Green;
int b = c1.Blue + c2.Blue;
int a = c1.Alpha + c2.Alpha;

return new(r, g, b, a);
}

// Sum Colors (in place)

public void Sum(in TextureColor16 c)
{
Red += c.Red;
Green += c.Green;
Blue += c.Blue;
Alpha += c.Alpha;
}

// Get Difference between two Colors

public static TextureColor16 operator -(in TextureColor16 c1, in TextureColor16 c2)
{
int r = c2.Red - c1.Red;
int g = c2.Green - c1.Green;
int b = c2.Blue - c1.Blue;
int a = c2.Alpha - c1.Alpha;

return new(r, g, b, a);
}

// Substract Colors (in place)

public void Substract(in TextureColor16 c)
{
Red -= c.Red;
Green -= c.Green;
Blue -= c.Blue;
Alpha -= c.Alpha;
}

// Multiply by Factor

public static TextureColor16 operator *(in TextureColor16 c, int factor)
{
int r = c.Red * factor;
int g = c.Green * factor;
int b = c.Blue * factor;
int a = c.Alpha * factor;

return new(r, g, b, a);
}

// Multiply by Factor (In place)

public void Multiply(int factor)
{
Red *= factor;
Green *= factor;
Blue *= factor;
Alpha *= factor;
}

// Multiply by Factor

public static TextureColor16 operator /(in TextureColor16 c, int factor)
{
int r = c.Red / factor;
int g = c.Green / factor;
int b = c.Blue / factor;
int a = c.Alpha / factor;

return new(r, g, b, a);
}

// Dot product of two Colors

public static int operator %(in TextureColor16 c1, in TextureColor16 c2)
{
int r = c1.Red * c2.Red;
int g = c1.Green * c2.Green;
int b = c1.Blue * c2.Blue;
int a = c1.Alpha * c2.Alpha;

return r + g + b + a;
}

public static implicit operator TextureColor16(Int128 color) => *(TextureColor16*)&color;

public static explicit operator Int128(TextureColor16 color) => *(Int128*)&color;
}