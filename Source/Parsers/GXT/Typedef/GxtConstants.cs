namespace TextureTranscoder.Parsers.GXT
{
/// <summary> Constants for GXT format </summary>

public static class GxtConstants
{
/// <summary> File identifier </summary>

public const uint MAGIC = 0x47585400;

/// <summary> File version </summary>

public const uint VERSION = 0x10000003;

/// <summary> Config source file </summary>

public const string SRC_CONFIG = "GxtInfo.json";

/// <summary> Images source dir </summary>

public const string SRC_IMAGES = "Images";

// Get error msg

public static string FormatMsg(GxtFormat format) => $"Unsupported format: {format} (0x{(uint)format:X8})";
}

}