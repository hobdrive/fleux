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
        public abstract int Width{get; protected set;}
        public abstract int Height{get; protected set;}

        public Size Size{ get{ return new Size(Width, Height); }}

        public virtual void Dispose()
        {
            if (skBitmap != null)
            {
                skBitmap.Dispose();
            }
        }

        internal SKBitmap skBitmap;
	}

    public class Bitmap : Image
    {
        public Bitmap(SKBitmap texture)
        {
            skBitmap = texture;
            this.Width = texture.Width;
            this.Height = texture.Height;
        }

        public Bitmap(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            skBitmap = new SKBitmap(w, h);
        }

        public Bitmap(MemoryStream ms)
        {
            System.Console.WriteLine("create bitmap from MemoryStream");
        }
        public Bitmap(Stream rs)
        {
            System.Console.WriteLine("create bitmap from Stream");
        }

        public Bitmap(string file)
        {
            System.Console.WriteLine("create bitmap from file");
        }

        public void Clear(Color color)
        {
            System.Console.WriteLine("Bitmap Clear");
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