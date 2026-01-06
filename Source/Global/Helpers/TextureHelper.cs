using System;
using System.Numerics;
using SkiaSharp;

/// <summary> Initializes some useful Tasks for Textures. </summary>

public static class TextureHelper
{
// Calculate Texture Size

public static int ComputeSize(int width, int height, int factor = 0) => (width * height) << factor;

// Get Padded Width
	
public static int Pad(int x, int factor) => x % factor != 0 ? x / factor * factor + factor : x;

// Get Padded Dim

public static int GetBlockDim(int dim, int blockSize) => (dim + blockSize - 1) / blockSize;

// Ensure color is inside bounds

public static byte ColorClamp(int color) => (byte)Math.Clamp(color, 0, 255);
	
// ColorClamp for floats

public static byte ColorClamp(float color) => ColorClamp( (int)color);

// Adjust Size to Power of two

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

// Interpolate colors

private static byte Interpolate(byte c1, byte c2, int w1, int w2, int norm) => (byte)( (c1 * w1 + c2 * w2) / norm);

// Interpolate Colors with Weights

public static TextureColor InterpolateColors(in TextureColor c1, in TextureColor c2, int weight1, int weight2,
                                             bool useAlpha)
{
int norm = weight1 + weight2;

var r = Interpolate(c1.Red, c2.Red, weight1, weight2, norm);
var g = Interpolate(c1.Green, c2.Green, weight1, weight2, norm);
var b = Interpolate(c1.Blue, c2.Blue, weight1, weight2, norm);
var a = useAlpha ? Interpolate(c1.Alpha, c2.Alpha, weight1, weight2, norm) : (byte)255;

return new(r, g, b, a);
}

}