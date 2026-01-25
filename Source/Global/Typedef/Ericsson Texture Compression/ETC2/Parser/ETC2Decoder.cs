using static ETCBase;
using static ETC2Base;

using System;

// Decodes ETC2 images

public static class ETC2Decoder
{
// Get Palette Index

private static int ComputePIndex(ulong blockData, int i)
{
var lsb = (int)( (blockData >> i) & 1);
var msb = (int)( (blockData >> (i + 16) ) & 1);

return (msb << 1) | lsb;
}

// Decode ETC1/Individual mode
	
private static void DecodeETC1Mode(ulong blockData, Span<TextureColor> output, bool punchThrough)
{
bool flip = BitHelper.Extract(blockData, 32, 1) == 1;
bool diff = BitHelper.Extract(blockData, 33, 1) == 1;

var table1 = (int)BitHelper.Extract(blockData, 37, 3);
var table2 = (int)BitHelper.Extract(blockData, 34, 3);

DecodeBaseColors(blockData, diff, out var baseColor1, out var baseColor2);

Span<TextureColor> colors1 = stackalloc TextureColor[4];
Span<TextureColor> colors2 = stackalloc TextureColor[4];
  
GenerateSubBlockColors(baseColor1, table1, colors1);
GenerateSubBlockColors(baseColor2, table2, colors2);

for(int y = 0; y < 4; y++)

for (int x = 0; x < 4; x++)
{
int i = y * 4 + x;
int index = ComputePIndex(blockData, i);

bool useFirst = flip ? (y < 2) : (x < 2);
output[i] = useFirst ? colors1[index] : colors2[index];

if(punchThrough && index == 2)
output[i] = default;

}

}

// Decode T mode

private static void DecodeTMode(ulong blockData, Span<TextureColor> output)
{
byte r1 = BitHelper.ExtractAndExpandTo8(blockData, 59, 4);
byte g1 = BitHelper.ExtractAndExpandTo8(blockData, 51, 4);
byte b1 = BitHelper.ExtractAndExpandTo8(blockData, 43, 4);

byte r2 = BitHelper.ExtractAndExpandTo8(blockData, 55, 4);
byte g2 = BitHelper.ExtractAndExpandTo8(blockData, 47, 4);
byte b2 = BitHelper.ExtractAndExpandTo8(blockData, 39, 4);

var table = (int)BitHelper.Extract(blockData, 37, 3);
int dist = MOD_TABLE[table, 0];

Span<TextureColor> palette =
[
	new(r1, g1, b1),
	TextureHelper.AddColorBias(r2, g2, b2, dist),
	TextureHelper.AddColorBias(r2, g2, b2, -dist),
	new(r2, g2, b2)
];

for (int i = 0; i < 16; i++)
{
int index = ComputePIndex(blockData, i);

output[i] = palette[index];
}

}

// Decode H mode

private static void DecodeHMode(ulong blockData, Span<TextureColor> output)
{
byte r1 = BitHelper.ExtractAndExpandTo8(blockData, 59, 4);
byte g1 = BitHelper.ExtractAndExpandTo8(blockData, 51, 4);
byte b1 = BitHelper.ExtractAndExpandTo8(blockData, 43, 4);

byte r2 = BitHelper.ExtractAndExpandTo8(blockData, 56, 3);
byte g2 = BitHelper.ExtractAndExpandTo8(blockData, 48, 3);
byte b2 = BitHelper.ExtractAndExpandTo8(blockData, 40, 3);

var table = (int)BitHelper.Extract(blockData, 34, 3);
int dist = MOD_TABLE[table, 0];

Span<TextureColor> palette =
[
	TextureHelper.AddColorBias(r1, g1, b1, dist),
	TextureHelper.AddColorBias(r1, g1, b1, -dist),
	TextureHelper.AddColorBias(r2, g2, b2, dist),
	TextureHelper.AddColorBias(r2, g2, b2, -dist)
];

for (int i = 0; i < 16; i++)
{
int index = ComputePIndex(blockData, i);

output[i] = palette[index];
}

}

// Interpolate Color for Planar (bilinear)

private static byte InterpolateColor(int o, int h, int v, int x, int y)
{
int dO = o * (4 - x) * (4 - y);
int dH = h * x * (4 - y);
int dV = v * (4 - x) * y;

int delta = (dO + dH + dV) / 16;

return TextureHelper.ColorClamp(delta);
}

// Decode planar mode
	
internal static void DecodePlanarMode(ulong blockData, Span<TextureColor> output)
{
var rO = (int)BitHelper.Extract(blockData, 54, 6) << 2;
var gO = (int)BitHelper.Extract(blockData, 48, 6) << 2;
var bO = (int)BitHelper.Extract(blockData, 42, 6) << 2;

var rH = (int)BitHelper.Extract(blockData, 36, 6) << 2;
var gH = (int)BitHelper.Extract(blockData, 30, 6) << 2;
var bH = (int)BitHelper.Extract(blockData, 24, 6) << 2;

var rV = (int)BitHelper.Extract(blockData, 18, 6) << 2;
var gV = (int)BitHelper.Extract(blockData, 12, 6) << 2;
var bV = (int)BitHelper.Extract(blockData, 6, 6) << 2;

for(int y = 0; y < 4; y++)

for(int x = 0; x < 4; x++)
{
int index = y * 4 + x;

byte r = InterpolateColor(rO, rH, rV, x, y);
byte g = InterpolateColor(gO, gH, gV, x, y);
byte b = InterpolateColor(bO, bH, bV, x, y);

output[index] = new(r, g, b);
}

}

// Decode block

public static void DecodeBlock(ulong blockData, Span<TextureColor> output)
{
var mode = (int)BitHelper.Extract(blockData, 60, 4);

switch(mode)
{
case 2:
DecodePlanarMode(blockData, output);
break;

case 6:
DecodeTMode(blockData, output);
break;

case 7:
DecodeHMode(blockData, output);
break;

default:
DecodeETC1Mode(blockData, output, false);
break;
}

}

}