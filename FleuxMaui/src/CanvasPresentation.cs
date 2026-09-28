using SkiaSharp;

namespace Fleux.Controls;

internal static class CanvasPresentation
{
    // Reflect the final image in surface pixels, independently of back-buffer
    // resolution. Input is reflected separately by FleuxControl in buffer pixels.
    internal static void DrawImage(SKCanvas canvas, SKImage image, int width, int height,
        SKPaint? paint, bool horizontalMirror, bool verticalMirror)
    {
        using var saved = new SKAutoCanvasRestore(canvas, true);
        canvas.Translate(horizontalMirror ? width : 0, verticalMirror ? height : 0);
        canvas.Scale(horizontalMirror ? -1 : 1, verticalMirror ? -1 : 1);
        canvas.DrawImage(image, new SKRect(0, 0, width, height), paint);
    }
}
