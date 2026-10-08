using System;
using System.Runtime.InteropServices;
using SkiaSharp;

// Supports Linear Textures in RGB format

public static unsafe class RGB
{
#region ==========  ENCODER  ==========

// Color encoder delegate

public delegate T ColorEncoder<T>(in TextureColor color);

// Encode Image as Binary Texture

private static NativeBuffer Encode<T>(SKBitmap image, ColorEncoder<T> encoder)
                                      where T : unmanaged
{
var pixels = (TextureColor*)image.GetPixels().ToPointer();
int square = image.GetSquare();

int rawSize = square * sizeof(T);
NativeBuffer output = new(rawSize);

var colorInfo = MemoryMarshal.Cast<byte, T>(output.AsSpan() );

for(int i = 0; i < square; i++)
colorInfo[i] = encoder(pixels[i] );

return output;
}

// Encode RGB 8-bits

public static NativeBuffer Encode8(SKBitmap image, ColorEncoder<byte> encoder)
{
return Encode(image, encoder);
}

// Encode RGB 16-bits

public static NativeBuffer Encode16(SKBitmap image, ColorEncoder<ushort> encoder)
{
return Encode(image, encoder);
}

// Encode Helper for 24-bits

private static ColorEncoder<uint> EncodeFunc24(ColorEncoder<uint> baseFunc)
{
return (in c) => baseFunc(c) & 0x00FFFFFF;
}

// Encode RGB 24-bits

public static NativeBuffer Encode24(SKBitmap image, ColorEncoder<uint> encoder)
{
return Encode(image, EncodeFunc24(encoder) );
}

// Encode RGB 32-bits (to span)

internal static void Encode32(SKBitmap image, Span<byte> destination, ColorEncoder<uint> encoder)
{
int expected = image.GetSquare() * 4;

if(destination.Length < expected)
throw new ArgumentException("Destination buffer is too small.", nameof(destination) );

var pixels = (TextureColor*)image.GetPixels().ToPointer();
var colorInfo = MemoryMarshal.Cast<byte, uint>(destination[.. expected] );

for(int i = 0; i < image.GetSquare(); i++)
colorInfo[i] = encoder(pixels[i] );

}

// Encode RGB 32-bits 

public static NativeBuffer Encode32(SKBitmap image, ColorEncoder<uint> encoder)
{
int rawSize = image.GetSquare() * 4;
NativeBuffer output = new(rawSize);

Encode32(image, output.AsSpan(), encoder);

return output;
}

// Encode RGB 64-bits

public static NativeBuffer Encode64(SKBitmap image, ColorEncoder<ulong> encoder)
{
return Encode(image, encoder);
}

// Encode Tiled Image

private static NativeBuffer EncodeTile<T>(SKBitmap image, int tileSize, ColorEncoder<T> encoder)
                                          where T : unmanaged
{
int width = image.Width;
int height = image.Height;

int newWidth = (width + (tileSize - 1) ) & ~(tileSize - 1);
var pixels = (TextureColor*)image.GetPixels().ToPointer();

int totalPixels = newWidth * height;

int rawSize = totalPixels * sizeof(T);
NativeBuffer owner = new(rawSize);

var colorInfo = MemoryMarshal.Cast<byte, T>(owner.AsSpan());

int blocksPerRow = (newWidth + tileSize - 1) / tileSize;

for(int i = 0; i < height; i++)
{
int col = i / tileSize;
int inBlockY = i % tileSize;

for(int j = 0; j < newWidth; j++)
{
int row = j / tileSize;
int inBlockX = j % tileSize;

int blockIndex = col * blocksPerRow + row;
int pixelIndexInBlock = inBlockY * tileSize + inBlockX;

int dstIndex = blockIndex * tileSize * tileSize + pixelIndexInBlock;

if(j < width)
colorInfo[dstIndex] = encoder(pixels[i * width + j] );

}

}

return owner;
}

// Encode Tile as 16-bits

public static NativeBuffer EncodeTile16(SKBitmap image, int tileSize, ColorEncoder<ushort> encoder)
{
return EncodeTile(image, tileSize, encoder);
}

#endregion


#region ==========  DECODER  ==========

// Color decoder delegate

public delegate TextureColor ColorDecoder<T>(in T flags);

// Decode Binary Texture as Png

private static SKBitmap Decode<T>(ReadOnlySpan<byte> source, 
                                  int width,
                                  int height,
                                  ColorDecoder<T> decoder)
                                  where T : unmanaged
{
SKBitmap image = new(width, height);

int square = image.GetSquare();
var pixels = (TextureColor*)image.GetPixels().ToPointer();

var colorInfo = MemoryMarshal.Cast<byte, T>(source);

for(int i = 0; i < square; i++)
pixels[i] = decoder(colorInfo[i] );

return image;
}

// Decode RGB 8-bits

public static SKBitmap Decode8(ReadOnlySpan<byte> source,
                               int width,
							   int height,
                               ColorDecoder<byte> decoder)
{
return Decode(source, width, height, decoder);
}

// Decode RGB 16-bits

public static SKBitmap Decode16(ReadOnlySpan<byte> source,
                                int width,
								int height,
                                ColorDecoder<ushort> decoder)
{
return Decode(source, width, height, decoder);
}

// Decode Helper for 24-bits

private static ColorDecoder<uint> DecodeFunc24(ColorDecoder<uint> baseFunc)
{
return (in f) => baseFunc(f & 0x00FFFFFF);
}

// Decode RGB 24-bits

public static SKBitmap Decode24(ReadOnlySpan<byte> source,
                                int width,
								int height,
                                ColorDecoder<uint> decoder)
{
return Decode(source, width, height, DecodeFunc24(decoder) );
}

// Decode RGB 32-bits

public static SKBitmap Decode32(ReadOnlySpan<byte> source,
                                int width,
								int height,
                                ColorDecoder<uint> decoder)
{
return Decode(source, width, height, decoder);
}

// Decode RGB 64-bits

public static SKBitmap Decode64(ReadOnlySpan<byte> source,
                                int width,
								int height,
                                ColorDecoder<ulong> decoder)
{
return Decode(source, width, height, decoder);
}

// Decode Tiled Texture as Png

private static SKBitmap DecodeTile<T>(ReadOnlySpan<byte> source,
                                      int width,
								      int height,
									  int tileSize,
                                      ColorDecoder<T> decoder)
                                      where T : unmanaged
{
SKBitmap image = new(width, height);

var pixels = (TextureColor*)image.GetPixels().ToPointer();
var colorInfo = MemoryMarshal.Cast<byte, T>(source);

int blocksPerRow = (width + tileSize - 1) / tileSize;

for(int i = 0; i < height; i++)
{
int col = i / tileSize;
int inBlockY = i % tileSize;

for(int j = 0; j < width; j++)
{
int row = j / tileSize;
int inBlockX = j % tileSize;

int blockIndex = col * blocksPerRow + row;
int pixelIndexInBlock = inBlockY * tileSize + inBlockX;

int srcIndex = blockIndex * tileSize * tileSize + pixelIndexInBlock;

pixels[i * width + j] = decoder(colorInfo[srcIndex] );
}

}

return image;
}

// Decode Tile (16-bits)

public static SKBitmap DecodeTile16(ReadOnlySpan<byte> source,
                                    int width,
									int height,
									int tileSize,
                                    ColorDecoder<ushort> decoder)
{
return DecodeTile(source, width, height, tileSize, decoder);
}

#endregion
}