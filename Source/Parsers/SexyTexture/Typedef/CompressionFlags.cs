namespace TextureTranscoder.Parsers.SexyTexture
{
/// <summary> Determines how to Compress SexyTex Files

public enum CompressionFlags : uint
{
/// <summary> SexyTextures won't use Compression. </summary>
NoCompression,

/// <summary> SexyTextures will be Compressed by using the ZLIB algorithm. </summary>
ZLib
}

}