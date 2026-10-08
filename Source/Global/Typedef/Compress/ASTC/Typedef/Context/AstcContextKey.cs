// Astc Context Key 

using System;

public readonly struct AstcContextKey : IEquatable<AstcContextKey>
{
public readonly int BlockW;

public readonly int BlockH;

public readonly float Quality;

public readonly AstcProfile Profile;

public readonly int Threads;

public readonly bool Decompress;

// ctor

public AstcContextKey(int blockW, int blockH, float quality, AstcProfile profile,
                      int threads, bool decompress)
{
BlockW = blockW;
BlockH = blockH;

Quality = quality;
Profile = profile;

Threads = threads;
Decompress = decompress;
}

// Compare

public bool Equals(AstcContextKey other)
{

return BlockW == other.BlockW &&
       BlockH == other.BlockH &&
       Quality == other.Quality &&
       Profile == other.Profile &&
       Threads == other.Threads &&
       Decompress == other.Decompress;
		
}

// Compare as raw obj

public override bool Equals(object obj)
{
return obj is AstcContextKey other && Equals(other);		   
}

// Compute hash code

public override int GetHashCode()
{

return HashCode.Combine(BlockW, BlockH, Quality, Profile,
                        Threads, Decompress);

}
	
}