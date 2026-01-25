using static ETCBase;
using static ETC2Base;

using System;

// Encodes images with ETC2

public static class ETC2Encoder
{
// ETC1/Individual mode encoder

private static ETC2BlockData EncodeETC1Mode(ReadOnlySpan<TextureColor> block, bool punchThrough)
{
Span<TextureColor> a = stackalloc TextureColor[8];
Span<TextureColor> b = stackalloc TextureColor[8];

var bestError = int.MaxValue;
ulong data = 0;

for(int flip = 0; flip < 2; flip++)
{

if(flip == 0)
{
GetLeftColors(block, a);
GetRightColors(block, b);
}

else
{
GetTopColors(block, a);
GetBottomColors(block, b);
}

int mod1 = FindBestModifier(a, out var baseColor1, out int error1);
int mod2 = FindBestModifier(b, out var baseColor2, out int error2);

data = BitHelper.Insert(data, (ulong)flip, 32, 1);
data = BitHelper.Insert(data, 0uL, 33, 1);
data = BitHelper.Insert(data, (ulong)mod1, 37, 3);
data = BitHelper.Insert(data, (ulong)mod2, 34, 3);

EncodeBaseColors(ref data, baseColor1, baseColor2, false, punchThrough);

int x1 = flip == 0 ? 0 : 1;
EncodePixelIndices(ref data, a, baseColor1, mod1, x1, 0);

int x2 = flip == 0 ? 2 : 3;
EncodePixelIndices(ref data, b, baseColor2, mod2, x2, 0);

int curError = error1 + error2;

if(curError < bestError)
bestError = curError;

}

return new(data, ETC2Mode.Individual, bestError);
}

// T Mode encoder

private static ETC2BlockData EncodeTMode(ReadOnlySpan<TextureColor> block)
{
ulong data = BitHelper.Insert(0, 6uL, 60, 4);

Span<TextureColor> left = stackalloc TextureColor[8];
GetLeftColors(block, left);

Span<TextureColor> right = stackalloc TextureColor[8];
GetRightColors(block, right);

_ = FindBestModifier(left, out var baseColor1, out int error1);
int mod = FindBestModifier(right, out var baseColor2, out int error2);

EncodeBaseColors(ref data, baseColor1, baseColor2, false, false);
data = BitHelper.Insert(data, (ulong)mod, 37, 3);

Span<TextureColor> palette = stackalloc TextureColor[4];
GenerateSubBlockColors(baseColor2, mod, palette);

for(int i = 0; i < 16; i++)
{
int best = 0, bestError = int.MaxValue;

for(int p = 0; p < 4; p++)
{
int curError = TextureHelper.ColorDistanceL1(palette[p], block[i], false);

if(curError < bestError)
{
bestError = curError;
best = p;
}

}

if( (best & 1) != 0)
data |= 1uL << i;

if( (best & 2) != 0)
data |= 1uL << (i + 16);
		
}

return new(data, ETC2Mode.T, error1 + error2);
}


// H Mode encoder

private static ETC2BlockData EncodeHMode(ReadOnlySpan<TextureColor> block)
{
ulong blockData = 0;
blockData = BitHelper.Insert(blockData, 5uL, 60, 4);

Span<TextureColor> top = stackalloc TextureColor[8];
GetTopColors(block, top);

Span<TextureColor> bottom = stackalloc TextureColor[8]; 
GetBottomColors(block, bottom);

int modifier = FindBestModifier(top, out var baseColor1, out int error1);
_ = FindBestModifier(bottom, out var baseColor2, out int error2);

EncodeBaseColors(ref blockData, baseColor1, baseColor2, false, false);
blockData = BitHelper.Insert(blockData, (ulong)modifier, 34, 3);

int modLow = ETCBase.MOD_TABLE[modifier, 0];
int modHigh = ETCBase.MOD_TABLE[modifier, 1];

Span<TextureColor> palette = stackalloc TextureColor[4];

int r1 = baseColor1.Red;
int g1 = baseColor1.Green;
int b1 = baseColor1.Blue;

int r2 = baseColor2.Red;
int g2 = baseColor2.Green;
int b2 = baseColor2.Blue;

palette[0] = TextureHelper.AddColorBias(r1, g1, b1, modHigh);
palette[1] = TextureHelper.AddColorBias(r1, g1, b1, -modHigh);
palette[2] = TextureHelper.AddColorBias(r2, g2, b2, modHigh);
palette[3] = TextureHelper.AddColorBias(r2, g2, b2, -modHigh);

for(int i = 0; i < 16; i++)
{
int best = 0, bestError = int.MaxValue;

for(int p = 0; p < 4; p++)
{
int curError = TextureHelper.ColorDistanceL1(palette[p], block[i], false);

if(curError < bestError)
{
bestError = curError;
best = p;
}

}

if( (best & 1) != 0)
blockData |= 1uL << i;

if( (best & 2) != 0)
blockData |= 1uL << (i + 16);

}

return new(blockData, ETC2Mode.H, error1 + error2);
}

// Planar Mode encoder
	
private static ETC2BlockData EncodePlanarMode(ReadOnlySpan<TextureColor> block)
{
TextureColor cO = block[0];  // Top-left
TextureColor cH = block[3];  // Top-right
TextureColor cV = block[12]; // Bottom-left

ulong blockData = EncodePlanarColors(cO, cH, cV);

Span<TextureColor> decoded = stackalloc TextureColor[16];
ETC2Decoder.DecodePlanarMode(blockData, decoded);

int error = TextureHelper.BlockDistanceL1(block, decoded, 4, 4, false);

return new(blockData, ETC2Mode.Planar, error);
}

// Encode RGB Block

public static ulong EncodeBlock(ReadOnlySpan<TextureColor> block)
{

Span<ETC2BlockData> candidates =
[
	EncodeETC1Mode(block, false),
	EncodeTMode(block),
	EncodeHMode(block),
	EncodePlanarMode(block),
];

int bestIndex = 0;

for(int i = 1; i < candidates.Length; i++)
{

if(candidates[i].Error < candidates[bestIndex].Error)
bestIndex = i;

}

return candidates[bestIndex].ColorBits;
}

}