namespace TextureTranscoder.Parsers.DirectDrawSurface
{
/// <summary> Represents formats for DDS Files </summary>

public enum DdsFormat : uint
{
/// <summary> DirectX Texture </summary>
DXT1 = 0x31545844,

/// <summary> DirectX Texture 3 </summary>
DXT3 = 0x33545844,

/// <summary> DirectX Texture 5 </summary>
DXT5 = 0x35545844,
}

}