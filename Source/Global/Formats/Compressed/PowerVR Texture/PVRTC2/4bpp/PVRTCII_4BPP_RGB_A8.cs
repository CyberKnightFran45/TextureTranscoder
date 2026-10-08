using System;
using SkiaSharp;

// Parse PVR2-RGB Images followed by A8 (4bpp)

public static class PVRTCII_4BPP_RGB_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
PVR2Base.AdjustSize(ref width, ref height, false);

int compressedSize = PVRBase.ComputePVRSize(width, height, false);

var decImg = PVRII_4BPP_Decoder.Decode(source[.. compressedSize], width, height, false);
AlphaCodec.Decode8(source[compressedSize ..], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Write(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => PVR2Base.AdjustSize(ref w, ref h, false) );

int compressedSize = PVRBase.ComputePVRSize(image.Width, image.Height, false);
int alphaSize = image.GetSquare();

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

PVRII_4BPP_Encoder.Encode(ref image, colorInfo, false);
AlphaCodec.Encode8(image, alphaInfo);

return output;
}

}
