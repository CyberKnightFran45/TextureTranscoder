using System;
using SkiaSharp;

// Parse DXT5 Images in RGBA Order with tiled Morton mapping

public static class DXT5_RGBA_Morton_Tiled
{
// Decode image

public static SKBitmap Decode(ReadOnlySpan<byte> source, int width, int height)
{
_ = TextureHelper.AdjustSize(ref width, ref height);

var decImg = DXTDecoder.Decode(source, width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);
Morton.FromCurve(decImg);

return decImg;
}

// Encode image

public static NativeBuffer Encode(ref SKBitmap image)
{
TextureHelper.ResizeImage(ref image);
Morton.ToCurve(image);

return DXTEncoder.Encode(image, false, null, DXT4_RGBA.EncodeAlpha);
}

}