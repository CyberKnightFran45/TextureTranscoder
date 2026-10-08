using System;
using SkiaSharp;

// Parse DXT5 Images in RGBA Order with Morton mapping

public static class DXT5_RGBA_Morton
{
// Decode texture

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
_ = TextureHelper.AdjustSize(ref width, ref height);

var decImg = DXTDecoder.Decode(source, width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);
Morton.FromMap(decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image);
Morton.ToMap(image);

return DXTEncoder.Encode(image, false, null, DXT4_RGBA.EncodeAlpha);
}

}