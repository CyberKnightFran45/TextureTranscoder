using System;

// Supports DirectX Texture Compression (DXT)

public static class DXTCommon
{
// Tile Size (4x4)

public const int TILE_SIZE = 4;

// Block encoder

public delegate void DXTBlockEncoder(Span<TextureColor> block);

// Alpha encoder

public delegate void DXTAlphaEncoder(ReadOnlySpan<TextureColor> block, Span<ushort> alpha);

// Block decoder

public delegate void DXTBlockDecoder(ReadOnlySpan<byte> alpha, Span<TextureColor> block);

// Alpha decoder (only used in DXT3-5)

public delegate void DXTAlphaDecoder(ReadOnlySpan<ushort> encoded, Span<byte> plain);
}