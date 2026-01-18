using System.IO;
using SkiaSharp;

// Parse DXT5 Images in Morton Order (Tiled)

public static class DXT5_RGBA_Morton_Tiled
{
// Read Bitmap

public static SKBitmap Read(Stream reader, int width, int height)
{
TraceLogger.WriteLine("• DXT5-RGBA-Morton-Tiled Texture Decode:");
TraceLogger.WriteLine();

_ = TextureHelper.AdjustSize(ref width, ref height);

var decImg = DXTDecoder.Decode(reader, width, height, DXT3_RGBA.DecodeBlock, DXT4_RGBA.DecodeAlpha);

TraceLogger.WriteActionStart("Sorting blocks...");
Morton.FromCurve(decImg);

TraceLogger.WriteActionEnd();

return decImg;
}

// Write pixels

public static int Write(Stream writer, SKBitmap image)
{
TraceLogger.WriteLine("• DXT5-RGBA-Morton-Tiled Texture Encode:");
TraceLogger.WriteLine();

TextureHelper.ResizeImage(ref image);

TraceLogger.WriteActionStart("Sorting blocks...");
Morton.ToCurve(image);

TraceLogger.WriteActionEnd();

return DXTEncoder.Encode(writer, image, false, null, DXT4_RGBA.EncodeAlpha);
}

}