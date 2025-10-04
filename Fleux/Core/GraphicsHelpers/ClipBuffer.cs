namespace Fleux.Core.GraphicsHelpers
{
    using System;
    using System.Drawing;

    public class ClipBuffer : IDisposable
    {
        private readonly Image image;
        private readonly Rectangle region;
        private readonly Graphics ownerGraphics;
        private readonly Graphics graphics;
        private readonly DrawingGraphics drawingGraphics;

        public ClipBuffer(Image image, Graphics imageGraphics, Rectangle region, Graphics ownerGraphics)
        {
            this.image = image;
            this.region = region;
            this.ownerGraphics = ownerGraphics;
            this.graphics = imageGraphics;
            this.drawingGraphics = DrawingGraphics.FromGraphicsAndRect(this.graphics, 
                this.image, 
                new Rectangle(0, 0, this.image.Width, this.image.Height));
        }

        public DrawingGraphics DrawingGr
        {
            get
            {
                return this.drawingGraphics;
            }
        }

        public void Dispose()
        {
            this.Apply();
            this.graphics.Dispose();
        }

        public void Apply()
        {
            graphics.Flush();
            var srcRect = new Rectangle(0, 0, this.region.Width, this.region.Height);
            this.ownerGraphics.DrawImage(this.image, this.region.X, this.region.Y, srcRect, GraphicsUnit.Pixel);
            //this.ownerGraphics.DrawImage(this.image, this.region.X, this.region.Y);
        }
    }
}
