using System;
using System.Numerics;
using SkiaSharp;

/// <summary> Initializes some useful Tasks for Textures. </summary>

public static class TextureHelper
{
#region =========================  DIMENSIONS  ========================

// Calculate Texture Size

public static int ComputeSize(int width, int height, int factor = 0) => (width * height) << factor;

// Get Padded Width
	
public static int Pad(int x, int factor) => x % factor != 0 ? x / factor * factor + factor : x;

// Get Padded Dim

public static int GetBlockDim(int dim, int blockSize) => (dim + blockSize - 1) / blockSize;

// Adjust Size to POT (Power of two)

public static bool AdjustSize(ref int width, ref int height)
{
int oldWidth = width;
int oldHeight = height;

width = (int)BitOperations.RoundUpToPowerOf2( (uint)width);
height = (int)BitOperations.RoundUpToPowerOf2( (uint)height);

return width != oldWidth || height != oldHeight;
}

// Delegate used for Resizing Dimensions

public delegate bool ResizePredicate(ref int width, ref int height);

// Resize Image if Applies

public static void ResizeImage(ref SKBitmap image, ResizePredicate resizeCheck = null)
{
resizeCheck ??= AdjustSize;

int width = image.Width;
int height = image.Height;

bool shouldResize = resizeCheck(ref width, ref height);

if(shouldResize)
{
SKBitmap resizedImage = new(width, height);

image.MoveTo(resizedImage, 0, 0);
image.Dispose();

image = resizedImage;
}

}

// Get SubBlock from Tile

public static void ExtractSubBlock(ReadOnlySpan<TextureColor> pixels, Span<TextureColor> result,
                                   int startX, int startY, int width, int height,
                                   int tileSize)
{

for(int row = 0; row < height; row++)
{

for(int col = 0; col < width; col++)
result[row * width + col] = pixels[(startY + row) * tileSize + startX + col];
  
}

}

// Calculate Midpoint

public static byte Midpoint(byte a, byte b) => (byte)( (a + b) / 2);

// Calculate Pixel Offset

public static int GetPxOffset(int blockX, int blockY, int x, int y,
                              int width, int blockWidth, int blockHeight)
{
int startX = blockX * blockWidth;
int startY = blockY * blockHeight;

return (startY + y) * width + startX + x;
}

#endregion


#region =========================  COLOR  ========================

// Ensure color is inside bounds

public static byte ColorClamp(int color) => (byte)Math.Clamp(color, 0, 255);
	
// ColorClamp for floats

public static byte ColorClamp(float color) => ColorClamp( (int)color);

// Check range

private static bool IsInsideRange(int v, int min, int max) => v > min && v < max;

// Check if Color is inside Range

public static bool IsColorInsideRange(int r, int g, int b, int min, int max)
{
	
return IsInsideRange(r, min, max) &&
       IsInsideRange(g, min, max) &&
       IsInsideRange(b, min, max);

}

// Apply modifier to Colors

public static TextureColor AddColorBias(int r, int g, int b, int delta)
{
byte dR = ColorClamp(r + delta);
byte dG = ColorClamp(g + delta);
byte dB = ColorClamp(b + delta);

return new(dR, dG, dB);
}

// Linear interpolation (8-bits)

private static byte Lerp8(byte c1, byte c2, int w1, int w2) => (byte)Lerp32(c1, c2, w1, w2);

// Linear interpolation (32-bits)

public static int Lerp32(int c1, int c2, int w1, int w2) => (c1 * w1 + c2 * w2) / (w1 + w2);

// Interpolate Colors with Weights (Linear)

public static TextureColor LerpColors(in TextureColor c1, in TextureColor c2, int w1, int w2,
                                      bool useAlpha)
{
var r = Lerp8(c1.Red, c2.Red, w1, w2);
var g = Lerp8(c1.Green, c2.Green, w1, w2);
var b = Lerp8(c1.Blue, c2.Blue, w1, w2);
var a = useAlpha ? Lerp8(c1.Alpha, c2.Alpha, w1, w2) : (byte)255;

return new(r, g, b, a);
}

// Lerp Colors (128-bits)

public static TextureColor16 LerpColors(in TextureColor16 c1, in TextureColor16 c2, int w1, int w2,
                                        bool useAlpha)
{
var r = Lerp32(c1.Red, c2.Red, w1, w2);
var g = Lerp32(c1.Green, c2.Green, w1, w2);
var b = Lerp32(c1.Blue, c2.Blue, w1, w2);
var a = useAlpha ? Lerp32(c1.Alpha, c2.Alpha, w1, w2) : 255;

return new(r, g, b, a);
}

// Get Channel Step

public static int GetChannelStep(int src, int dst, int step) => (dst - src) / step;

// Get Color Mean

public static int GetColorMean(int r, int g, int b) => (r + g + b) / 3;

// Get DiffMean between colors

public static int GetColorRangeMean(in TextureColor min, in TextureColor max)
{
int rDiff = max.Red - min.Red;
int gDiff = max.Green - min.Green;
int bDiff = max.Blue - min.Blue;

return GetColorMean(rDiff, gDiff, bDiff);
}

// Calculate color distance (Linear)
	
public static int ColorDistanceL1(in TextureColor c1, in TextureColor c2, bool useAlpha)
{
int rDiff = Math.Abs(c2.Red - c1.Red);
int gDiff = Math.Abs(c2.Green - c1.Green);
int bDiff = Math.Abs(c2.Blue - c1.Blue);
int aDiff = useAlpha ? Math.Abs(c2.Alpha - c1.Alpha) : 0;

return rDiff + gDiff + bDiff + aDiff;
}

// Color distance (128-bits)
	
public static int ColorDistanceL1(in TextureColor c1, in TextureColor16 c2, bool useAlpha)
{
int rDiff = Math.Abs(c2.Red - c1.Red);
int gDiff = Math.Abs(c2.Green - c1.Green);
int bDiff = Math.Abs(c2.Blue - c1.Blue);
int aDiff = useAlpha ? Math.Abs(c2.Alpha - c1.Alpha) : 0;

return rDiff + gDiff + bDiff + aDiff;
}

// Block distance between colors (Linear)

public static int BlockDistanceL1(ReadOnlySpan<TextureColor> plain,
                                  ReadOnlySpan<TextureColor> encoded,
                                  bool useAlpha)
{
int distance = 0;

for(int i = 0; i < 16; i++)
distance += ColorDistanceL1(plain[i], encoded[i], useAlpha);

return distance;
}

#endregion
}