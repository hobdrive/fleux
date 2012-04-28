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
        private readonly IDrawingGraphics drawingGraphics;

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

        public IDrawingGraphics DrawingGr
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

        private void Apply()
        {
            this.ownerGraphics.DrawImage(this.image, this.region.X, this.region.Y);
        }
    }
}
