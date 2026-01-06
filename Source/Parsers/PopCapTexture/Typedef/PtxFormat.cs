namespace TextureTranscoder.Parsers.PopCapTexture
{
/// <summary> Represents formats for PopCap Textures </summary>

public enum PtxFormat
{
/// <summary> 0 - ARGB8888 (for iOS) </summary>
ARGB8888,

/// <summary> 1 - RGBA4444 (for Android and iOS) </summary>
RGBA4444,

/// <summary> 2 - RGB565 (for Android) </summary>
RGB565,

/// <summary> 3 - RGBA5551 (for Android and iOS) </summary>
RGBA5551,

/// <summary> 21 - RGBA4444-Tiled (for Android and iOS) </summary>
RGBA4444_Tiled = 21,

/// <summary> 22 - RGB565-Tiled (for Android) </summary>
RGB565_Tiled,

/// <summary> 23 - RGBA5551-Tiled (for Android and iOS) </summary>
RGBA5551_Tiled,

/// <summary> 30 - PVR-4BPP-RGBA (iOS) </summary>
PVRTC_4BPP_RGBA = 30,

/// <summary> 147 - ETC1-RGB-A8 (Android) </summary>
ETC1_RGB_A8 = 147,

/// <summary> 148 - PVR-4BPP-RGB-A8 (iOS) </summary>
PVRTC_4BPP_RGB_A8,

/// <summary> 0 - RGBA8888 (for Android) </summary>
RGBA8888 = -1,

/// <summary> 147 - ETC1-RGB-A-Palette (Android, Chinese Version) </summary>
ETC1_RGB_A_Palette = -147,
}

}