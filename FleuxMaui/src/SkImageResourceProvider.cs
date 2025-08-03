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
                System.Console.WriteLine($"Failed to load embedded resource: {resourceName}");
                return null;
            }
            var bitmap = SKBitmap.Decode(stream);
            return new Bitmap(bitmap);
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

            // 3. Create a bitmap and draw the picture into it
            var skbmp = new SKBitmap(width, height);
            using (var canvas = new SKCanvas(skbmp))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.DrawPicture(picture);
                canvas.Flush();
            }
            bmp = new Bitmap(skbmp);
        }
        else
        {
            bmp = GetBitmap(imagePath);
        }
        
        if (bmp == null)
        {
            System.Console.WriteLine($"Failed to create bitmap from image path: {imagePath}");
            return null;
        }

        var iimage = new BasicImage(bmp);
        return new IImageWrapper(iimage);
    }

    public Bitmap GetBitmap(string imagePath)
    {
        var skBitmap = SKBitmap.Decode(imagePath);
        if (skBitmap == null)
        {
            System.Console.WriteLine($"Failed to decode image from path: {imagePath}");
            return null;
        }
        return new Bitmap(skBitmap);
    }

}