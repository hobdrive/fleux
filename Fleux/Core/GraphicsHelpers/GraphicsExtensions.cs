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
#if WINCE
        /// for code compatibility
        public static void Flush(this Graphics gr)
        {
        }
#endif

        public static Graphics DrawPng(this Graphics gr, IImageWrapper pngImage, Rectangle destRect)
        {
#if WINCE
            var hDc = gr.GetHdc();
            pngImage.Draw(hDc, destRect, new Rectangle(0, 0, pngImage.Size.Width, pngImage.Size.Height));
            gr.ReleaseHdc(hDc);
#endif
#if __ANDROID__
            if (pngImage.Image is AImage)
            {
                gr.DrawImage( ((AImage)pngImage.Image).bitmap,
                    destRect, new Rectangle(0, 0, pngImage.Size.Width, pngImage.Size.Height),
                    GraphicsUnit.Pixel);
            }
#endif
#if WIN32
            if (pngImage.Image is Win32Image)
            {
                gr.DrawImage( ((Win32Image)pngImage.Image).bitmap,
                              destRect, new Rectangle(0, 0, pngImage.Size.Width, pngImage.Size.Height),
                              GraphicsUnit.Pixel);
            }
#endif
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
#if WINCE
            byte bopacity = unchecked((byte)(255 * opacity));

            using (Graphics gxSrc = Graphics.FromImage(b))
            {
              IntPtr hdcDst = gr.GetHdc();
              IntPtr hdcSrc = gxSrc.GetHdc();
              BlendFunction blendFunction = new BlendFunction();
              blendFunction.BlendOp = (byte)BlendOperation.AC_SRC_OVER;
              blendFunction.BlendFlags = (byte)BlendFlags.Zero;
              blendFunction.SourceConstantAlpha = bopacity;
              blendFunction.AlphaFormat = (byte)0;
              try{
                  PlatformAPI.AlphaBlend(hdcDst, to.X, to.Y, to.Width, to.Height, hdcSrc, from.X, from.Y, from.Width, from.Height, blendFunction);
              }catch(Exception e){}
              gr.ReleaseHdc(hdcDst);
              gxSrc.ReleaseHdc(hdcSrc);
            }
#else
            var ia = new ImageAttributes();
            float[][] ptsArray = {
                        new float[] {1, 0, 0, 0, 0},
                        new float[] {0, 1, 0, 0, 0},
                        new float[] {0, 0, 1, 0, 0},
                        new float[] {0, 0, 0, opacity, 0}, 
                        new float[] {0, 0, 0, 0, 1}}; 
            ia.SetColorMatrix(new ColorMatrix(ptsArray));
            gr.DrawImage(b, to, from.X, from.Y, from.Width, from.Height, GraphicsUnit.Pixel, ia);
#endif
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
#if WINCE
    public class PlatformAPI
    {
      [DllImport("coredll.dll")]
      extern public static Int32 AlphaBlend(IntPtr hdcDest, Int32 xDest, Int32 yDest, Int32 cxDest, Int32 cyDest, IntPtr hdcSrc, Int32 xSrc, Int32 ySrc, Int32 cxSrc, Int32 cySrc, BlendFunction blendFunction);         
    }
#endif
}
