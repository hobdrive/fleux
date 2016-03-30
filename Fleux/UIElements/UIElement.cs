namespace Fleux.UIElements
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Animations;
    using Core;
    using Core.Dim;
    using Core.GraphicsHelpers;
    using Events;

    public abstract class UIElement
    {
        protected readonly List<UIElement> Children = new List<UIElement>();

        public event EventHandler VisibleChanged;

        private Size size;

        public UIElement()
        {
            Enabled = true;
        }

        public event EventHandler<SizeChangedEventArgs> SizeChanged;

        private void OnVisibleChanged()
        {
            var temp = VisibleChanged;
            if (temp != null)
            {
                temp(this, new EventArgs());
            }
        }

        /**
         * If true, the element will receive its own TapHandler _before_ all its children (prioritizing it)
         */
        public bool PreTap = false;

        private Point location;
        private bool _enabled;

        public Point Location
        {
            get { return this.location; }
            set
            {
                if (this.location != value)
                {
                    Point location = this.location;
                    this.location = value;
                    if (this.LocationChanged != null && location != this.location)
                    {
                        this.LocationChanged(this, null);
                    }
                }
            }
        }

        public event EventHandler<SizeChangedEventArgs> LocationChanged;

        public Size Size
        {
            get { return this.size; }

            set
            {
                if (this.size != value)
                {
                    var old = this.Size;
                    this.size = value;
                    if (this.SizeChanged != null)
                    {
                        this.SizeChanged(this, new SizeChangedEventArgs {New = value, Old = old});
                    }
                }
            }
        }

        public int Width
        {
            get { return Size.Width; }

            set { this.Size = new Size(value, Size.Height); }
        }

        public int Height
        {
            get { return Size.Height; }

            set { this.Size = new Size(Size.Width, value); }
        }

        public DGTransformation Transformation { get; set; }

        public virtual Rectangle Bounds
        {
            get { return new Rectangle(this.Location.X, this.Location.Y, this.Size.Width, this.Size.Height); }
        }

        public IAnimation EntranceAnimation { get; set; }

        public IAnimation ExitAnimation { get; set; }

        public Action Updated { get; set; }

        /// <summary>
        /// If true, then this element will consume all the unhandled input, and will not pass it to the parent.
        /// </summary>
        public bool ConsumeAllInput { get; set; }

        public Func<Point, bool> TapHandler { get; set; }

        public Func<Point, bool> DoubleTapHandler { get; set; }

        public Func<Point, bool> HoldHandler { get; set; }

        public Func<Point, Point, int, Point, bool> FlickHandler { get; set; }

        public Func<Point, Point, bool, Point, bool> PanHandler { get; set; }

        public Func<Point, UIElement> PressedHandler { get; set; }

        /// <summary>
        /// Indicates the mouse released event
        /// In case handler returns true, all other highlevel events are cancelled
        /// </summary>
        public Func<bool> ReleasedHandler { get; set; }

        public UIElement Parent { get; set; }

        /// <summary>
        /// Gets or sets boolean indicating whether the UIElememt can respond to user interaction.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value) return;
                _enabled = value;
                Update();
            }
        }

        public IEnumerable<UIElement> ChildrenEnumerable
        {
            get
            {
                return this.Children.Select(x => x); // Returns read only IEnumerable of children
            }
        }

        public int ChildrenCount
        {
            get { return this.Children.Count; }
        }

        public string ID { get; set; }

        public bool Visible
        {
            get { return Visibility == StateVisible; }
            set { Visibility = value ? StateVisible : StateHidden; }
        }

        private const int StateVisible = 0;
        private const int StateHidden = 1;
        private const int StateGone = 2;

        private int _visibility;

        public int Visibility
        {
            get { return _visibility; }
            set
            {
                _visibility = value;
                OnVisibleChanged();
            }
        }

        public UIElement this[int idx]
        {
            get { return this.Children[idx]; }

        }

        public virtual bool PressFeedbackSupported
        {
            set
            {
                if (value)
                {
                    this.PressedHandler = p =>
                    {
                        this.Transformation = new DGTransformation()
                        {
                            ScalingX = 0.9f,
                            ScalingY = 0.9f,
                            ScalingCenter = new Point(this.Size.Width/2, this.Size.Height/2),
                        };
                        if (FleuxSettings.HapticFeedbackMode == FleuxSettings.HapticOptions.FeedbackEnabledPress)
                        {
                            FleuxApplication.Led.Vibrate();
                        }
                        this.Update();
                        return this;
                    };
                    this.ReleasedHandler = () =>
                    {
                        this.Transformation = null;
                        this.Update();
                        return false;
                    };
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
                return child.Bounds.IntersectsWith(new Rectangle(0, 0, this.Size.Width, this.Size.Height)) && this.Parent.IsShowing(this);
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

        /// <summary>
        /// Calculate location of child in depth hierarchy.
        /// </summary>
        /// <returns>The location of child agains "this" parent</returns>
        /// <param name="child">Child to calculate</param>
        public Point ChildLocation(UIElement child)
        {
            Point cloc = Point.Empty;
            while (child.Parent != null && child != this)
            {
                cloc = cloc.Add(child.Location);
                child = child.Parent;
            }
            return cloc;
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public virtual Point ApplyTransformation(Point source)
        {
            return source;
        }

        public virtual bool Tap(Point p)
        {
            if (!Enabled)
            {
                return false;
            }

            bool handled = false;
            if (PreTap && this.TapHandler != null)
            {
                handled = this.TapHandler(this.ApplyTransformation(p));
                if (handled) return handled;
            }
            handled = this.TraverseHandle(this.ApplyTransformation(p),
                el => el.Tap(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            handled |= ConsumeAllInput;
            if (!handled && !PreTap && this.TapHandler != null)
            {
                handled = this.TapHandler(this.ApplyTransformation(p));
            }
            return handled;
        }

        public virtual bool DoubleTap(Point p)
        {
            if (!Enabled)
            {
                return false;
            }

            bool handled = this.TraverseHandle(this.ApplyTransformation(p),
                el => el.DoubleTap(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            handled |= ConsumeAllInput;
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
            handled |= ConsumeAllInput;
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
            handled |= ConsumeAllInput;
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
            handled |= ConsumeAllInput;
            if (!handled && this.PanHandler != null)
            {
                handled = this.PanHandler(this.ApplyTransformation(from), this.ApplyTransformation(to), done,
                    this.ApplyTransformation(startPoint));
            }
            return handled;
        }

        public virtual UIElement Pressed(Point p)
        {
            if (!Enabled) return null;

            UIElement pressTarget = this.TraverseHandle(this.ApplyTransformation(p),
                el => el.Pressed(this.ApplyTransformation(p).ClientTo(this.ApplyTransformation(el.Location))));
            if (pressTarget == null && this.PressedHandler != null)
            {
                pressTarget = this.PressedHandler(this.ApplyTransformation(p));
            }
            if (ConsumeAllInput && pressTarget == null)
                pressTarget = this;
            return pressTarget;
        }

        public virtual bool Released()
        {
            if (!Enabled) return false;

            if (this.ReleasedHandler != null)
            {
                return this.ReleasedHandler();
            }
            return false;
        }

        public virtual void ResizeForWidth(int width)
        {
        }

        public virtual void ResizeForHeight(int height)
        {
        }

        protected virtual bool TraverseHandle(Point start, Func<UIElement, bool> elementHandler)
        {
            bool handled = false;
            foreach (var el in this.Children.Where(x => x.Visible && x.Bounds.Contains(start)).Reverse())
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
            foreach (var el in this.Children.Where(x => x.Visible && x.Bounds.Contains(start)).Reverse())
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