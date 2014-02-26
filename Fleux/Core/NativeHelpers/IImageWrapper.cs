namespace Fleux.Core.NativeHelpers
{
    using System;
    using System.Drawing;
    using GraphicsHelpers;

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

        /// <summary>
        /// Draws the Image at location
        /// </summary>
        /// <param name="hDc">Graphics Device Context Handler</param>
        /// <param name="location">Absolute position for the full Image draw</param>
        /// <param name="clipRect">Absolute clip rect</param>
        /// <returns>HResult</returns>
        public uint Draw(IntPtr hDc, Point location, Rectangle clipRect)
        {
            try
            {
                var destRect = this.bounds.Clone();
                destRect.Location = location;
                destRect = destRect.TransformToBounds();
                return this.Image.Draw(hDc, ref destRect, IntPtr.Zero);
            }
            catch
            {
                return 1;
            }
        }

        /// <summary>
        /// Draws the Image at destRect scaling if needed
        /// </summary>
        /// <param name="hDc">Graphics Device Context Handler</param>
        /// <param name="destRect">Absolute destination Rect</param>
        /// <param name="clipRect">Absolute clip rect</param>
        /// <returns>HResult</returns>
        public uint Draw(IntPtr hDc, Rectangle destRect, Rectangle clipRect)
        {
            try
            {
                var localDestRectRect = destRect.TransformToBounds();
                return this.Image.Draw(hDc, ref localDestRectRect, IntPtr.Zero);
            }
            catch
            {
                return 1;
            }
        }

        public void Dispose()
        {
            this.Image.Release();
        }
    }
}