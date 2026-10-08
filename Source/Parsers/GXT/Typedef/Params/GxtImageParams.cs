namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Defines some Params for Encoding a GXT Image. </summary>

public class GxtImageParams
{
/// <summary> Gets or Sets the PaletteIndex </summary>

public int PaletteIndex{ get; set; } = -1;

/// <summary> Gets or Sets some special Flags </summary>

public int Flags{ get; set; }

/// <summary> Gets or Sets the Texture Type </summary>

public GxtTextureType Type{ get; set; }

/// <summary> Gets or Sets the Texture format </summary>

public GxtFormat Format{ get; set; }

/// <summary> Gets or Sets the MipMaps </summary>

public uint MipCount{ get; set; } = 1;

// ctor

public GxtImageParams()
{
}

// ctor 2

public GxtImageParams(GxtFormat format)
{
Format = format;
}

// ctor 3

public GxtImageParams(int paletteIndex, int flags, GxtTextureType type, GxtFormat format, uint mips)
{
PaletteIndex = paletteIndex;
Flags = flags;

Type = type;
Format = format;

MipCount = mips;
}

}

}