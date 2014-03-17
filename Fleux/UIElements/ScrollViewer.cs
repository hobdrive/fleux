namespace Fleux.UIElements
{
    using System;
    using System.Drawing;
    using Animations;
    using Controls.Scrolling;
    using Core.GraphicsHelpers;
    using Core.Scaling;

    public class ScrollViewer : UIElement
    {
        private IGestureScrollingBehavior horizontalInertia;
        private IGestureScrollingBehavior verticalInertia;
        private bool lastGestureWasHorizontal = false;
        private UIElement content;
        private Bitmap clipBitmap;

        private bool panInProgress = false;
        private int panV = 0;
        private int panH = 0;

        public static int ShadowHeightDefault = 15;
        public static int ShadowStepsDefault = 3;
        public static bool DrawShadowsDefault = true;

        public bool DrawShadows { get; set; }
        Bitmap TopShadow, BottomShadow;
        public int ShadowHeight = ShadowHeightDefault;
        public int ShadowSteps = ShadowStepsDefault;

        public ScrollViewer()
        {
            this.EntranceAnimation = new ForwarderAnimation(() => this.content.EntranceAnimation);
            this.ExitAnimation = new ForwarderAnimation(() => this.content.ExitAnimation);
            this.ScrollBarWidth = 5;
            this.ScrollBarColor = ScrollBarDefaultColor;
            DrawShadows = DrawShadowsDefault;
        }

        ~ScrollViewer()
        {
            if (this.horizontalInertia != null)
            {
                this.horizontalInertia.Dispose();
            }
            if (this.verticalInertia != null)
            {
                this.verticalInertia.Dispose();
            }
        }

        public UIElement Content
        {
            get
            {
                return this.content;
            }

            set
            {
                // allow content recreation
                if (this.content != null)
                    this.Children.Remove(this.content);
                this.content = value;
                this.content.Updated = this.Update;
                this.content.Parent = this;
                this.Children.Add(this.content);
                this.horizontalInertia = null;
                this.verticalInertia = null;
                this.HorizontalOffset = 0;
                this.VerticalOffset = 0;
            }
        }

        public int HorizontalOffset
        {
            get { return this.content.Location.X; }
            set {
                this.content.Location = new Point(value, this.content.Location.Y);
                if (!panInProgress){
                    panH = value;
                }
            }
        }

        public int VerticalOffset
        {
            get { return this.content.Location.Y; }
            set {
                this.content.Location = new Point(this.content.Location.X, value);
                if (!panInProgress){
                    panV = value;
                }
            }
        }

        public bool HorizontalScroll { get; set; }

        public bool VerticalScroll { get; set; }

        public bool ShowScrollbars { get; set; }

        public Color ScrollBarColor { get; set; }
        
        public static Color ScrollBarDefaultColor = Color.White;

        public int ScrollBarWidth { get; set; }

        /**
          If true, scroll viewer will not redraw content during panning, but will use cached image for fast redraws
          Can indtroduce some artefacts
        */
        public bool CachePanning { get; set; }

        /**
          If true, vertical 'overscroll' when panning and flicking will be disabled
         */
        public bool TrimVerticalPanning = false;

        /**
          If true, horizontal 'overscroll' when panning and flicking will be disabled
         */
        public bool TrimHorizontalPanning = false;

        public override Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            var dg = drawingGraphics;
            if (this.clipBitmap == null)
            {
                this.clipBitmap = new Bitmap(this.Size.Width.ToPixels(), this.Size.Height.ToPixels()-1); // TODO roundup errors!
            }
            if (CachePanning && panInProgress && this.clipBitmap != null)
            {
                drawingGraphics.Graphics.DrawImage(this.clipBitmap, drawingGraphics.CalculateX(this.HorizontalOffset - this.panH), drawingGraphics.CalculateY(this.VerticalOffset - this.panV));
            }else
            {
            using (var clipBuffer = drawingGraphics.GetClipBuffer(new Rectangle(0, 0, this.Size.Width, this.Size.Height), this.clipBitmap))
            {
                /* Do shadows */
                if (this.DrawShadows)
                {
                    if (TopShadow == null)
                    {
                        TopShadow = new Bitmap(this.Size.Width.ToPixels(), drawingGraphics.CalculateHeight(ShadowHeight));
                        BottomShadow = new Bitmap(this.Size.Width.ToPixels(), drawingGraphics.CalculateHeight(ShadowHeight+1));
                    }
                    if (this.VerticalOffset < 0)
                    {
                        drawingGraphics.GetOpaqueClipBuffer(new Rectangle(0, 0, this.Size.Width, this.ShadowHeight), TopShadow).Dispose();
                    }
                    if (this.VerticalOffset > Math.Min(0, -this.Content.Size.Height + this.Size.Height))
                    {
                        drawingGraphics.GetOpaqueClipBuffer(new Rectangle(0, this.Size.Height - ShadowHeight,
                                                                          this.Size.Width, ShadowHeight+1), BottomShadow).Dispose();
                    }
                }
                
                this.Content.Draw(clipBuffer.DrawingGr.CreateChild(new Point(this.HorizontalOffset, this.VerticalOffset),
                                                                   this.content.TransformationScaling, this.content.TransformationCenter));
                if (this.ShowScrollbars)
                {
                    this.DrawScrollBar(clipBuffer.DrawingGr);
                }
            }
            }

            if (this.DrawShadows)
            {
                if (this.VerticalOffset < 0)
                {
                    int cheight = - dg.CalculateHeight(Math.Max(ShadowHeight + this.VerticalOffset, 0));
                    for(int s = 0; s < ShadowSteps; s++)
                    {
                        drawingGraphics.Graphics.AlphaBlend(TopShadow,
                          new Rectangle(dg.CalculateX(0), dg.CalculateY(0)+cheight, TopShadow.Width, TopShadow.Height/ShadowSteps),
                          new Rectangle(0, cheight, TopShadow.Width, TopShadow.Height/ShadowSteps), 0.95f - 1f/(ShadowSteps+2)*s);
                        cheight += TopShadow.Height/ShadowSteps;
                    }
                }
                if (this.VerticalOffset > Math.Min(0, -this.Content.Size.Height + this.Size.Height))
                {
                    int cheight = 0;
                    for(int s = 0; s < ShadowSteps; s++)
                    {
                        drawingGraphics.Graphics.AlphaBlend(BottomShadow,
                          new Rectangle(dg.CalculateX(0), dg.CalculateY(this.Size.Height-ShadowHeight)+cheight, BottomShadow.Width, BottomShadow.Height/ShadowSteps),
                          new Rectangle(0, cheight, BottomShadow.Width, BottomShadow.Height/ShadowSteps), 1f/(ShadowSteps+2)*(s+2));
                        cheight += BottomShadow.Height/ShadowSteps;
                    }
                }
            }
        }

        public void DrawScrollBar(IDrawingGraphics drawingGr)
        {
            drawingGr.DrawAlphaImage("verticalscrollbar.png", new Rectangle(this.Size.Width - this.ScrollBarWidth, 0, this.ScrollBarWidth, this.Size.Height));
            var scrollHeight = 0;
            var scrollWidth = 0;
            var scrollBegin = 0;
            if (this.Content.Size.Height != 0)
            {
                scrollHeight = Math.Max(this.Size.Height * this.Size.Height / this.Content.Size.Height, 20);
                scrollBegin = this.Size.Height * -this.VerticalOffset / this.Content.Size.Height;
            }
            drawingGr.Color(this.ScrollBarColor).FillRectangle(new Rectangle(this.Size.Width - this.ScrollBarWidth, scrollBegin, this.ScrollBarWidth, scrollHeight));
            
            if (HorizontalScroll)
            {
                if (this.Content.Size.Width != 0)
                {
                    scrollWidth = Math.Max(this.Size.Width * this.Size.Width / this.Content.Size.Width, 20);
                    scrollBegin = this.Size.Width * -this.HorizontalOffset / this.Content.Size.Width;
                }
                drawingGr.Color(this.ScrollBarColor).FillRectangle(new Rectangle(scrollBegin, this.Size.Height - this.ScrollBarWidth, scrollWidth, this.ScrollBarWidth));
            }
        }

        public override bool Pan(Point from, Point to, bool done, Point startPoint)
        {
            var directionDelta = Math.Abs(to.X - from.X) - Math.Abs(to.Y - from.Y);
            var isHorizontal = directionDelta == 0 ? this.lastGestureWasHorizontal : directionDelta > 0;
            panInProgress = !done;
            if (this.horizontalInertia != null && this.HorizontalScroll && !isHorizontal)
            {
                this.horizontalInertia.Pan(0, 0, done);
                //return false;
            }
            if (this.verticalInertia != null && this.VerticalScroll && isHorizontal)
            {
                this.verticalInertia.Pan(0, 0, done);
                //return false;
            }
            if ((isHorizontal && !this.HorizontalScroll)
                || (!isHorizontal && !this.VerticalScroll))
            {
                return false;
            }

            this.TryCreateInertia();

            if (this.horizontalInertia != null)
            {
                this.horizontalInertia.Pan(from.X, to.X, done);
                to.X = from.X;
            }
            if (this.verticalInertia != null)
            {
                this.verticalInertia.Pan(from.Y, to.Y, done);
                to.Y = from.Y;
            }
            return true;
        }

        public override bool Flick(Point from, Point to, int millisecs, Point startPoint)
        {
            this.TryCreateInertia();
            panInProgress = false;

            // Validate if should we handle this Flick
            var directionDelta = Math.Abs(to.X - from.X) - Math.Abs(to.Y - from.Y);
            var isHorizontal = directionDelta == 0 ? this.lastGestureWasHorizontal : directionDelta > 0;
            if ((isHorizontal && !this.HorizontalScroll)
                || (!isHorizontal && !this.VerticalScroll))
            {
                return false;
            }

            if (this.horizontalInertia != null)
            {
                this.horizontalInertia.Flick(from.X, to.X, millisecs);
                to.X = from.X;
            }
            if (this.verticalInertia != null)
            {
                this.verticalInertia.Flick(from.Y, to.Y, millisecs);
                to.Y = from.Y;
            }
            return true;
        }

        public override UIElement Pressed(Point p)
        {
            if (this.horizontalInertia != null && this.HorizontalScroll)
            {
                this.horizontalInertia.Pressed();
            }

            if (this.verticalInertia != null && this.VerticalScroll)
            {
                this.verticalInertia.Pressed();
            }

            return base.Pressed(p);
        }

        private void TryCreateInertia()
        {
            if (this.Size.Height > 0 && this.content.Size.Height > 0)
            {
                if (this.HorizontalScroll && this.horizontalInertia == null)
                {
                    this.horizontalInertia = new GestureInertiaBehavior(v =>
                                                                          {
                                                                              this.HorizontalOffset = v;
                                                                              this.Update();
                                                                          })
                                            {
                                                Min = -Math.Max(0, this.content.Size.Width - this.Size.Width),
                                                Max = 0,
                                                TrimPanning = TrimHorizontalPanning,
                                            };
                }
                if (this.VerticalScroll && this.verticalInertia == null)
                {
                    this.verticalInertia = new GestureInertiaBehavior(v =>
                                                                        {
                                                                            this.VerticalOffset = v;
                                                                            this.Update();
                                                                        })
                                            {
                                                Min = -Math.Max(0, this.content.Size.Height - this.Size.Height),
                                                Max = 0,
                                                TrimPanning = TrimVerticalPanning,
                                            };
                }
            }
        }
    }
}
