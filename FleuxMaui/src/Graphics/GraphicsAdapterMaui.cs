using System.Drawing.Imaging;
using SkiaSharp;

namespace System.Drawing;

public class Graphics : IDisposable
{

    protected Image Image;
    SKCanvas canvas;
    SKPaint skPaint = new SKPaint
    {
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1
    };

    public Graphics(Image image)
    {
        Image = image;
        canvas = new SKCanvas(image.skBitmap);
    }

    void Init()
    {
    }

    public void Flush()
    {
    }

    public static Graphics FromImage(Image image)
    {
        return new Graphics(image);
    }

    public SKPaint Paint { get { return skPaint; } }

    public SKCanvas Canvas { get { return canvas; } }

    public void DrawImage(Image image, Rectangle target, Rectangle source, GraphicsUnit gu)
    {
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        var srcRect = new SKRect(source.X, source.Y, source.X + source.Width, source.Y + source.Height);
        var dstRect = new SKRect(target.X, target.Y, target.X + target.Width, target.Y + target.Height);

        canvas.DrawBitmap(image.skBitmap, srcRect, dstRect, skPaint);
    }

    public void DrawImage(Image image, int x, int y)
    {
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        canvas.DrawBitmap(image.skBitmap, x, y, skPaint);
    }

    public void DrawImage(Image image, int x, int y, Rectangle source, GraphicsUnit gu)
    {
        skPaint.Color = SKColors.White;
        skPaint.Style = SKPaintStyle.Fill;

        var srcRect = new SKRect(source.X, source.Y, source.X + source.Width, source.Y + source.Height);
        canvas.DrawBitmap(image.skBitmap, srcRect, new SKRect(x, y, x + source.Width, y + source.Height), skPaint);
    }

    public void DrawImage(Image image, Rectangle to, int fromx, int fromy, int fromw, int fromh, GraphicsUnit gu, ImageAttributes ia)
    {
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

        canvas.DrawBitmap(image.skBitmap, srcRect, dstRect, skPaint);
    }

    public void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
    {
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawLine(x1, y1, x2, y2, skPaint);
    }

    public void DrawRectangle(Pen pen, int x1, int y1, int w, int h)
    {
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawRect(x1, y1, w, h, skPaint);
    }

    public void DrawEllipse(Pen pen, int x, int y, int w, int h)
    {
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        canvas.DrawOval(new SKRect(x, y, x + w, y + h), skPaint);
    }

    public void DrawArc(Pen pen, int x, int y, int w, int h, float sw, float swa)
    {
        skPaint.Color = pen.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Stroke;
        skPaint.StrokeWidth = pen.Width;
        var rect = new SKRect(x, y, x + w, y + h);
        canvas.DrawArc(rect, sw, swa, false, skPaint);
    }

    public void FillRectangle(Brush brush, int x1, int y1, int w, int h)
    {
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.Style = SKPaintStyle.Fill;
        skPaint.StrokeWidth = 1;

        canvas.DrawRect(x1, y1, w, h, skPaint);
    }

    public void FillPolygon(Brush brush, Point[] points)
    {
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
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.TextSize = font.Size;
        skPaint.Typeface = font.GetSKTypeface();
        skPaint.Style = SKPaintStyle.Fill;

        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var bounds = new SKRect();
            //skPaint.MeasureText(line, ref bounds);
            //y += (int)bounds.Height;
            y += (int)-skPaint.FontMetrics.Ascent;

            canvas.DrawText(line, x, y, SKTextAlign.Left, skPaint.ToFont(), skPaint);
            y += (int)skPaint.FontMetrics.Descent;
        }
    }

    public void DrawString(string text, Font font, Brush brush, RectangleF rect)
    {        
        skPaint.Color = brush.Color.ToSKColor();
        skPaint.TextSize = font.Size;
        skPaint.Typeface = font.GetSKTypeface();
        skPaint.Style = SKPaintStyle.Fill;

        float lineHeight = skPaint.FontMetrics.Descent - skPaint.FontMetrics.Ascent;
        float y = rect.Top - skPaint.FontMetrics.Ascent;
        float maxWidth = rect.Width;

        var multiLine = new MultiLine((int)rect.Width, MeasureTextCRLF, text);
        DrawString(multiLine.Text, font, brush, rect.X, rect.Y);
    }

    public Size MeasureStringWidth(string text, Font font, int width)
    {
        skPaint.TextSize = font.Size;
        skPaint.Typeface = font.GetSKTypeface();

        skPaint.TextSize = font.Size;
        skPaint.Typeface = font.GetSKTypeface();
        skPaint.Style = SKPaintStyle.Fill;

        float lineHeight = skPaint.FontMetrics.Descent - skPaint.FontMetrics.Ascent;

        if (width == 0)
            return new Size(0, 0);

        // Shortcut for single line text
        var simpleSize = MeasureTextCRLF(text);
        if (simpleSize.Width < width)
            return simpleSize;

        var multiLine = new MultiLine(width,  MeasureTextCRLF, text);

        return multiLine.Size;
    }

    internal Size MeasureTextCRLF(string text)
    {
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        int maxWidth = 0;
        int totalHeight = 0;

        foreach (var line in lines)
        {
            var bounds = new SKRect();
            skPaint.MeasureText(line, ref bounds);
            maxWidth = Math.Max(maxWidth, (int)bounds.Width);
            totalHeight += (int)bounds.Height;
        }

        return new Size(maxWidth, totalHeight);
    }

    public Size MeasureString(string text, Font font)
    {
        skPaint.TextSize = font.Size;
        skPaint.Typeface = font.GetSKTypeface();

        var bounds = new SKRect();
        skPaint.MeasureText(text, ref bounds);

        var y = skPaint.FontMetrics.Descent - skPaint.FontMetrics.Ascent;

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
