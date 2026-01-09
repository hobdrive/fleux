namespace Fleux.Core.NativeHelpers
{
    using System;
    using System.Drawing;
    using GraphicsHelpers;

    /// <summary>
    /// TODO: I see not reasoning in this class, suggest to cut it out
    /// </summary>
    [Obsolete("Use SkiaSharp based image implementation")]
    public class IImageWrapper : IDisposable
    {
        public IImage Image;
        public ImageInfo Info;
        public Size Size;
        public Size PhysicalDimension;

        private readonly SizeF physicalFactors;
        private Rectangle bounds;

        public IImageWrapper(IImage image)
        {
            this.Image = image;
            this.Info = new ImageInfo();
            image.GetImageInfo(out this.Info);

            // bounds should be always at location (0,0)
            this.bounds = new Rectangle(0, 0, (int)this.Info.Width, (int)this.Info.Height);
            this.Size = this.bounds.Size;
            image.GetPhysicalDimension(out this.PhysicalDimension);
            this.physicalFactors.Width = (float)this.PhysicalDimension.Width / this.Size.Width;
            this.physicalFactors.Height = (float)this.PhysicalDimension.Height / this.Size.Height;
        }

        public void Dispose()
        {
            this.Image.Release();
        }
    }
}