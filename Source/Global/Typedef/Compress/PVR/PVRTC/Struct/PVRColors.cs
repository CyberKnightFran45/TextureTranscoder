// Temp struct for PVR encoder

public struct PVRColors
{
// Raw color - A

public TextureColor128 RawA;

// Raw color - B

public TextureColor128 RawB;

// ctor

public PVRColors(TextureColor128 a, TextureColor128 b)
{
RawA = a;
RawB = b;
}

}