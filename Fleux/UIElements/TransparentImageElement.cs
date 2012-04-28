namespace Fleux.UIElements
{
    using System;
    using Core.GraphicsHelpers;
    using Core.NativeHelpers;

    public class TransparentImageElement : UIElement
    {
        private IImageWrapper image;

        public TransparentImageElement(IImageWrapper image)
        {
            this.image = image;
            this.Size = image.Size;
        }

        public IImageWrapper Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.DrawAlphaImage(this.image, 0, 0, Size.Width, Size.Height);
        }
    }
}