namespace TextureTranscoder.Parsers.GXT
{
// Represents a GXT Texture Type

public enum GxtTextureType : uint
{
Swizzled,
Cubic = 0x40,
Linear = 0x60,
Tiled = 0x80,
Strided = 0x0C
}

}