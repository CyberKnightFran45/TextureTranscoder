/// <summary> Structure for ETC2 block </summary>

public struct ETC2BlockData
{
/// <summary> Color bits (R, G, B) </summary>

public ulong ColorBits;

/// <summary> Alpha bits </summary>

public ulong AlphaBits;

/// <summary> Encoding mode </summary>

public ETC2Mode Mode;

/// <summary> Error between blocks </summary>

public int Error;

// ctor

public ETC2BlockData(ulong color, ulong alpha, ETC2Mode mode, int error)
{
ColorBits = color;
AlphaBits = alpha;

Mode = mode;
Error = error;
}

// ctor 2

public ETC2BlockData(ulong color, ETC2Mode mode, int error) : this(color, 0, mode, error)
{
}

}