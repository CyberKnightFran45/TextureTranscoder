using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 4) ]

// Represents a Texture Color. Order must be BGRA for Compatibility with SKiaSharp

public unsafe struct TextureColor
{
[FieldOffset(0)]
public byte Blue;

[FieldOffset(1)]
public byte Green;

[FieldOffset(2)]
public byte Red;

[FieldOffset(3)]
public byte Alpha;

// ctor

public TextureColor(byte r, byte g, byte b, byte a)
{
Red = r;
Green = g;
Blue = b;
Alpha = a;
}

// ctor 2

public TextureColor(byte r, byte g, byte b) : this(r, g, b, 255)
{
}

// ctor 3

public TextureColor(in TextureColor16 c) : this( (byte)c.Red, (byte)c.Green, (byte)c.Blue, (byte)c.Alpha)
{
}

// Convert from Hex String

public static TextureColor FromString(ReadOnlySpan<char> str)
{

if(str.IsEmpty)
return new();

if(str[0] == '#')
str = str[1..]; // Skip '#' at the beginning
	
if(str.Length != 8)
throw new FormatException("Invalid RGBA8 length. Expected 8 characters (#RRGGBBAA).");

using var rOwner = BinaryHelper.FromHex(str);
var rawBytes = rOwner.AsSpan();

return MemoryMarshal.Read<TextureColor>(rawBytes);
}

// Convert to Hex String

public override readonly string ToString() => $"#{Red:x2}{Green:x2}{Blue:x2}{Alpha:x2}";

// Get Sum of two Colors

public static TextureColor operator +(in TextureColor c1, in TextureColor c2)
{
var r = (byte)(c1.Red + c2.Red);
var g = (byte)(c1.Green + c2.Green);
var b = (byte)(c1.Blue + c2.Blue);
var a = (byte)(c1.Alpha + c2.Alpha);

return new(r, g, b, a);
}

// Sum Colors (in place)

public void Sum(in TextureColor c)
{
Red += c.Red;
Green += c.Green;
Blue += c.Blue;
Alpha += c.Alpha;
}

// Get Difference between two Colors

public static TextureColor operator -(in TextureColor c1, in TextureColor c2)
{
var r = (byte)(c2.Red - c1.Red);
var g = (byte)(c2.Green - c1.Green);
var b = (byte)(c2.Blue - c1.Blue);
var a = (byte)(c2.Alpha - c1.Alpha);

return new(r, g, b, a);
}

// Substract Colors (in place)

public void Substract(in TextureColor c)
{
Red -= c.Red;
Green -= c.Green;
Blue -= c.Blue;
Alpha -= c.Alpha;
}

// Multiply by Factor

public static TextureColor operator *(in TextureColor c, byte factor)
{
var r = (byte)(c.Red * factor);
var g = (byte)(c.Green * factor);
var b = (byte)(c.Blue * factor);
var a = (byte)(c.Alpha * factor);

return new(r, g, b, a);
}

// Multiply by Factor (In place)

public void Multiply(byte factor)
{
Red *= factor;
Green *= factor;
Blue *= factor;
Alpha *= factor;
}

// Dot product of two Colors

public static int operator %(in TextureColor c1, in TextureColor c2)
{
byte r = (byte)(c1.Red * c2.Red);
byte g = (byte)(c1.Green * c2.Green);
byte b = (byte)(c1.Blue * c2.Blue);
byte a = (byte)(c1.Alpha * c2.Alpha);

return r + g + b + a;
}

public static implicit operator TextureColor(uint color) => *(TextureColor*)&color;

public static explicit operator uint(TextureColor color) => *(uint*)&color;
}