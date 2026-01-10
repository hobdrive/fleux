using Fleux.Core.NativeHelpers;
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

        public static IDrawingGraphics FillRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int r)
        {
            return FillRoundedRectangle(dg, rect, r,r,r,r);
        }

        public static IDrawingGraphics DrawRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int r)
        {
            return DrawRoundedRectangle(dg, rect, r,r,r,r);
        }

        /// <summary>
        /// Fills the rounded rectangle with roundings in "roundings"
        /// </summary>
        public static IDrawingGraphics FillRoundedRectangle(this IDrawingGraphics dg, System.Drawing.Rectangle rect, int l, int r, int lb, int rb)
        {
            int ix, iy, ix2, iy2;
            ix = rect.X + Math.Max(l, lb);
            iy = rect.Y + Math.Max(l, r);
            ix2 = rect.Right - Math.Max(r, rb);
            iy2 = rect.Bottom - Math.Max(lb, rb);
                
            dg.FillEllipse(rect.X, rect.Y, ix+l, iy+l);
            dg.FillEllipse(rect.X, rect.Bottom, ix+lb, iy2-lb);
            dg.FillEllipse(ix2-r, rect.Y, rect.Right, iy+r);
            dg.FillEllipse(rect.Right, rect.Bottom, ix2-rb, iy2-rb);

            dg.FillRectangle(ix, iy, ix2, iy2);

            dg.FillRectangle(ix, rect.Y, ix2, iy+1);
            dg.FillRectangle(rect.X, iy, ix+1, iy2);
            dg.FillRectangle(ix, iy2-1, ix2, rect.Bottom);//!!!!
            dg.FillRectangle(ix2-1, iy, rect.Right, iy2);

            return dg;
        }

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
