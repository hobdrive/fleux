using System.Drawing.Imaging;
using Fleux.Core;
using SkiaSharp;

namespace System.Drawing;

public class Graphics : IDisposable
{
    static bool DebugIgnorePrivitives = false;
    static bool DebugIgnoreText = false;
    static bool DebugIgnoreImages = false;

    protected Image Image;
    
    SKCanvas canvas;

    SKPaint skPaint = new SKPaint
    {
        IsAntialias = false,
        FilterQuality = SKFilterQuality.Low,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
    };

    // GPU context for creating GPU-accelerated bitmaps (internal)
    internal GRRecordingContext GRContext => Image?.GetSurface()?.Context;

    public Graphics(Image image)
    {
        Image = image;
        if (image.GetSurface() != null)
        {
            canvas = image.GetSurface()?.Canvas;
        }
        else
        {
            throw new ArgumentException("Image must have OffscreenSurface initialized");
        }
    }

    void Init()
    {
    }

    public void Flush()
    {
        canvas.Flush();
        Image.InvalidateSnapshot();
    }

    public static Graphics FromImage(Image image)
    {
        return new Graphics(image);
    }

    // Factory method to create GPU-accelerated bitmaps when possible
    public Bitmap CreateBitmap(int width, int height)
    {
        var grContext = this.GRContext;
        return grContext != null
            ? new Bitmap(grContext, width, height)
            : new Bitmap(width, height);  // Fallback to CPU if no GPU context
    }

    public SKPaint Paint { get { return skPaint; } }

    public SKCanvas Canvas { get { return canvas; } }

    public void DrawImage(Image image, Rectangle target, Rectangle source, GraphicsUnit gu)
    {
        if (DebugIgnoreImages)
            return;
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        var srcRect = new SKRect(source.X, source.Y, source.X + source.Width, source.Y + source.Height);
        var dstRect = new SKRect(target.X, target.Y, target.X + target.Width, target.Y + target.Height);

        var skImage = image.GetSkImage();  // Don't dispose - owned by image
        canvas.DrawImage(skImage, srcRect, dstRect, skPaint);
    }

    public void DrawImage(Image image, int x, int y)
    {
        if (DebugIgnoreImages)
            return;
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        var skImage = image.GetSkImage();  // Don't dispose - owned by image
        canvas.DrawImage(skImage, x, y, skPaint);
    }

    public void DrawImage(Image image, int x, int y, Rectangle source, GraphicsUnit gu)
    {
        if (DebugIgnoreImages)
            return;
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        var srcRect = new SKRect(source.X, source.Y, source.X + source.Width, source.Y + source.Height);
        var skImage = image.GetSkImage();  // Don't dispose - owned by image
        canvas.DrawImage(skImage, srcRect, new SKRect(x, y, x + source.Width, y + source.Height), skPaint);
    }

    public void DrawImage(Image image, Rectangle to, int fromx, int fromy, int fromw, int fromh, GraphicsUnit gu, ImageAttributes ia)
    {
        if (DebugIgnoreImages)
            return;
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;
        // take ia.GetColorMatrix().Matrix[3][3] as transparency
        if (ia != null && ia.GetColorMatrix() != null)
        {
            float transparency = ia.GetColorMatrix().Matrix[3][3];
            skPaint.Color = skPaint.Color.WithAlpha((byte)(255 * (transparency)));
        }

        var srcRect = new SKRect(fromx, fromy, fromx + fromw, fromy + fromh);
        var dstRect = new SKRect(to.X, to.Y, to.X + to.Width, to.Y + to.Height);

#if xDEBUG
        FleuxApplication.Log($"DB {this.GetHashCode()} DrawImage: from {srcRect.ToString()} to {dstRect.ToString()} canvas Matrix {canvas.TotalMatrix.ToString()}");
#endif

        var skImage = image.GetSkImage();  // Don't dispose - owned by image
        canvas.DrawImage(skImage, srcRect, dstRect, skPaint);
    }

    public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawLine(x1, y1, x2, y2, skPaint);
    }

    public void DrawRectangle(Pen pen, int x1, int y1, int w, int h)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawRect(x1, y1, w, h, skPaint);
    }

    public void DrawEllipse(Pen pen, int x, int y, int w, int h)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawOval(new SKRect(x, y, x + w, y + h), skPaint);
    }

    public void DrawArc(Pen pen, int x, int y, int w, int h, float sw, float swa)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        var rect = new SKRect(x, y, x + w, y + h);
        canvas.DrawArc(rect, sw, swa, false, skPaint);
    }

    public void FillRectangle(Brush brush, int x1, int y1, int w, int h)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Fill;
        skPaint.StrokeWidth = 1;

        canvas.DrawRect(x1, y1, w, h, skPaint);
    }

    public void FillPolygon(Brush brush, Point[] points)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Fill;
        skPaint.StrokeWidth = 1;

        var skPoints = new SKPoint[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            skPoints[i] = new SKPoint(points[i].X, points[i].Y);
        }

        canvas.DrawPoints(SKPointMode.Polygon, skPoints, skPaint);
    }

    public void FillEllipse(Brush brush, int x, int y, int w, int h)
    {
        if (DebugIgnorePrivitives)
            return;
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Fill;
        skPaint.StrokeWidth = 1;

        canvas.DrawOval(new SKRect(x, y, x + w, y + h), skPaint);
    }

    public void DrawString(string text, Font font, Brush brush, float x, float y)
    {
        DrawString(text, font, brush, (int)x, (int)y);
    }

    public void DrawString(string text, Font font, Brush brush, int x, int y)
    {
        if (DebugIgnoreText)
            return;
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Fill;

        if (brush.Stroke)
        {
            skPaint.Style = SKPaintStyle.Stroke;
            skPaint.StrokeWidth = brush.StrokeWidth;
        }

        var skLineSpacing = font.ToSKFont().GetFontMetrics(out var fontMetrics);

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            //var bounds = new SKRect();
            //skPaint.MeasureText(line, ref bounds);
            //y += (int)bounds.Height;
            y += (int)-fontMetrics.Ascent;

            canvas.DrawText(line, x, y, SKTextAlign.Left, font.ToSKFont(), skPaint);
            y += (int)fontMetrics.Descent;
        }
    }

    public void DrawString(string text, Font font, Brush brush, RectangleF rect)
    {        
        if (DebugIgnoreText)
            return;

        var skLineSpacing = font.ToSKFont().GetFontMetrics(out var fontMetrics);

        float lineHeight = fontMetrics.Descent - fontMetrics.Ascent;
        float y = rect.Top - fontMetrics.Ascent;
        float maxWidth = rect.Width;

        var multiLine = new MultiLine((int)rect.Width, (t) => MeasureTextCRLF(t, font.ToSKFont()), text);
        DrawString(multiLine.Text, font, brush, rect.X, rect.Y);
    }

    public Size MeasureStringWidth(string text, Font font, int width)
    {
        if (DebugIgnoreText)
            return Size.Empty;
        skPaint.Style = SKPaintStyle.Fill;

        var skLineSpacing = font.ToSKFont().GetFontMetrics(out var fontMetrics);

        float lineHeight = fontMetrics.Descent - fontMetrics.Ascent;

        if (width == 0)
            return new Size(0, 0);

        // Shortcut for single line text
        var simpleSize = MeasureTextCRLF(text, font.ToSKFont());
        if (simpleSize.Width < width)
            return simpleSize;

        var multiLine = new MultiLine(width, (t) => MeasureTextCRLF(t, font.ToSKFont()), text);

        return multiLine.Size;
    }

    internal Size MeasureTextCRLF(string text, SKFont font)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        int maxWidth = 0;
        int totalHeight = 0;

        foreach (var line in lines)
        {
            var bounds = new SKRect();
            font.MeasureText(line, out bounds, skPaint);
            maxWidth = Math.Max(maxWidth, (int)bounds.Width);
            totalHeight += (int)bounds.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    public Size MeasureString(string text, Font font)
    {
        if (DebugIgnoreText)
            return Size.Empty;

        var bounds = new SKRect();
        font.ToSKFont().MeasureText(text, out bounds, skPaint);

        var skLineSpacing = font.ToSKFont().GetFontMetrics(out var fontMetrics);
        float lineHeight = fontMetrics.Descent - fontMetrics.Ascent;
        var y = fontMetrics.Descent - fontMetrics.Ascent;

        return new Size((int)bounds.Width, Math.Max((int)bounds.Height, (int)y));
    }

    public void Clear()
    {
        Clear(Color.Transparent);
    }

    public void Clear(Color fill)
    {
        canvas.Clear(fill.ToSKColor());
    }

    // What for???
    int DeviceDPI = 92;
    public int DpiX{
        get{
            return DeviceDPI;
        }
    }

    public void TranslateTransform(int x, int y)
    {
        canvas.Translate(x, y);
    }

    public void RotateTransform(float angle)
    {
        canvas.RotateDegrees(angle);
    }

    public void ScaleTransform(float sx, float sy)
    {
        canvas.Scale(sx, sy);
    }

    public object Transform{
        get{
            return canvas.TotalMatrix;
        }
        set{
            if (value is SKMatrix matrix)
            {
                canvas.SetMatrix(matrix);
            }
            else
            {
                throw new ArgumentException("Transform must be of type SKMatrix");
            }
        }
    }

    float ctransparency = 0;

    public float Transparency{
        get{
            return ctransparency;
        }
        set
        {
            ctransparency = value;
            skPaint.Color = skPaint.Color.WithAlpha((byte)(255 * (1 - ctransparency)));
        }
    }

    public void ResetTransform()
    {
        canvas.ResetMatrix();
    }

    public void Dispose()
    {
    }
}
