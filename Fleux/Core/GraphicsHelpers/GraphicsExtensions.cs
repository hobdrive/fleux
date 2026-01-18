using Fleux.Core.NativeHelpers;
#if __SKIA__
using SkiaSharp;
#endif

namespace Fleux.Core.GraphicsHelpers
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using Core.NativeHelpers;

    public static class GraphicsExtensions
    {
        public static Rectangle Clone(this Rectangle r)
        {
            return new Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static Rectangle TransformToBounds(this Rectangle r)
        {
            return new Rectangle(r.Left, r.Top, r.Right, r.Bottom);
        }

        public static Rectangle Translate(this Rectangle r, int x, int y)
        {
            return new Rectangle(r.Left+x, r.Top+y, r.Width, r.Height);
        }

#if __SKIA__
        public static IDrawingGraphics FillRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int r)
        {
            return FillRoundedRectangle(dg, rect, r,r,r,r);
        }
#endif

        public static IDrawingGraphics DrawRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int r)
        {
            return DrawRoundedRectangle(dg, rect, r,r,r,r);
        }

#if __SKIA__
        /// <summary>
        /// Fills the rounded rectangle with roundings in "roundings"
        /// </summary>
        public static IDrawingGraphics FillRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int l, int r, int lb, int rb)
        {
            // Use Skia path for uniform filling without pixel-matching issues
            using (var path = new SKPath())
            {
                // Calculate transformed coordinates
                float x = dg.CalculateX(rect.X);
                float y = dg.CalculateY(rect.Y);
                float right = dg.CalculateX(rect.Right);
                float bottom = dg.CalculateY(rect.Bottom);

                // Calculate scaled corner radii (using CalculateWidth for scaling)
                float lt = dg.CalculateWidth(l);
                float rt = dg.CalculateWidth(r);
                float lbr = dg.CalculateWidth(lb);
                float rbr = dg.CalculateWidth(rb);

                // Create rounded rectangle path with proper corner arcs
                path.MoveTo(x + lt, y);
                path.LineTo(right - rt, y);
                path.ArcTo(new SKRect(right - rt * 2, y, right, y + rt * 2), -90, 90, false);
                path.LineTo(right, bottom - rbr);
                path.ArcTo(new SKRect(right - rbr * 2, bottom - rbr * 2, right, bottom), 0, 90, false);
                path.LineTo(x + lbr, bottom);
                path.ArcTo(new SKRect(x, bottom - lbr * 2, x + lbr * 2, bottom), 90, 90, false);
                path.LineTo(x, y + lt);
                path.ArcTo(new SKRect(x, y, x + lt * 2, y + lt * 2), 180, 90, false);
                path.Close();

                // Fill the path using Skia canvas directly
                dg.Graphics.Paint.Color = dg.State.CurrentBrush.Color.ToSKColor();
                dg.Graphics.Paint.Style = SKPaintStyle.Fill;
                dg.Graphics.Canvas.DrawPath(path, dg.Graphics.Paint);
            }

            return dg;
        }
#endif

        /// <summary>
        /// Fills the rounded rectangle with roundings in "roundings"
        /// </summary>
        public static IDrawingGraphics DrawRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int lt, int rt, int lb, int rb)
        {
            int ix, iy, ix2, iy2;
            ix = rect.X + Math.Max(lt, lb);
            iy = rect.Y + Math.Max(lt, rt);
            ix2 = rect.Right - Math.Max(rt, rb);
            iy2 = rect.Bottom - Math.Max(lb, rb);

            dg.DrawArc(rect.X, rect.Y, ix+lt, iy+lt, (float)Math.PI, (float)Math.PI/2);
            dg.DrawArc(rect.X, rect.Bottom, ix+lb, iy2-lb, (float)Math.PI, -(float)Math.PI/2);
            dg.DrawArc(ix2-rt, rect.Y, rect.Right, iy+rt, (float)0, -(float)Math.PI/2);
            dg.DrawArc(rect.Right, rect.Bottom, ix2-rb, iy2-rb, 0, (float)Math.PI/2);

            dg.DrawLine(ix, rect.Y, ix2, rect.Y);
            dg.DrawLine(ix, rect.Bottom, ix2, rect.Bottom);

            dg.DrawLine(rect.X, rect.Y+lt, rect.X, rect.Bottom-lb);
            dg.DrawLine(rect.Right, rect.Y+rt, rect.Right, rect.Bottom-rb);

            return dg;
        }

        public static Graphics DrawPng(this Graphics gr, IImageWrapper pngImage, Rectangle destRect)
        {
            return DrawPng(gr, pngImage, destRect, new Rectangle(0, 0, pngImage.Size.Width, pngImage.Size.Height));
        }

        public static Graphics DrawPng(this Graphics gr, IImageWrapper pngImage, Rectangle destRect, Rectangle sourceRect)
        {
#if __ANDROID__ && !__SKIA__
            if (pngImage.Image is AImage)
            {
                gr.DrawImage( ((AImage)pngImage.Image).bitmap,
                    destRect, sourceRect,
                    GraphicsUnit.Pixel);
                return gr;
            }
#endif
            if (pngImage.Image is BasicImage)
            {
                gr.DrawImage( ((BasicImage)pngImage.Image).bitmap,
                              destRect, sourceRect,
                              GraphicsUnit.Pixel);
            }
            return gr;
        }

        public static Graphics DrawPng(this Graphics gr, string resourceName, Rectangle destRect)
        {
            return DrawPng(gr, 
                           ResourceManager.Instance.GetIImageFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly()), 
                           destRect);
        }

        public static Graphics AlphaBlend(this Graphics gr, Bitmap b, Rectangle to, Rectangle from, float opacity)
        {
            var ia = new ImageAttributes();
            float[][] ptsArray = {
                        new float[] {1, 0, 0, 0, 0},
                        new float[] {0, 1, 0, 0, 0},
                        new float[] {0, 0, 1, 0, 0},
                        new float[] {0, 0, 0, opacity, 0}, 
                        new float[] {0, 0, 0, 0, 1}}; 
            ia.SetColorMatrix(new ColorMatrix(ptsArray));
            gr.DrawImage(b, to, from.X, from.Y, from.Width, from.Height, GraphicsUnit.Pixel, ia);

            return gr;
        }
    }

    public struct BlendFunction
    {
      public byte BlendOp;
      public byte BlendFlags;
      public byte SourceConstantAlpha;
      public byte AlphaFormat;
    }

    public enum BlendOperation : byte
    {
      AC_SRC_OVER = 0x00
    }

    public enum BlendFlags : byte
    {
      Zero = 0x00
    }

    public enum SourceConstantAlpha : byte
    {
      Transparent = 0x00,
      Opaque = 0xFF
    }

    public enum AlphaFormat : byte
    {
      AC_SRC_ALPHA = 0x01
    }
}
