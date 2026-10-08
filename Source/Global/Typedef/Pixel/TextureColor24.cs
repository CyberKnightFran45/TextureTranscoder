using System;
using System.Runtime.InteropServices;

/// <summary> Represents a 24-bits TextureColor in <b>RGB</b> order </summary>

[StructLayout(LayoutKind.Explicit, Size = 3) ]

public struct TextureColor24
{
// Fields

[FieldOffset(0)]
public byte Red;

[FieldOffset(1)]
public byte Green;

[FieldOffset(2)]
public byte Blue;

// ctor

public TextureColor24(byte r, byte g, byte b)
{
Red = r;
Green = g;
Blue = b;
}

// ctor 2

public TextureColor24(in TextureColor c) : this(c.Red, c.Green, c.Blue)
{
}

// Min Color (Black)

public static readonly TextureColor24 MinValue = default;

// Max Color (White)

public static readonly TextureColor24 MaxValue = new(255, 255, 255);

// Convert from Hex String

public static TextureColor24 FromString(ReadOnlySpan<char> str)
{

if(str.IsEmpty)
return new();

if(str[0] == '#')
str = str[1..]; // Skip '#' at the beginning
	
if(str.Length != 6)
throw new FormatException("Invalid RGBA6 length. Expected 6 characters (#RRGGBB)");

using var rOwner = BinaryHelper.FromHex(str);
var rawBytes = rOwner.GetView();

return MemoryMarshal.Read<TextureColor24>(rawBytes);
}

// Convert to Hex String

public override readonly string ToString() => $"#{Red:x2}{Green:x2}{Blue:x2}";
}