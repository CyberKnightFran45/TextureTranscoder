using System;

// Base class for PowerVR Texture Compression (PVRTC)

public static class PVRBase
{
// Block Height

public const int BLOCK_HEIGHT = 4;

// Block Widths

public const int BLOCK_WIDTH_2BPP = 8;
public const int BLOCK_WIDTH_4BPP = 4;

// Bit Mask

public const uint MASK_2BPP = 0b11u;
public const uint MASK_4BPP = 0b1111u;

// Modulation bit pos (2bpp)

public static readonly byte[] MOD_POS_2BPP = [ 0, 1, 4, 5, 2, 3, 6, 7, 2, 3, 6, 7, 2, 3, 6, 7 ];

// Modulation bit pos (4bpp)

public static readonly byte[] MOD_POS_4BPP = [ 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 ];

// Pixel offsets (2bpp mode)

public static readonly int[][] PX_OFFSETS_2BPP =
[
    [  0,  2,  4,  6,  0,  2,  4,  6 ],
    [  8, 10, 12, 14,  8, 10, 12, 14 ],
    [ 16, 18, 20, 22, 16, 18, 20, 22 ],
    [ 24, 26, 28, 30, 24, 26, 28, 30 ]
];

// Pixel offsets (4bpp mode)

public static readonly int[][] PX_OFFSETS_4BPP =
[
    [  0,  4,  8, 12 ],
    [ 16, 20, 24, 28 ],
    [ 32, 36, 40, 44 ],
    [ 48, 52, 56, 60 ]
];

// Bilinear factors

public static readonly byte[][] BILINEAR_FACTORS =
[
    [  4,  4,  4,  4 ],
    [  2,  6,  2,  6 ],
    [  8,  0,  8,  0 ],
    [  6,  2,  6,  2 ],

    [  2,  2,  6,  6 ],
    [  1,  3,  3,  9 ],
    [  4,  0, 12,  0 ],
    [  3,  1,  9,  3 ],

    [  8,  8,  0,  0 ],
    [  4, 12,  0,  0 ],
    [ 16,  0,  0,  0 ],
    [ 12,  4,  0,  0 ],

    [  6,  6,  2,  2 ],
    [  3,  9,  1,  3 ],
    [ 12,  0,  4,  0 ],
    [  9,  3,  3,  1 ]
];

// Adjust Image Dimensions

public static bool AdjustSize(ref int width, ref int height, bool is2BPP)
{
int minWidth = is2BPP ? 16 : 8;
int minHeight = 8;

width = Math.Max(width, minWidth);
height = Math.Max(height, minHeight);

return TextureHelper.AdjustSize(ref width, ref height);
}

}