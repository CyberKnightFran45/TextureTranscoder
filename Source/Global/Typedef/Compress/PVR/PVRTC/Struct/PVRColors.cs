// Temp struct for PVR encoder

public struct PVRColors
{
// Raw color - A

public TextureColor16 RawA;

// Raw color - B

public TextureColor16 RawB;

// ctor

public PVRColors(TextureColor16 a, TextureColor16 b)
{
RawA = a;
RawB = b;
}

}