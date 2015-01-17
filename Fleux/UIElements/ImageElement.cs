using Fleux.Core.NativeHelpers;

namespace Fleux.UIElements
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using Core;
    using Core.GraphicsHelpers;
    using Core.Scaling;

    public class ImageElement : UIElement
    {
        private IImageWrapper image;
        Size ImageSize;
        bool KeepAspect = true;

        public ImageElement(string resourceName) : this(resourceName, true)
        {
        }

        public ImageElement(string resourceName, bool keepAspect)
        {
            this.image = ResourceManager.Instance.GetIImage(resourceName);
            KeepAspect = keepAspect;

            this.ImageSize = new Size(image.Size.Width.ToLogic(), image.Size.Height.ToLogic());
            this.Size = ImageSize;
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            var size = Size;
            var trect = new Rectangle(0,0, size.Width, size.Height);

            if (KeepAspect)
            {
                double scale = (double)ImageSize.Width / ImageSize.Height;
                size = new Size(Size.Width, (int)(Size.Width / scale));
                if (size.Height > Size.Height)
                    size = new Size((int)(Size.Height * scale), Size.Height);
                trect = new Rectangle((Size.Width - size.Width)/2,
                                     (Size.Height - size.Height)/2,
                                     size.Width, size.Height);
            }

            drawingGraphics.DrawAlphaImage(this.image, trect);
        }
    }
}
