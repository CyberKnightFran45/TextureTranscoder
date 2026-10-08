using System;
using SkiaSharp;

// Parse PVR-RGB Images followed by A8 (2bpp)

public static class PVRTC_2BPP_RGB_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
PVRBase.AdjustSize(ref width, ref height, true);

int compressedSize = PVRBase.ComputePVRSize(width, height, true);

var decImg = PVRDecoder.Decode(source[.. compressedSize], width, height, true, false);
AlphaCodec.Decode8(source[compressedSize ..], decImg);

return decImg;
}

// Encode Image

public static NativeBuffer Encode(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => PVRBase.AdjustSize(ref w, ref h, true) );

int compressedSize = PVRBase.ComputePVRSize(image.Width, image.Height, true);
int alphaSize = image.GetSquare();

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

PVREncoder.Encode(ref image, colorInfo, true, false);
AlphaCodec.Encode8(image, alphaInfo);

return output;
}

}