namespace Fleux.UIElements
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using Core;
    using Core.GraphicsHelpers;

    public class ImageElement : UIElement
    {
        private Image image;

        public ImageElement(string resourceName)
            : this(ResourceManager.Instance.GetBitmapFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly()))
        {
        }

        public ImageElement(Image image)
        {
            this.image = image;
            this.Size = image.Size;
        }

        public Image Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.DrawImage(this.image, 0, 0, Size.Width, Size.Height);
        }
    }
}
