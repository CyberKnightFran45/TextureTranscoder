using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Defines some Params for Encoding a GXT Image. </summary>

public class GxtImageParams
{
/// <summary> Gets or Sets the PaletteIndex </summary>

public int PaletteIndex{ get ; set; } = -1;

/// <summary> Gets or Sets some special Flags </summary>

public int Flags{ get ; set; }

/// <summary> Gets or Sets the Texture Type </summary>

public GxtTextureType Type{ get ; set; }

/// <summary> Gets or Sets the Texture format </summary>

public GxtFormat Format{ get ; set; }

// ctor

public GxtImageParams()
{
}

// ctor 2

public GxtImageParams(GxtFormat format)
{
Format = format;
}

}

}