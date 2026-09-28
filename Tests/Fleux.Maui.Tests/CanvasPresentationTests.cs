using Fleux.Controls;
using NUnit.Framework;
using SkiaSharp;

namespace Fleux.Tests;

public class CanvasPresentationTests
{
    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(true, true)]
    public void PresentsEveryPixelAtItsReflectedPosition(bool horizontal, bool vertical)
    {
        using var source = new SKBitmap(3, 2);
        source.Pixels = new[] { SKColors.Red, SKColors.Green, SKColors.Blue,
            SKColors.Yellow, SKColors.Magenta, SKColors.Cyan };
        using var image = SKImage.FromBitmap(source);

        // An asymmetric image and unequal axes catch axis swaps and wrong pivots.
        foreach (var scale in new[] { 1, 2, 3 })
        {
            using var target = new SKBitmap(source.Width * scale, source.Height * scale);
            using var canvas = new SKCanvas(target);
            for (var frame = 0; frame < 3; frame++)
            {
                canvas.Clear(SKColors.Black);
                CanvasPresentation.DrawImage(canvas, image, target.Width, target.Height,
                    null, horizontal, vertical);
                for (var y = 0; y < target.Height; y++)
                for (var x = 0; x < target.Width; x++)
                {
                    // Same inverse mapping as FleuxControl's pointer handling.
                    var sourceX = horizontal ? source.Width - x / scale - 1 : x / scale;
                    var sourceY = vertical ? source.Height - y / scale - 1 : y / scale;
                    Assert.That(target.GetPixel(x, y), Is.EqualTo(source.GetPixel(sourceX, sourceY)),
                        $"frame={frame}, scale={scale}, pixel={x},{y}");
                }
            }
        }
    }

    [Test]
    public void PreservesExistingCanvasStateAndDoesNotMirrorTheNextFrame()
    {
        using var source = new SKBitmap(3, 2);
        source.Erase(SKColors.Red);
        using var image = SKImage.FromBitmap(source);
        using var target = new SKBitmap(12, 8);
        using var canvas = new SKCanvas(target);
        canvas.Translate(2, 1);
        canvas.ClipRect(new SKRect(0, 0, 8, 6));
        var matrix = canvas.TotalMatrix;
        var clip = canvas.DeviceClipBounds;
        var saves = canvas.SaveCount;

        CanvasPresentation.DrawImage(canvas, image, 6, 4, null, true, true);
        Assert.That(canvas.TotalMatrix, Is.EqualTo(matrix));
        Assert.That(canvas.DeviceClipBounds, Is.EqualTo(clip));
        Assert.That(canvas.SaveCount, Is.EqualTo(saves));

        source.SetPixel(0, 0, SKColors.Blue);
        using var normal = SKImage.FromBitmap(source);
        CanvasPresentation.DrawImage(canvas, normal, 3, 2, null, false, false);
        Assert.That(target.GetPixel(2, 1), Is.EqualTo(SKColors.Blue));
    }
}
