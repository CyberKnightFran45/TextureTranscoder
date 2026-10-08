using System;
using SkiaSharp;

// Parse PVR2-RGB Images followed by A8 (2bpp)

public static class PVRTCII_2BPP_RGB_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
PVR2Base.AdjustSize(ref width, ref height, true);

int compressedSize = PVRBase.ComputePVRSize(width, height, true);

var decImg = PVRII_2BPP_Decoder.Decode(source[.. compressedSize], width, height, false);
AlphaCodec.Decode8(source[compressedSize ..], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => PVR2Base.AdjustSize(ref w, ref h, true) );

int compressedSize = PVRBase.ComputePVRSize(image.Width, image.Height, true);
int alphaSize = image.GetSquare();

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

PVRII_2BPP_Encoder.Encode(ref image, colorInfo, false);
AlphaCodec.Encode8(image, alphaInfo);

return output;
}

}
