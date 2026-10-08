using System;
using SkiaSharp;

// Parse Image in the RGBA4444 format (Tiled)

public static class RGBA4444_Tiled
{
// Read Bitmap

public static SKBitmap Read(ReadOnlySpan<byte> source, int width, int height, int tileSize)
{
return RGB.DecodeTile16(source, width, height, tileSize, RGBA4444.DecodeColor);
}

public static SKBitmap Read(NativeBuffer source, int width, int height, int tileSize)
{
return Read(source.AsSpan(), width, height, tileSize);
}


// Write pixels

public static NativeBuffer Write(SKBitmap image, int tileSize)
{
return RGB.EncodeTile16(image, tileSize, RGBA4444.EncodeColor);
}


}