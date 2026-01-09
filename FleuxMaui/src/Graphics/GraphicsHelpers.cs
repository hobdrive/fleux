using System.IO;
using SkiaSharp;

namespace System.Drawing.Imaging
{
    public class ImageAttributes
    {
        ColorMatrix cm;
        public void SetColorMatrix(ColorMatrix cm)
        {
            this.cm = cm;
        }
        public ColorMatrix GetColorMatrix()
        {
            return cm;
        }
    }
    public class ColorMatrix
    {
        float[][] matrix;
        public float[][] Matrix{
            get{ return matrix; }
        }
        public ColorMatrix(float[][] matrix)
        {
            this.matrix = matrix;
        }
    }

}

    
namespace System.Drawing
{

    public enum GraphicsUnit
    {
        Pixel,
    }

    public static class ColorHelper
    {
        public static SKColor ToSKColor(this Color c)
        {
            return new SKColor(c.R, c.G, c.B, c.A);
        }

        public static Color ToSDColor(this SKColor c)
        {
            return Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue);
        }

    }

    public abstract class Image : IDisposable
    {
        protected SKImage skImage;
        //protected SKBitmap skBitmap;
        protected SKSurface OffscreenSurface;

        // Old snapshot waiting for GPU to finish
        private SKImage pendingDisposal;

        // Track if we need new snapshot
        private bool snapshotInvalidated = false;

        public abstract int Width{get; protected set;}
        public abstract int Height{get; protected set;}

        public Size Size{ get{ return new Size(Width, Height); }}

        public SKSurface GetSurface()
        {
            return OffscreenSurface;
        }

        // Returns true if this is a static/immutable image (from resources)
        // Static images should NOT be disposed by caller
        public bool IsStaticImage()
        {
            return skImage != null && OffscreenSurface == null;
        }

        // Get SKImage - returns an owned reference (DO NOT DISPOSE by caller)
        // The Image object owns this SKImage and will dispose it
        public SKImage GetSkImage()
        {
            if (IsStaticImage())
                return skImage;

            if (OffscreenSurface != null)
            {
                // For surfaces, create new snapshot if invalidated or none exists
                if (skImage == null || snapshotInvalidated)
                {
                    // CRITICAL: Flush first to ensure GPU commands complete
                    OffscreenSurface.Canvas.Flush();
                    
                    // Dispose any previously pending snapshot - should be safe now
                    pendingDisposal?.Dispose();
                    pendingDisposal = null;
                    
                    // Store current snapshot for deferred disposal
                    pendingDisposal = skImage;
                    
                    // Create new snapshot
                    skImage = OffscreenSurface.Snapshot();
                    snapshotInvalidated = false;
                    
                    // Don't dispose old snapshot yet - GPU might still be using it
                    // It will be disposed on NEXT snapshot creation or final Dispose()
                }
                return skImage;
            }
            throw new Exception("Bitmap has no SKImage");
        }

        // Call this when the surface has been modified to invalidate cached snapshot
        public void InvalidateSnapshot()
        {
            // Just mark as invalidated - actual disposal happens in GetSkImage()
            // This prevents disposing while GPU might still be using it
            snapshotInvalidated = true;
        }

        public virtual void Dispose()
        {
            /*
            if (skBitmap != null)
            {
                skBitmap.Dispose();
                skBitmap = null;
            }
            */
            if (skImage != null && !IsStaticImage())
            {
                // Only dispose snapshots, not static images
                var temp = skImage;
                skImage = null;
                temp?.Dispose();
            }
            // Also dispose any pending snapshot
            if (pendingDisposal != null)
            {
                pendingDisposal.Dispose();
                pendingDisposal = null;
            }
            if (OffscreenSurface != null)
            {
                OffscreenSurface.Dispose();
                OffscreenSurface = null;
            }
        }
	}

    public class Bitmap : Image
    {
        // Constructor for immutable resources
        public Bitmap(SKImage image)
        {
            skImage = image;
            Width = image.Width;
            Height = image.Height;
        }

        public Bitmap(GRRecordingContext grContext, int w, int h)
        {
            this.Width = w;
            this.Height = h;
            var surfaceInfo = new SKImageInfo(
                w, 
                h,
                SKColorType.Rgba8888,
                SKAlphaType.Premul
            );
            OffscreenSurface = SKSurface.Create(grContext, false, surfaceInfo);
        }

        public Bitmap(int w, int h)
        {
            this.Width = w;
            this.Height = h;

            var info = new SKImageInfo(w, h, SKColorType.Rgba8888, SKAlphaType.Premul);
            OffscreenSurface = SKSurface.Create(info);
        }

        public Bitmap(MemoryStream ms)
        {
            throw new Exception("create bitmap from MemoryStream");
        }
        public Bitmap(Stream rs)
        {
            throw new Exception("create bitmap from Stream");
        }

        public Bitmap(string file)
        {
            throw new Exception("create bitmap from file");
        }

        public void Clear(Color color)
        {
            throw new Exception("Bitmap Clear");
        }

        public override int Width
        {
            get; protected set;
        }
        public override int Height
        {
            get; protected set;
        }
    }

    public class Pen : IDisposable
    {
        public Color Color;
        public int Width;

        public Pen(Color c)
        {
            this.Color = c;
            this.Width = 1;
        }

        public Pen(Color c, int width)
        {
            this.Color = c;
            this.Width = width;
        }
        public void Dispose()
        {
        }
    }


    public class Brush : IDisposable
    {
        public Color Color;

        public Brush(Color c)
        {
            this.Color = c;
        }
        public void Dispose()
        {
        }
    }

    public class SolidBrush : Brush
    {
        public SolidBrush(Color c) : base(c)
        {
        }
    }

    public enum FontStyle
    {
        Regular,
        Bold,
        Italic,
    }
    
    public enum NamedSize
    {
        Default,
        Small,
        Medium,
        Large,
    }

    public class Font : IDisposable
    {

        public string Name;
        public int Size;
        public FontStyle Style;

        public Font(string face, int size, FontStyle style)
        {
            Size = size;
            Name = face;
            Style = style;
        }

        public void Dispose()
        {
        }

        public static Font? SystemFontOfSize(object size)
        {
            if (size is NamedSize namedSize)
            {
                switch (namedSize)
                {
                    case NamedSize.Small:
                        return new Font("Arial", 10, FontStyle.Regular);
                    case NamedSize.Medium:
                        return new Font("Arial", 12, FontStyle.Regular);
                    case NamedSize.Large:
                        return new Font("Arial", 14, FontStyle.Regular);
                    default:
                        return new Font("Arial", 12, FontStyle.Regular);
                }
            }
            else if (size is int intSize)
            {
                return new Font("Arial", intSize, FontStyle.Regular);
            }
            else
                return null;
        }

        public SKTypeface GetSKTypeface()
        {
            return SKTypeface.FromFamilyName(Name, (Style == FontStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                (Style == FontStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
        }
        
        public SKFont ToSKFont()
        {
            return new SKFont(GetSKTypeface(), Size);
        }

    }

}