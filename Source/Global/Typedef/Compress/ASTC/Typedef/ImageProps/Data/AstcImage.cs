using System.Runtime.InteropServices;

// Astc Image (matches astcenc_image in astcenc.h)

[StructLayout(LayoutKind.Sequential)]

public unsafe struct AstcImage
{
public uint DimX;

public uint DimY;

public uint DimZ;

public AstcDataType DataType;

public void** Data;

// ctor

public AstcImage(int width, int height, void** dataPtr,
                 AstcDataType type = AstcDataType.U8)
{
DimX = (uint)width;
DimY = (uint)height;
DimZ = 1;

Data = dataPtr;
DataType = type;
}

}