using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 16) ]

// Represents a 128-bits TextureColor in RGBA order.

public unsafe struct TextureColor16
{
// Fields

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

// Min Color (Black)

public static readonly TextureColor16 MinValue = default;

// Max Color (White)

public static readonly TextureColor16 MaxValue = new(255, 255, 255);

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

// Add factor to a Color

public static TextureColor16 operator +(in TextureColor16 c, int factor)
{
int r = c.Red + factor;
int g = c.Green + factor;
int b = c.Blue + factor;
int a = c.Alpha + factor;

return new(r, g, b, a);
}

// Sum factor (in place)

public void Sum(int factor)
{
Red += factor;
Green += factor;
Blue += factor;
Alpha += factor;
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

// Substract factor to a Color

public static TextureColor16 operator -(in TextureColor16 c, int factor)
{
int r = c.Red - factor;
int g = c.Green - factor;
int b = c.Blue - factor;
int a = c.Alpha - factor;

return new(r, g, b, a);
}

// Substract factor (in place)

public void Substract(int factor)
{
Red -= factor;
Green -= factor;
Blue -= factor;
Alpha -= factor;
}

// Multiply Colors

public static TextureColor16 operator *(in TextureColor16 c1, in TextureColor16 c2)
{
int r = c1.Red * c2.Red;
int g = c1.Green * c2.Green;
int b = c1.Blue * c2.Blue;
int a = c1.Alpha * c2.Alpha;

return new(r, g, b, a);
}

// Multiply Colors (In place)

public void Multiply(in TextureColor16 c)
{
Red *= c.Red;
Green *= c.Green;
Blue *= c.Blue;
Alpha *= c.Alpha;
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

// Divide Colors

public static TextureColor16 operator /(in TextureColor16 c1, in TextureColor16 c2)
{
int r = c1.Red / c2.Red;
int g = c1.Green / c2.Green;
int b = c1.Blue / c2.Blue;
int a = c1.Alpha / c2.Alpha;

return new(r, g, b, a);
}

// Divide Colors (In place)

public void Divide(in TextureColor16 c)
{
Red /= c.Red;
Green /= c.Green;
Blue /= c.Blue;
Alpha /= c.Alpha;
}

// Divide by Factor

public static TextureColor16 operator /(in TextureColor16 c, int factor)
{
int r = c.Red / factor;
int g = c.Green / factor;
int b = c.Blue / factor;
int a = c.Alpha / factor;

return new(r, g, b, a);
}

// Divide by Factor (In place)

public void Divide(int factor)
{
Red /= factor;
Green /= factor;
Blue /= factor;
Alpha /= factor;
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