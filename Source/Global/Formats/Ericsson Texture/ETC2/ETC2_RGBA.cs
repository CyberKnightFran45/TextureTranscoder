using System;
using System.IO;
using SkiaSharp;

// ETC2 RGBA compression (with alpha channel)

public static class ETC2_RGBA
{
// Alpha modifier table for EAC

private static readonly int[] ALPHA_MOD_TABLE =
[
    3,   6,  11,  16,  23,  32,  41,  64,
   -3,  -6, -11, -16, -23, -32, -41, -64
];

#region ======================= ENCODER =======================

// Alpha encoder for EAC (Ericsson Alpha Compression)

private static ulong EncodeAlpha(ReadOnlySpan<TextureColor> block)
{
byte minAlpha = 255, maxAlpha = 0;

for(int i = 0; i < 16; i++)
{
byte a = block[i].Alpha;

if(a < minAlpha)
minAlpha = a;

if(a > maxAlpha)
maxAlpha = a;

}

byte baseAlpha = minAlpha;
int range = maxAlpha - minAlpha;

int multiplier = Math.Clamp( (range + 4) / 8, 1, 15);

ulong alphaData = 0;

alphaData = BitHelper.Insert(alphaData, baseAlpha, 56, 8);
alphaData = BitHelper.Insert(alphaData, (ulong)multiplier, 52, 4);

for(int i = 0; i < 16; i++)
{
int alphaDiff = block[i].Alpha - baseAlpha;

int bestIndex = 0;
int bestError = int.MaxValue;

for(int idx = 0; idx < 16; idx++)
{
int modVal = ALPHA_MOD_TABLE[idx] * multiplier;
int curError = Math.Abs(alphaDiff - modVal);

if(curError < bestError)
{
bestError = curError;
bestIndex = idx;
}
 
}

alphaData = BitHelper.Insert(alphaData, (ulong)bestIndex, 45 - (i * 3), 3);
}

return alphaData;
}

// Write pixels to Bitmap

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• ETC2-RGBA Texture Encode:");
TraceLogger.WriteLine();

return ETCBase.Encode(writer, image, ETC2Encoder.EncodeBlock, EncodeAlpha);
}

#endregion


#region ======================= DECODER =======================

// Decode alpha from EAC

private static void DecodeAlpha(ulong encoded, Span<byte> plain)
{
var baseAlpha = (int)BitHelper.Extract(encoded, 56, 8);
var multiplier = (int)BitHelper.Extract(encoded, 52, 4);
 
if(multiplier == 0)
multiplier = 1;

for(int i = 0; i < 16; i++)
{
var modifierIndex = (int)BitHelper.Extract(encoded, 45 - (i * 3), 3);
int modifier = ALPHA_MOD_TABLE[modifierIndex];

int alpha = baseAlpha + modifier * multiplier;

if(alpha < 0)
alpha = 0;

if(alpha > 255)
alpha = 255;

plain[i] = (byte)alpha;
}

}

// Read bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• ETC2-RGBA Texture Decode:");
TraceLogger.WriteLine();

return ETCBase.Decode(reader, width, height, ETC2Decoder.DecodeBlock, DecodeAlpha);
}

#endregion
}