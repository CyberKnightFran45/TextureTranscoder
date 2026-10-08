using SkiaSharp;
using System;
using System.IO;

// SkiaSharp Plugin

public static unsafe class SKPlugin
{
// Default Png Info

public static SKImageInfo GetDefaultImgInfo(int width, int height) => new()
{
ColorType = SKColorType.Bgra8888,
AlphaType = SKAlphaType.Unpremul,
ColorSpace = null,
Width = width,
Height = height
};

// Default Encoder Options

public static SKPngEncoderOptions DefaultEncoderOptions => new();

// Create new Bitmap

public static SKBitmap Create(int width, int height)
{
var imgInfo = GetDefaultImgInfo(width, height);

return new(imgInfo);
}

// Load Core

private static SKBitmap LoadCore(SKCodec codec)
{
var codedData = codec.Info;
var imgInfo = GetDefaultImgInfo(codedData.Width, codedData.Height);

return SKBitmap.Decode(codec, imgInfo);
}

// Load Img from Stream

public static SKBitmap FromStream(Stream source)
{
using SKCodec codec = SKCodec.Create(source);

return LoadCore(codec);
}

// Load Img from File

public static SKBitmap FromFile(string filePath)
{
using SKCodec codec = SKCodec.Create(filePath);

return LoadCore(codec);
}

// Get Square

public static int GetSquare(this SKBitmap bitmap) => bitmap.Width * bitmap.Height;

// Save Core

private static void SaveCore(SKBitmap bitmap, Stream output)
{
using SKPixmap pixels = bitmap.PeekPixels();
using SKData data = pixels.Encode(DefaultEncoderOptions);

using var imgStream = data.AsStream(true);

FileManager.Process(imgStream, output);
}

// Write the Bitmap as a Png file

public static void Save(this SKBitmap bitmap, string filePath)
{
using var outStream = FileManager.OpenWrite(filePath);

SaveCore(bitmap, outStream);
}

// Write Bitmap as Png Stream

public static void Save(this SKBitmap bitmap, Stream output) => SaveCore(bitmap, output);

// Move Bitmap to Coordinate (X, Y)

public static void MoveTo(this SKBitmap src, SKBitmap dest, int startX, int startY)
{
int srcHeight = src.Height;
int srcWidth = src.Width;

int dstHeight = dest.Height;
int dstWidth = dest.Width;

var srcPtr = (uint*)src.GetPixels().ToPointer();
var dstPtr = (uint*)dest.GetPixels().ToPointer();

for(int row = 0; row < srcHeight; row++)
{
int dstY = startY + row;
uint* srcRow = srcPtr + row * srcWidth;

if(dstY < 0 || dstY >= dstHeight) 
continue;

uint* dstRow = dstPtr + dstY * dstWidth;

for(int col = 0; col < srcWidth; col++)
{
int dstX = startX + col;

if(dstX < 0 || dstX >= dstWidth)
continue;

dstRow[dstX] = srcRow[col];
}

}

}

// Cut Bitmap by Dimensions

public static SKBitmap Cut(this SKBitmap src, int startX, int startY, int width, int height)
{
SKBitmap cut = new(width, height);

var srcPtr = (uint*)src.GetPixels().ToPointer();
var dstPtr = (uint*)cut.GetPixels().ToPointer();

int srcWidth = src.Width;
int srcHeight = src.Height;

for(int row = 0; row < height; row++)
{
uint* dstRow = dstPtr + row * width;
int srcY = startY + row;

for(int col = 0; col < width; col++)
{
int srcX = startX + col;

if(srcX < 0 || srcX >= srcWidth || srcY < 0 || srcY >= srcHeight)
*dstRow++ = 0;

else
*dstRow++ = srcPtr[srcY * srcWidth + srcX];

}

}

return cut;
}

// Put SubImage to Main Bitmap

public static void Put(this SKBitmap source, SKBitmap child, int x, int y, int width, int height)
{
using SKCanvas drawer = new(source);

SKRect srcDim = new(0, 0, width, height);
SKRect dstDim = new(x, y, x + width, y + height);

drawer.DrawBitmap(child, srcDim, dstDim);
}

// Pixel transform

private delegate long PixelTransform(int x, int y, int width, int height);

// Rotate Pixels

private static void RotatePx(uint* src, uint* dst, int width, int height,
                             PixelTransform transform)
{

for(int y = 0; y < height; y++)
{

for(int x = 0; x < width; x++)
{
long rotIdx = transform(x, y, width, height);
long linearIdx = y * width + x;

dst[rotIdx] = src[linearIdx];
}

}

}

// Rotate Image (Core)

private static SKBitmap RotateCore(SKBitmap target, PixelTransform transform)
{
int width = target.Height;
int height = target.Width;

SKBitmap rotated = new(width, height);

var src = (uint*)target.GetPixels().ToPointer();
var dst = (uint*)rotated.GetPixels().ToPointer();

RotatePx(src, dst, width, height, transform);

return rotated;
}

// Rotate Image to 90 Degrees

public static SKBitmap Rotate90(this SKBitmap target)
{
static long rot90(int x, int y, int width, int _) => x * width + (width - y - 1);

return RotateCore(target, rot90);
}

// Rotate Image to 180 Degrees

public static SKBitmap Rotate180(this SKBitmap target)
{
static long rot180(int x, int y, int width, int height) => (height - y - 1) * width + (width - x - 1);

return RotateCore(target, rot180);
}

// Rotate Image to 270 Degrees

public static SKBitmap Rotate270(this SKBitmap target)
{
static long rot270(int x, int y, int width, int height) => (height - x - 1) * width + y;

return RotateCore(target, rot270);
}

}