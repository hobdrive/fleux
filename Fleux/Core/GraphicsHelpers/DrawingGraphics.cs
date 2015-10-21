using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing.Imaging;

using Fleux.Controls;
using Fleux.Core.NativeHelpers;
using Fleux.Core.Dim;
using Fleux.Core.Scaling;
using Fleux.Styles;

namespace Fleux.Core.GraphicsHelpers
{

    public class DGTransformation
    {
        public float Rotation = 0;
        public Point RotationCenter = Point.Empty;

        public float ScalingX = 1, ScalingY = 1;
        public Point ScalingCenter = Point.Empty;

        public float Transparency = 0;
        public string Effect = null;

        public DGTransformation()
        {
        }

        public float Scaling{
            set{
                ScalingX = ScalingY = value;
            }
        }

        public static DGTransformation Empty {
            get { return new DGTransformation{ ScalingX = 1, ScalingY = 1 }; }
        }
        
        public DGTransformation Mix(DGTransformation other)
        {
            return new DGTransformation()
            {
                ScalingX = this.ScalingX * other.ScalingX,
                ScalingY = this.ScalingY * other.ScalingY,
                ScalingCenter = other.ScalingCenter.ClientTo(this.ScalingCenter),
                Transparency = this.Transparency * other.Transparency,
            };
        }
    }

    public class DrawingGraphics : IDrawingGraphics
    {
        // The state (not scaled) used for the drawing primitives
        public readonly DrawingHelperState state;

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
            this.transformation = DGTransformation.Empty;
            
        }

        // Expressed in pixels
        public int MaxWidth { get; set; }

        public int MaxHeight { get; set; }

        // Graphics where the actions will be performed
        public Graphics Graphics { get; private set; }

        private DGTransformation transformation;

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
            get {
                var vx = Math.Max(0, -this.Location.X.ToLogic());
                return new Rectangle(vx, Math.Max(0,-this.Location.Y.ToLogic()), MaxWidth.ToLogic()-vx, MaxHeight.ToLogic());
            }
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
            if (this.state.CurrentPen.Width > 0)
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
            if (this.state.CurrentPen.Width > 0)
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
            if (this.state.CurrentPen.Width > 0)
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

        public IDrawingGraphics FillPolygon(Point[] points)
        {
            this.Graphics.FillPolygon(this.state.CurrentBrush, points.Select((p) => new Point(CalculateX(p.X), CalculateY(p.Y))).ToArray());
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
            if (this.state.CurrentPen.Width > 0)
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
            if (this.state.CurrentPen.Width > 0)
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
            /*
            //Huge performance hurt:                         
            var measure = this.Graphics.MeasureString(text, this.state.CurrenFont);
            this.state.CurrentX += FleuxApplication.ScaleToLogic((int)measure.Width);
            this.ValidateExtends(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY) + (int)measure.Height);
            //*/
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

        private Size CalculateTextSizeForFontSize(string text, int fontSize)
        {
            var measuredFront = ResourceManager.Instance.GetFont(this.state.CurrenFont.Name, this.state.CurrenFont.Style, fontSize);

            var s = this.Graphics.MeasureString(text, measuredFront);
            return new Size(((int) s.Width).ToLogic(), ((int) s.Height).ToLogic());
        }

        public int CalculateFontSizeForArea(string text, Size maxArea, out Size newArea)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            if (maxArea.IsEmpty)
                throw new ArgumentException("maxArea can't be empty");

            var current = (int) (state.CurrenFont.Size*0.95);
            newArea = new Size();

            while (current > 0)
            {
                newArea = CalculateTextSizeForFontSize(text, current);

                if (maxArea.Height > newArea.Height && maxArea.Width > newArea.Width)
                    break;

                current -= 3;
            }

            return current;
        }

        public IDrawingGraphics DrawMultiLineText(string text, int width, int height)
        {
            var rect = new RectangleF(this.CalculateX(this.state.CurrentX), this.CalculateY(this.state.CurrentY), this.CalculateWidth(width), this.CalculateHeight(height));
            this.Graphics.DrawString(text, this.state.CurrenFont, this.state.CurrentBrush, rect);
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

        public IDrawingGraphics CreateChild(Point innerlocation, DGTransformation t)
        {
            var childLocation = innerlocation.ToPixels().ToParent(this.Location);
            var childMaxWidth = this.MaxWidth - (childLocation.X - this.Location.X);

            var g = this.Graphics;
            var child = FromGraphicsLocationMaxWidth(g, this.canvasImage, childLocation.X, childLocation.Y, childMaxWidth);
            if (t != null)
                child.transformation = t;
            return child;
        }

        public IDrawingGraphics CreateChild(Point innerlocation)
        {
            return this.CreateChild(innerlocation, null);
        }

        public void BatchDraw(Action<IDrawingGraphics> drawer)
        {
            var omatrix = this.Graphics.Transform;
            var oloc = Location;
            #if XNA
            var otrans = this.Graphics.Transparency;
            #endif

            Location = Point.Empty;

            try{

                this.Graphics.TranslateTransform(oloc.X, oloc.Y);

                if (transformation != null)
                {
                    this.Graphics.TranslateTransform(CalculateWidth(transformation.ScalingCenter.X),
                        CalculateHeight(transformation.ScalingCenter.Y));
                    this.Graphics.ScaleTransform(transformation.ScalingX == 0 ? 0.001f : transformation.ScalingX,
                                                 transformation.ScalingY == 0 ? 0.001f : transformation.ScalingY);
                    this.Graphics.TranslateTransform(-CalculateWidth(transformation.ScalingCenter.X),
                        -CalculateHeight(transformation.ScalingCenter.Y));
                    
                    this.Graphics.TranslateTransform(CalculateWidth(transformation.RotationCenter.X),
                        CalculateHeight(transformation.RotationCenter.Y));
                    this.Graphics.RotateTransform(transformation.Rotation);
                    this.Graphics.TranslateTransform(-CalculateWidth(transformation.RotationCenter.X),
                        -CalculateHeight(transformation.RotationCenter.Y));
                    #if XNA
                    this.Graphics.Transparency = transformation.Transparency;
                    #endif
                }

                drawer(this);

            }finally{
                Location = oloc;
                this.Graphics.Transform = omatrix;
                #if XNA
                this.Graphics.Transparency = otrans;
                #endif
            }
        }

        public ClipBuffer GetClipBuffer(Rectangle region, Bitmap bitmap)
        {
            var gr = Graphics.FromImage(bitmap);
            var realRegion = this.CalculateRect(region);
#if WINCE || WIN32
            // win32 gdi draws text badly on transparent images ;(
            gr.DrawImage(this.canvasImage, 0, 0, realRegion, GraphicsUnit.Pixel);
#else
            // Android is just fine!
            gr.Clear(System.Drawing.Color.Transparent);
#endif
            FleuxApplication.ApplyGraphicsSettings(gr);
            return new ClipBuffer(bitmap, gr, realRegion, this.Graphics);
        }

        
        public ClipBuffer GetOpaqueClipBuffer(Rectangle region, Bitmap bitmap)
        {
            var gr = Graphics.FromImage(bitmap);
            var realRegion = this.CalculateRect(region);
            Graphics.Flush();
            gr.DrawImage(this.canvasImage, 0, 0, realRegion, GraphicsUnit.Pixel);
            FleuxApplication.ApplyGraphicsSettings(gr);
            return new ClipBuffer(bitmap, gr, realRegion, this.Graphics);
        }

        public int CalculateWidth(int logicalWidth)
        {
            return (int)(Math.Abs(logicalWidth).ToPixels());
        }

        public int CalculateHeight(int logicalHeight)
        {
            return (int)(Math.Abs(logicalHeight).ToPixels());
        }

        public int CalculateX(int x)
        {
            return this.drawingExtends.Left + x.ToPixels();
        }

        public int CalculateY(int y)
        {
            return this.drawingExtends.Top + y.ToPixels();
        }

        public Rectangle CalculateRect(Rectangle logicalRect)
        {
            return new Rectangle(this.CalculateX(logicalRect.X),
                                 this.CalculateY(logicalRect.Y),
                                 this.CalculateWidth(logicalRect.Width),
                                 this.CalculateHeight(logicalRect.Height));
        }

        public void Translate(int x, int y)
        {
            this.Graphics.TranslateTransform(CalculateX(x), CalculateY(y));
        }

        public void Rotate(float f)
        {
            this.Graphics.RotateTransform(f);
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

        public void Dispose()
        {
        }

    }

}
