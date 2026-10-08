using System;
using SkiaSharp;

// Parse PVR-RGB Images followed by A8 (4bpp)

public static class PVRTC_4BPP_RGB_A8
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
PVRBase.AdjustSize(ref width, ref height, false);

int compressedSize = PVRBase.ComputePVRSize(width, height, false);

var decImg = PVRDecoder.Decode(source[..compressedSize], width, height, false, false);
AlphaCodec.Decode8(source[compressedSize..], decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image, (ref w, ref h) => PVRBase.AdjustSize(ref w, ref h, false) );

int compressedSize = PVRBase.ComputePVRSize(image.Width, image.Height, false);
int alphaSize = image.GetSquare();

int encodedSize = compressedSize + alphaSize;

NativeBuffer output = new(encodedSize);

var colorInfo = output.AsSpan(0, compressedSize);
var alphaInfo = output.AsSpan(compressedSize);

PVREncoder.Encode(ref image, colorInfo, false, false);
AlphaCodec.Encode8(image, alphaInfo);

return output;
}

}