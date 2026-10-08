using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size = 4) ]

// Represents a Texture Color. Order must be BGRA for Compatibility with SKiaSharp

public unsafe struct TextureColor
{
// Fields

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

// Min Color (Black)

public static readonly TextureColor MinValue = default;

// Max Color (White)

public static readonly TextureColor MaxValue = new(255, 255, 255);

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
byte r = TextureHelper.ColorClamp(c1.Red + c2.Red);
byte g = TextureHelper.ColorClamp(c1.Green + c2.Green);
byte b = TextureHelper.ColorClamp(c1.Blue + c2.Blue);
byte a = TextureHelper.ColorClamp(c1.Alpha + c2.Alpha);

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

// Add factor to a Color

public static TextureColor operator +(in TextureColor c, int factor)
{
byte r = TextureHelper.ColorClamp(c.Red + factor);
byte g = TextureHelper.ColorClamp(c.Green + factor);
byte b = TextureHelper.ColorClamp(c.Blue + factor);
byte a = TextureHelper.ColorClamp(c.Alpha + factor);

return new(r, g, b, a);
}

// Sum factor (in place)

public void Sum(byte factor)
{
Red += factor;
Green += factor;
Blue += factor;
Alpha += factor;
}

// Get Difference between two Colors

public static TextureColor operator -(in TextureColor c1, in TextureColor c2)
{
byte r = TextureHelper.ColorClamp(c2.Red - c1.Red);
byte g = TextureHelper.ColorClamp(c2.Green - c1.Green);
byte b = TextureHelper.ColorClamp(c2.Blue - c1.Blue);
byte a = TextureHelper.ColorClamp(c2.Alpha - c1.Alpha);

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

// Substract factor to a Color

public static TextureColor operator -(in TextureColor c, int factor)
{
byte r = TextureHelper.ColorClamp(c.Red - factor);
byte g = TextureHelper.ColorClamp(c.Green - factor);
byte b = TextureHelper.ColorClamp(c.Blue - factor);
byte a = TextureHelper.ColorClamp(c.Alpha - factor);

return new(r, g, b, a);
}

// Substract factor (in place)

public void Substract(byte factor)
{
Red -= factor;
Green -= factor;
Blue -= factor;
Alpha -= factor;
}

// Multiply Colors

public static TextureColor operator *(in TextureColor c1, in TextureColor c2)
{
byte r = TextureHelper.ColorClamp(c1.Red * c2.Red);
byte g = TextureHelper.ColorClamp(c1.Green * c2.Green);
byte b = TextureHelper.ColorClamp(c1.Blue * c2.Blue);
byte a = TextureHelper.ColorClamp(c1.Alpha * c2.Alpha);

return new(r, g, b, a);
}

// Multiply Colors (In place)

public void Multiply(in TextureColor c)
{
Red *= c.Red;
Green *= c.Green;
Blue *= c.Blue;
Alpha *= c.Alpha;
}

// Multiply by Factor

public static TextureColor operator *(in TextureColor c, int factor)
{
byte r = TextureHelper.ColorClamp(c.Red * factor);
byte g = TextureHelper.ColorClamp(c.Green * factor);
byte b = TextureHelper.ColorClamp(c.Blue * factor);
byte a = TextureHelper.ColorClamp(c.Alpha * factor);

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

// Divide Colors

public static TextureColor operator /(in TextureColor c1, in TextureColor c2)
{
byte r = TextureHelper.ColorClamp(c1.Red / c2.Red);
byte g = TextureHelper.ColorClamp(c1.Green / c2.Green);
byte b = TextureHelper.ColorClamp(c1.Blue / c2.Blue);
byte a = TextureHelper.ColorClamp(c1.Alpha / c2.Alpha);

return new(r, g, b, a);
}

// Divide Colors (In place)

public void Divide(in TextureColor c)
{
Red /= c.Red;
Green /= c.Green;
Blue /= c.Blue;
Alpha /= c.Alpha;
}

// Divide by Factor

public static TextureColor operator /(in TextureColor c, int factor)
{
byte r = TextureHelper.ColorClamp(c.Red / factor);
byte g = TextureHelper.ColorClamp(c.Green / factor);
byte b = TextureHelper.ColorClamp(c.Blue / factor);
byte a = TextureHelper.ColorClamp(c.Alpha / factor);

return new(r, g, b, a);
}

// Divide by Factor (In place)

public void Divide(byte factor)
{
Red /= factor;
Green /= factor;
Blue /= factor;
Alpha /= factor;
}

// Dot product of two Colors

public static int operator %(in TextureColor c1, in TextureColor c2)
{
byte r = TextureHelper.ColorClamp(c1.Red * c2.Red);
byte g = TextureHelper.ColorClamp(c1.Green * c2.Green);
byte b = TextureHelper.ColorClamp(c1.Blue * c2.Blue);
byte a = TextureHelper.ColorClamp(c1.Alpha * c2.Alpha);

return r + g + b + a;
}

public static implicit operator TextureColor(uint color) => *(TextureColor*)&color;

public static explicit operator uint(TextureColor color) => *(uint*)&color;
}