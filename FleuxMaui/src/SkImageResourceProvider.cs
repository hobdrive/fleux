using System;
using System.Reflection;
using System.Drawing;
using SkiaSharp;
using Fleux.Core;
using Fleux.Core.NativeHelpers;

namespace Fleux.Core.SkiaSharp;

public class SkImageResourceProvider : IImageResourceProvider
{
    public IImageWrapper GetIImageFromEmbeddedResource(string resourceName, Assembly asm)
    {
        var bitmap = GetBitmapFromEmbeddedResource(resourceName, asm);
        if (bitmap == null)
        {
            return null;
        }
        var iimage = new BasicImage(bitmap);
        return new IImageWrapper(iimage);
    }

    public Bitmap GetBitmapFromEmbeddedResource(string resourceName, Assembly asm)
    {
        using (var stream = asm.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                FleuxApplication.Log($"Failed to load embedded resource: {resourceName}");
                return null;
            }
            //var bitmap = SKBitmap.Decode(stream);
            var skImage = SKImage.FromEncodedData(stream);
            if (skImage == null)
            {
                FleuxApplication.Log($"Failed to decode image from embedded resource: {resourceName}");
                return null;
            }
            return new Bitmap(skImage);
        }
    }

    public IImageWrapper GetIImage(string imagePath)
    {
        Bitmap bmp = null;
        if (imagePath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            var svg = new Svg.Skia.SKSvg();
            svg.Load(imagePath);

            // 2. Get bounds of the SVG
            var picture = svg.Picture;
            var bounds = picture.CullRect;

            // Optional: scale the output to a specific size
            int width = (int)bounds.Width;
            int height = (int)bounds.Height;

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.DrawPicture(picture);
            surface.Canvas.Flush();
            var image = surface.Snapshot();  // Immutable image
            bmp = new Bitmap(image);  // Thread-safe, GPU-friendly
        }
        else
        {
            bmp = GetBitmap(imagePath);
        }
        
        if (bmp == null)
        {
            FleuxApplication.Log($"Failed to create bitmap from image path: {imagePath}");
            return null;
        }

        var iimage = new BasicImage(bmp);
        return new IImageWrapper(iimage);
    }

    public Bitmap GetBitmap(string imagePath)
    {
        var skImage = SKImage.FromEncodedData(imagePath);
        if (skImage == null)
        {
            FleuxApplication.Log($"Failed to decode image from path: {imagePath}");
            return null;
        }
        return new Bitmap(skImage);
    }

}