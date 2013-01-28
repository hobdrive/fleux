namespace Fleux.UIElements
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Animations;
    using Controls.Gestures;
    using Core;
    using Core.Dim;
    using Core.GraphicsHelpers;
    using Core.NativeHelpers;
    using Core.Scaling;
    using Events;

    public abstract class UIElement
    {
        protected readonly List<UIElement> Children = new List<UIElement>();

        private Size size;

        public UIElement()
        {
            // Default transformation parameters
            this.TransformationScaling = 1.0;
            this.TransformationCenter = new Point(0, 0);
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;

        /**
         * If true, the element will receive its own TapHandler _before_ all its children (prioritizing it)
         */
        public bool PreTap = false;

        private Point location;

        public Point Location
        {
            get
            {
                return this.location;
            }
            set
            {
                if (this.location != value)
                {
                    Point location = this.location;
                    this.location = value;
                    if (this.LocationChanged != null)
                    {
                        this.LocationChanged(this, null);
                    }
                }
            }
        }

        public event EventHandler<SizeChangedEventArgs> LocationChanged;

        public Size Size 
        { 
            get
            {
                return this.size;
            }

            set
            {
                if (this.size != value)
                {
                    var old = this.Size;
                    this.size = value;
                    if (this.SizeChanged != null)
                    {
                        this.SizeChanged(this, new SizeChangedEventArgs { New = value, Old = old });
                    }
                }
            }
        }
        
        public int Width
        {
            get{
                return Size.Width;
            }
            
            set{
                this.Size = new Size(value, Size.Height);
            }
        }

        public double TransformationScaling { get; set; }

        public Point TransformationCenter { get; set; }

        public virtual Rectangle Bounds
        {
            get { return new Rectangle(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height); }
        }

        public IAnimation EntranceAnimation { get; set; }

        public IAnimation ExitAnimation { get; set; }

        public Action Updated { get; set; }

        public Func<Point, bool> TapHandler { get; set; }

        public Func<Point, bool> DoubleTapHandler { get; set; }

        public Func<Point, bool> HoldHandler { get; set; }

        public Func<Point, Point, int, Point, bool> FlickHandler { get; set; }

        public Func<Point, Point, bool, Point, bool> PanHandler { get; set; }

        public Func<Point, UIElement> PressedHandler { get; set; }

        public Action ReleasedHandler { get; set; }

        public UIElement Parent { get; set; }

        public IEnumerable<UIElement> ChildrenEnumerable
        {
            get
            {
                return this.Children.Select(x => x); // Returns read only IEnumerable of children
            }
        }

        public int ChildrenCount{
            get{ return this.Children.Count; }
        }

        public string ID { get; set; }

        public bool Visible{ get
            {
                return Visibility == StateVisible;
            }
            set
            {
                Visibility = value ? StateVisible : StateHidden;
            }
        }

        const int StateVisible = 0;
        const int StateHidden = 1;
        const int StateGone = 2;
        public int Visibility { get; set; }

        public UIElement this[int idx]
        {
            get{
                return this.Children[idx];
            }
            
        }

        public virtual bool PressFeedbackSupported
        {
            set
            {
                if (value)
                {
                    this.PressedHandler = p =>
                    {
                        this.TransformationScaling = 0.95;
                        this.TransformationCenter = new Point(this.Size.Width / 2, this.Size.Height / 2);
                        if (FleuxSettings.HapticFeedbackMode == FleuxSettings.HapticOptions.FeedbackEnabledPress)
                        {
                            FleuxApplication.Led.Vibrate();
                        }
                        this.Update();
                        return this;
                    };
                    this.ReleasedHandler = () => { this.TransformationScaling = 1.0; this.Update(); };
                }
                else
                {
                    this.PressedHandler = p => null;
                }
            }
        }

        public abstract void Draw(IDrawingGraphics drawingGraphics);

        public bool IsShowing(UIElement child)
        {
            if (this.Parent != null)
            {
                if (child.Bounds.IntersectsWith(new Rectangle(0, 0, this.Size.Width, this.Size.Height)))
                {
                    return this.Parent.IsShowing(this);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void Update()
        {
            if (this.Updated != null && (this.Parent == null || this.Parent.IsShowing(this)))
            {
                this.Updated();
            }
        }

        public virtual Point ApplyTransformation(Point source)
        {
            return source;
        }

        public virtual bool Tap(Point p)
        {
            bool handled = false;
            if (PreTap && this.TapHandler != null)
            {
                handled = this.TapHandler(this.ApplyTransformation(p));
                if (handled) return handled;
            }
            handled = this.TraverseHandle(this.ApplyTransformation(p),
                                                el => el.Tap(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            if (!handled && !PreTap && this.TapHandler != null)
            {
                handled = this.TapHandler(this.ApplyTransformation(p));
            }
            return handled;
        }

        public virtual bool DoubleTap(Point p)
        {
            bool handled = this.TraverseHandle(this.ApplyTransformation(p),
                                               el => el.DoubleTap(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            if (!handled && this.DoubleTapHandler != null)
            {
                handled = this.DoubleTapHandler(this.ApplyTransformation(p));
            }
            return handled;
        }

        public virtual bool Hold(Point p)
        {
            bool handled = this.TraverseHandle(this.ApplyTransformation(p),
                                               el => el.Hold(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            if (!handled && this.HoldHandler != null)
            {
                handled = this.HoldHandler(this.ApplyTransformation(p));
            }
            return handled;
        }

        public virtual bool Flick(Point from, Point to, int millisecs, Point startPoint)
        {
            bool handled = this.TraverseHandle(this.ApplyTransformation(startPoint),
                                                el => el.Flick(this.ApplyTransformation(from).ClientTo(this.ApplyTransformation(el.Location)),
                                                               this.ApplyTransformation(to).ClientTo(this.ApplyTransformation(el.Location)),
                                                               millisecs,
                                                               this.ApplyTransformation(startPoint).ClientTo(this.ApplyTransformation(el.Location))));
            if (!handled && this.FlickHandler != null)
            {
                handled = this.FlickHandler(this.ApplyTransformation(from),
                                            this.ApplyTransformation(to),
                                            millisecs,
                                            this.ApplyTransformation(startPoint));
            }
            return handled;
        }

        public virtual bool Pan(Point from, Point to, bool done, Point startPoint)
        {
            bool handled = this.TraverseHandle(this.ApplyTransformation(startPoint),
                                               el => el.Pan(this.ApplyTransformation(from).ClientTo(this.ApplyTransformation(el.Location)),
                                                            this.ApplyTransformation(to).ClientTo(this.ApplyTransformation(el.Location)),
                                                            done,
                                                            this.ApplyTransformation(startPoint).ClientTo(this.ApplyTransformation(el.Location))));
            if (!handled && this.PanHandler != null)
            {
                handled = this.PanHandler(this.ApplyTransformation(from), this.ApplyTransformation(to), done, this.ApplyTransformation(startPoint));
            }
            return handled;
        }

        public virtual UIElement Pressed(Point p)
        {
            UIElement handled = this.TraverseHandle(this.ApplyTransformation(p),
                                               el => el.Pressed(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            if (handled == null && this.PressedHandler != null)
            {
                handled = this.PressedHandler(this.ApplyTransformation(p));
            }
            return handled;
        }

        public virtual void Released()
        {
            if (this.ReleasedHandler != null)
            {
                this.ReleasedHandler();
            }
        }

        public virtual void ResizeForWidth(int width)
        {
        }

        protected virtual bool TraverseHandle(Point start, Func<UIElement, bool> elementHandler)
        {
            bool handled = false;
            foreach (var el in this.Children.Where(x => x.Bounds.Contains(start)).Reverse())
            {
                if (elementHandler(el))
                {
                    handled = true;
                    break;
                }
            }
            return handled;
        }

        protected virtual UIElement TraverseHandle(Point start, Func<UIElement, UIElement> elementHandler)
        {
            UIElement handled = null;
            foreach (var el in this.Children.Where(x => x.Bounds.Contains(start)).Reverse())
            {
                handled = elementHandler(el);
                if (handled != null)
                {
                    break;
                }
            }
            return handled;
        }

        public void PutBelow(UIElement anchor)
        {
            PutBelow(anchor, 0);
        }

        public void PutBelow(UIElement anchor, int padding)
        {
            this.Location = new Point(anchor.Location.X, anchor.Location.Y + anchor.Size.Height + padding);
        }

        public void PutAtLeft(UIElement anchor)
        {
            PutAtLeft(anchor, 0);
        }

        public void PutAtLeft(UIElement anchor, int padding)
        {
            this.Location = new Point(anchor.Location.X - this.Size.Width - padding, anchor.Location.Y);
        }

        public void PutAtRight(UIElement anchor)
        {
            PutAtRight(anchor, 0);
        }

        public void PutAtRight(UIElement anchor, int padding)
        {
            this.Location = new Point((anchor.Location.X + anchor.Size.Width) + padding, anchor.Location.Y);
        }


    }
}