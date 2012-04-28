namespace Fleux.Core.Scaling
{
    using System.Drawing;

    public static class ImagingExtensions
    {
        public static Bitmap Scaled(this Bitmap source)
        {
            var bmp = new Bitmap(source.Width.ToPixels(), source.Height.ToPixels());
            using (var gr = Graphics.FromImage(bmp))
            {
                gr.DrawImage(source,
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    new Rectangle(0, 0, source.Width, source.Height),
                    GraphicsUnit.Pixel);
            }
            return bmp;
        }
    }
}
