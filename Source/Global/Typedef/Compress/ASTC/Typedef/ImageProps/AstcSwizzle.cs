using System.Runtime.InteropServices;

// Astc Swizzle

[StructLayout(LayoutKind.Sequential)]

public struct AstcSwizzle
{
public int R, G, B, A;

// ctor

public AstcSwizzle(int r, int g, int b, int a)
{
R = r;
G = g;
B = b;
A = a;
}

}