namespace Fleux.Core.GraphicsHelpers
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows.Forms;
    using Core.NativeHelpers;
    using Dim;
    using Scaling;
    using Styles;
    using UIElements;

    public class DrawingGraphics : IDrawingGraphics
    {
        // The state (not scaled) used for the drawing primitives
        public readonly DrawingHelperState state;

        // ScalingTransformation fields
        private double scalingFactorFromParent = 1.0;
        private double scalingFactor = 1.0;

        // Rectangle (pixels) enclosing the content
        private Rectangle drawingExtends;

        private Point location;

        private Image canvasImage;

        public DrawingGraphics(Graphics gr, Image canvasImage)
        {
            this.Graphics = gr;
            this.canvasImage = canvasImage;
            this.state = new DrawingHelperState(this.ValidateExtendsFromLogic);
            this.drawingExtends = new Rectangle();
            this.MaxWidth = canvasImage.Width;
            this.MaxHeight = canvasImage.Height;
        }

        // Expressed in pixels
        public int MaxWidth { get; set; }

        public int MaxHeight { get; set; }

        // Graphics where the actions will be performed
        public Graphics Graphics { get; private set; }

        // Transformations
        //----------------
        // Scaling
        public double TransformationScaling
        {
            get { return this.scalingFactor; }
            set { this.scalingFactor = value; }
        }

        public Point TransformationCenter { get; set; }

        // Rectangle to be used on the Graphics
        // Expressed in pixels
        public Rectangle ScaledBounds
        {
            get
            {
                return new Rectangle(this.drawingExtends.Left, this.drawingExtends.Top, FleuxApplication.ScaleFromLogic(this.MaxWidth), this.drawingExtends.Height);
            }

            set
            {
            }
        }

        public int X
        {
            get { return this.state.CurrentX; }
        }

        public int Y
        {
            get { return this.state.CurrentY; }
        }

        public Point Location
        {
            get
            {
                return this.location;
            }

            set
            {
                this.location = value;
                this.drawingExtends = new Rectangle(value.X, value.Y, this.drawingExtends.Width, this.drawingExtends.Height);
            }
        }

        public int Right
        {
            get { return FleuxApplication.ScaleToLogic(this.drawingExtends.Width); }
        }

        public int Width
        {
            get { return FleuxApplication.ScaleToLogic(this.MaxWidth - this.location.X); }
        }

        public int Bottom
        {
            get { return FleuxApplication.ScaleToLogic(this.drawingExtends.Bottom - this.location.Y); }
        }

        public Rectangle VisibleRect
        {
            // cail: wtf? useless clipping on high resolutions
            // cail: WTF2:
            get { return new Rectangle(Math.Max(0, -this.Location.X.ToLogic()), Math.Max(0,-this.Location.Y.ToLogic()), MaxWidth.ToLogic(), MaxHeight.ToLogic()); }
        }

        public static DrawingGraphics FromGraphicsAndRect(Graphics gr, Image canvasImage, Rectangle rect)
        {
            return new DrawingGraphics(gr, canvasImage)
            {
                MaxWidth = rect.Width,
                MaxHeight = rect.Height,
                Location = rect.Location,
            };
        }

        public static DrawingGraphics FromPaintEventArgs(PaintEventArgs e, Image canvasImage)
        {
            return DrawingGraphics.FromGraphicsAndRect(e.Graphics, canvasImage, e.ClipRectangle);
        }

        public static DrawingGraphics FromGraphicsLocationMaxWidth(Graphics gr, Image canvasImage, int x, int y, int maxWidth)
        {
            return new DrawingGraphics(gr, canvasImage)
            {
                MaxWidth = maxWidth,
                Location = new Point(x, y),
            };
        }

        public IDrawingGraphics Color(System.Drawing.Color color)
        {
            this.state.Color = color;
            return this;
        }

        public IDrawingGraphics PenWidth(int width)
        {
            this.state.PenWidth = FleuxApplication.ScaleFromLogic(width);
            return this;
        }

        public IDrawingGraphics FontSize(int size)
        {
            this.state.FontSize = size;
            return this;
        }

        public IDrawingGraphics FontName(string fontName)
        {
            this.state.FontName = fontName;
            return this;
        }

        public IDrawingGraphics Bold(bool enable)
        {
            this.state.SetFontStyle(FontStyle.Bold, enable);
            return this;
        }

        public IDrawingGraphics Italic(bool enable)
        {
            this.state.SetFontStyle(FontStyle.Italic, enable);
            return this;
        }

        public IDrawingGraphics Style(TextStyle style)
        {
            style.ApplyTo(this);
            return this;
        }

        public IDrawingGraphics MoveTo(int x, int y)
        {
            this.state.CurrentX = x;
            this.state.CurrentY = y;
            return this;
        }

        public IDrawingGraphics MoveX(int x)
        {
            this.state.CurrentX = x;
            return this;
        }

        public IDrawingGraphics MoveY(int y)
        {
            this.state.CurrentY = y;
            return this;
        }

        public IDrawingGraphics MoveRelX(int dx)
        {
            this.state.CurrentX += dx;
            return this;
        }

        public IDrawingGraphics MoveRelY(int dy)
        {
            this.state.CurrentY += dy;
            return this;
        }

        public IDrawingGraphics MoveRel(int dx, int dy)
        {
            this.state.CurrentX += dx;
            this.state.CurrentY += dy;
            return this;
        }

        public IDrawingGraphics DrawLine(int x1, int y1, int x2, int y2)
        {
            this.Graphics.DrawLine(this.state.CurrentPen, this.CalculateX(x1), this.CalculateY(y1), this.CalculateX(x2), this.CalculateY(y2));
            this.MoveTo(x2, y2);
            this.ValidateExtends(this.CalculateX(x2), this.CalculateY(y2));
            return this;
        }

        public IDrawingGraphics DrawLineTo(int x2, int y2)
        {
            return this.DrawLine(this.state.CurrentX, this.state.CurrentY, x2, y2);
        }

        public IDrawingGraphics DrawRectangle(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Swap(ref y1, ref y2);
            }
            this.Graphics.DrawRectangle(this.state.CurrentPen,
                                        this.CalculateX(x1),
                                        this.CalculateY(y1),
                                        this.CalculateWidth(x2 - x1),
                                        this.CalculateHeight(y2 - y1));
            this.ValidateExtends(this.CalculateX(x2), this.CalculateY(y2));
            return this;
        }

        public IDrawingGraphics DrawRectangle(System.Drawing.Rectangle r)
        {
            this.Graphics.DrawRectangle(this.state.CurrentPen,
                                        this.CalculateX(r.X),
                                        this.CalculateY(r.Y),
                                        this.CalculateWidth(r.Width),
                                        this.CalculateHeight(r.Height));
            this.ValidateExtends(this.CalculateX(r.Right), this.CalculateY(r.Bottom));
            return this;
        }

        public IDrawingGraphics FillRectangle(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Swap(ref y1, ref y2);
            }
            this.Graphics.FillRectangle(this.state.CurrentBrush,
                                        this.CalculateX(x1),
                                        this.CalculateY(y1),
                                        this.CalculateWidth(x2 - x1),
                                        this.CalculateHeight(y2 - y1));
            this.ValidateExtends(this.CalculateX(x2), this.CalculateY(y2));
            return this;
        }

        public IDrawingGraphics FillRectangle(System.Drawing.Rectangle r)
        {
            this.Graphics.FillRectangle(this.state.CurrentBrush,
                                        this.CalculateX(r.X),
                                        this.CalculateY(r.Y),
                                        this.CalculateWidth(r.Width),
                                        this.CalculateHeight(r.Height));
            this.ValidateExtends(this.CalculateX(r.Right), this.CalculateY(r.Bottom));
            return this;
        }

        public IDrawingGraphics DrawEllipse(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Swap(ref y1, ref y2);
            }
            this.Graphics.DrawEllipse(this.state.CurrentPen,
                                      this.CalculateX(x1),
                                      this.CalculateY(y1),
                                      this.CalculateWidth(x2 - x1),
                                      this.CalculateHeight(y2 - y1));
            this.ValidateExtends(this.CalculateX(x2), this.CalculateY(y2));
            return this;
        }

        public IDrawingGraphics DrawEllipse(System.Drawing.Rectangle r)
        {
            this.Graphics.DrawEllipse(this.state.CurrentPen,
                                      this.CalculateX(r.X),
                                      this.CalculateY(r.Y),
                                      this.CalculateWidth(r.Width),
                                      this.CalculateHeight(r.Height));
            this.ValidateExtends(this.CalculateX(r.Right), this.CalculateY(r.Bottom));
            return this;
        }

        public IDrawingGraphics FillEllipse(int x1, int y1, int x2, int y2)
        {
            if (x1 > x2)
            {
                Swap(ref x1, ref x2);
            }
            if (y1 > y2)
            {
                Swap(ref y1, ref y2);
            }
            this.Graphics.FillEllipse(this.state.CurrentBrush,
                                      this.CalculateX(x1),
                                      this.CalculateY(y1),
                                      this.CalculateWidth(x2 - x1),
                                      this.CalculateHeight(y2 - y1));
            this.ValidateExtends(this.CalculateX(x2), this.CalculateY(y2));
            return this;
        }

        public IDrawingGraphics FillEllipse(System.Drawing.Rectangle r)
        {
            this.Graphics.FillEllipse(this.state.CurrentBrush,
                                      this.CalculateX(r.X),
                                      this.CalculateY(r.Y),
                                      this.CalculateWidth(r.Width),
                                      this.CalculateHeight(r.Height));
            this.ValidateExtends(this.CalculateX(r.Right), this.CalculateY(r.Bottom));
            return this;
        }

        public IDrawingGraphics DrawImage(System.Drawing.Image image, int x, int y)
        {
            return this.DrawImage(image, new Rectangle(x, y, image.Width, image.Height), new Rectangle(0, 0, image.Width, image.Height));
        }

        public IDrawingGraphics DrawImage(System.Drawing.Image image, int x, int y, int width, int height)
        {
            return this.DrawImage(image, new Rectangle(x, y, width, height), new Rectangle(0, 0, image.Width, image.Height));
        }

        public IDrawingGraphics DrawImage(System.Drawing.Image image, System.Drawing.Rectangle r)
        {
            return this.DrawImage(image, r, new Rectangle(0, 0, image.Width, image.Height));
        }

        public IDrawingGraphics DrawImage(System.Drawing.Image image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            var destScaledRect = this.CalculateRect(destRect);
            this.Graphics.DrawImage(image, destScaledRect, sourceRect, GraphicsUnit.Pixel);

            this.state.CurrentX = destRect.Right;
            this.ValidateExtends(0, destScaledRect.Bottom);
            return this;
        }

        public IDrawingGraphics DrawImage(string resourceName, int x, int y)
        {
            return this.DrawImage(ResourceManager.Instance.GetBitmapFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly()), x, y);
        }

        public IDrawingGraphics DrawImage(string resourceName, int x, int y, int width, int height)
        {
            this.Graphics.DrawImage(
                ResourceManager.Instance.GetBitmapFromEmbeddedResource(resourceName,
                                                                       width.ToPixels(),
                                                                       height.ToPixels(),
                                                                       Assembly.GetCallingAssembly()),
                this.CalculateX(x),
                this.CalculateY(y));

            this.state.CurrentX = this.CalculateX(x + width);
            this.ValidateExtends(this.state.CurrentX, this.CalculateY(y + height));
            return this;
        }

        public IDrawingGraphics DrawImage(string resourceName, System.Drawing.Rectangle r)
        {
            return this.DrawImage(resourceName, r.X, r.Y, r.Width, r.Height);
        }

        public IDrawingGraphics DrawImage(string resourceName, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            return
                this.DrawImage(
                    ResourceManager.Instance.GetBitmapFromEmbeddedResource(resourceName, Assembly.GetCallingAssembly()),
                    destRect,
                    sourceRect);
        }

        public IDrawingGraphics DrawImageFile(string fileName, int x, int y)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawImageFile(string fileName, int x1, int y1, int width, int height)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawImageFile(string fileName, System.Drawing.Rectangle r)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawImageFile(string fileName, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImage(IImageWrapper image, int x, int y)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImage(IImageWrapper image, int x1, int y1, int width, int height)
        {
            return this.DrawAlphaImage(image, new Rectangle(x1, y1, width, height));
        }

        public IDrawingGraphics DrawAlphaImage(IImageWrapper image, System.Drawing.Rectangle r)
        {
            var destScaledRect = this.CalculateRect(r);
            if (image != null)
            {
                this.Graphics.DrawPng(image, destScaledRect);
            }
            this.state.CurrentX = r.Right;
            this.ValidateExtends(0, destScaledRect.Bottom);
            return this;
        }

        public IDrawingGraphics DrawAlphaImage(IImageWrapper image, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImage(string resourceName, int x, int y)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImage(string resourceName, int x1, int y1, int width, int height)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImage(string resourceName, System.Drawing.Rectangle r)
        {
            return this.DrawAlphaImage(ResourceManager.Instance.GetIImageFromEmbeddedResource(resourceName, Assembly.GetExecutingAssembly()),
                                    r);
        }

        public IDrawingGraphics DrawAlphaImage(string resourceName, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImageFile(string fileName, int x, int y)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImageFile(string fileName, int x1, int y1, int width, int height)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImageFile(string fileName, System.Drawing.Rectangle r)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawAlphaImageFile(string fileName, System.Drawing.Rectangle destRect, System.Drawing.Rectangle sourceRect)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawText(string text)
        {
            this.Graphics.DrawString(text,
                                     this.state.CurrenFont,
                                     this.state.CurrentBrush,
                                     this.CalculateX(this.state.CurrentX),
                                     this.CalculateY(this.state.CurrentY));
            var measure = this.Graphics.MeasureString(text, this.state.CurrenFont);
            this.state.CurrentX += FleuxApplication.ScaleToLogic((int)measure.Width);
            this.ValidateExtends(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY) + (int)measure.Height);
            return this;
        }

        public IDrawingGraphics DrawText(string text, int width)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawTextEllipsis(string text, int width)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics DrawMultiLineText(string text, int width)
        {
            return this.DrawMultiLineText(text, 
                              width,
                              this.CalculateMultilineTextHeight(text, width));
        }

        public int CalculateMultilineTextHeight(string text, int width)
        {
            return StringHelpers.MeasureString(this.Graphics,
                                                                      this.state.CurrenFont,
                                                                      text,
                                                                      width.ToPixels()).Height.ToLogic();
        }

        public Size CalculateTextSize(string text)
        {
            var s = this.Graphics.MeasureString(text, this.state.CurrenFont);
            return new Size(((int)s.Width).ToLogic(), ((int)s.Height).ToLogic());
        }

        public IDrawingGraphics DrawMultiLineText(string text, int width, int height)
        {
            this.Graphics.DrawString(text,
                                     this.state.CurrenFont,
                                     this.state.CurrentBrush,
                                     new RectangleF(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY), this.CalculateWidth(width), this.CalculateHeight(height)));
            this.ValidateExtends(this.CalculateX(this.state.CurrentX + width), this.CalculateY(this.state.CurrentY + height));
            return this;
        }

         public IDrawingGraphics DrawCenterText(string text, int width)
         {
            var measure = this.Graphics.MeasureString(text, this.state.CurrenFont);
            int textOffset = (width - ((int)measure.Width).ToLogic()) / 2;

            this.Graphics.DrawString(text,
                                     this.state.CurrenFont,
                                     this.state.CurrentBrush,
                                     this.CalculateX(this.state.CurrentX + textOffset),
                                     this.CalculateY(this.state.CurrentY));
            this.state.CurrentX += width;
            this.ValidateExtends(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY) + (int)measure.Height);
            return this;
        }

        public IDrawingGraphics DrawCenterText(string text, int width, int height)
        {
            var measure = this.Graphics.MeasureString(text, this.state.CurrenFont);
            int textOffsetX = (width - ((int)measure.Width).ToLogic()) / 2;
            int textOffsetY = (height - ((int)measure.Height).ToLogic()) / 2;

            this.Graphics.DrawString(text,
                                     this.state.CurrenFont,
                                     this.state.CurrentBrush,
                                     this.CalculateX(this.state.CurrentX + textOffsetX),
                                     this.CalculateY(this.state.CurrentY + textOffsetY));
            this.state.CurrentX += width;
            this.ValidateExtends(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY) + (int)measure.Height);
            return this;
        }
        

        public IDrawingGraphics DrawRightText(string text)
        {
            throw new NotImplementedException();
        }

        public IDrawingGraphics CreateChild(Point innerlocation, double scalingTransformation, Point transformationCenter)
        {
            var location = innerlocation.ToPixels().ToParent(this.Location);
            var children = DrawingGraphics.FromGraphicsLocationMaxWidth(this.Graphics, this.canvasImage, location.X, location.Y, this.MaxWidth - location.X);
            children.scalingFactorFromParent = this.scalingFactorFromParent * this.scalingFactor;
            children.scalingFactor = scalingTransformation;
            children.TransformationCenter = transformationCenter;
            return children;
        }

        public IDrawingGraphics CreateChild(Point innerlocation)
        {
            return this.CreateChild(innerlocation, 1.0, new Point(0, 0));
        }

        public ClipBuffer GetClipBuffer(Rectangle region, Bitmap bitmap)
        {
            var realRegion = this.CalculateRect(region);
            var gr = Graphics.FromImage(bitmap);
            gr.DrawImage(this.canvasImage, 0, 0, realRegion, GraphicsUnit.Pixel);
            return new ClipBuffer(bitmap, gr, realRegion, this.Graphics);
        }

        protected int CalculateWidth(int logicalWidth)
        {
            return (int)(Math.Abs(logicalWidth).ToPixels() * this.scalingFactorFromParent * this.scalingFactor);
        }

        protected int CalculateHeight(int logicalHeight)
        {
            return (int)(Math.Abs(logicalHeight).ToPixels() * this.scalingFactorFromParent * this.scalingFactor);
        }

        public int CalculateX(int x)
        {
            return this.ScaledBounds.Left + 
                ((int)(this.TransformationCenter.X + ((x - this.TransformationCenter.X) * this.scalingFactorFromParent * this.scalingFactor))).ToPixels();
        }

        public int CalculateY(int y)
        {
            return this.ScaledBounds.Top +
                ((int)(this.TransformationCenter.Y + ((y - this.TransformationCenter.Y) * this.scalingFactorFromParent * this.scalingFactor))).ToPixels();
        }

        public Rectangle CalculateRect(Rectangle logicalRect)
        {
            return new Rectangle(this.CalculateX(logicalRect.X),
                                 this.CalculateY(logicalRect.Y),
                                 this.CalculateWidth(logicalRect.Width),
                                 this.CalculateHeight(logicalRect.Height));
        }

        private static void Swap(ref int v1, ref int v2)
        {
            var t = v1;
            v1 = v2;
            v2 = t;
        }

        /// <summary>
        /// Calculates the drawingExtends based on coordinates already scaled!
        /// </summary>
        /// <param name="x">X value in pixels (this value should be already scaled)</param>
        /// <param name="y">Y value in pixels (this value should be already scaled)</param>
        private void ValidateExtends(int x, int y)
        {
            var potentialWidth = x - this.drawingExtends.X;
            var potentialHeight = y - this.drawingExtends.Y;
            this.drawingExtends = new Rectangle(this.drawingExtends.X,
                                                this.drawingExtends.Y,
                                                Math.Max(this.drawingExtends.Width, potentialWidth),
                                                Math.Max(this.drawingExtends.Height, potentialHeight));
        }

        private void ValidateExtendsFromLogic(int x, int y)
        {
            this.ValidateExtends(this.CalculateX(x), this.CalculateY(y));
        }
    }
}
