namespace Fleux.Controls.Gestures
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using Core.Dim;

    // Tap
    // A tap represents the left click of a mouse.
    // The application receives a single GID_SELECT gesture message when the finger–down and finger–up events occur in 
    // a defined time period and a specific distance from each other. 
    // There can be several WM_MOUSEMOVE messages after the WM_LBUTTONDOWN event and before the GID_SELECT message.
    //
    // Double Tap
    // A double tap represents the left double click of a mouse.
    // The application receives a GID_DOUBLESELECT gesture message when the finger–up events occur in a defined time period 
    // and within a specific distance of each other.
    //
    // Hold
    // The user can press and hold on the screen to represent the right click of a mouse.
    // The application receives a single GID_HOLD message when the finger remains down for longer than a defined time period 
    // and all points during that period are within a specific distance. 
    // It is followed by a GID_END message on finger–up or at the end of the Hold time threshold.
    //
    // Flick
    // The user can move a finger across the screen to initiate per-pixel scrolling, and if this movement is fast enough, 
    // scrolling will continue after the finger is lifted.
    // The application receives a single GID_SCROLL gesture message at the end of the movement upon finger-up. 
    // A flick frequently occurs after a pan (one or more GID_PAN gesture messages, followed by a GID_SCROLL message, 
    // and then a GID_END message).
    //
    // Pan
    // The user can press and hold on the screen and then drag the finger in any direction to represent a mouse move event.
    // The application receives one or more GID_PAN gesture messages as the position changes followed by a GID_END when the 
    // finger is lifted. Mouse messages are interleaved with gesture messages.
    // Panning can occur after a Hold gesture.
    //
    // Press
    // The user has pressed the screen.
    //
    // Release
    // The user has removed his finger from the screen
    public class GesturesEngine
    {
        private readonly GestureDetectionParameters parameters;
        private int mouseDownTicks;
        private int lastTapTicks;
        private Point lastTapPoint;
        private Point mouseDownPoint;
        private bool canBeTap;
        private bool canBeHold;
        private bool mouseDown;
        private Point prevDownPoint;
        private Timer holdTimer;
        
        public int LastTapDuration;

        public GesturesEngine()
        {
            this.parameters = GestureDetectionParameters.Current;
            this.holdTimer = new Timer();
            this.holdTimer.Tick += (obj, ev) => {
                this.holdTimer.Enabled = false;
                if (this.canBeHold)
                {
                    this.RaiseHold();
                    this.canBeHold = false;
                    this.MouseUp(this.prevDownPoint);
                }
            };
        }

        public Action<Point> Tap { get; set; }

        public Action<Point> DoubleTap { get; set; }

        public Action<Point> Hold { get; set; }

        public Action<Point, Point, int> Flick { get; set; }

        public Action<Point, Point, bool> Pan { get; set; }

        public Action<Point> Pressed { get; set; }

        public Action<Point, Point> Released { get; set; }

        public Point GestureStartPoint
        {
            get { return this.mouseDownPoint; }
        }

        public static bool IsHorizontal(Point from, Point to)
        {
            return Math.Abs(to.X - from.X) - Math.Abs(to.Y - from.Y) > 0;
        }

        public void MouseDown(Point point)
        {
            this.mouseDownTicks = Environment.TickCount;
            this.mouseDownPoint = point;
            this.prevDownPoint = point;
            this.canBeTap = true;
            this.canBeHold = true;
            this.mouseDown = true;
            this.RaisePressed(point);
            this.holdTimer.Enabled = false;
            this.holdTimer.Interval = this.parameters.TapTimePeriod;
            this.holdTimer.Enabled = true;
        }

        public void MouseMove(Point point)
        {
            if (!this.mouseDown) return;
            this.canBeHold = this.canBeHold && this.mouseDownPoint.IsCloseTo(point, this.parameters.TapDistance);
            if (!this.mouseDownPoint.IsCloseTo(point, this.parameters.PanThreshold))
                this.RaisePan(this.prevDownPoint, point, false);
            this.prevDownPoint = point;
        }

        public void MouseUp(Point point)
        {
            this.mouseDown = false;
            this.LastTapDuration = Environment.TickCount - this.mouseDownTicks;
            this.canBeHold = false;
            this.RaiseReleased(this.mouseDownPoint, point);

            // Check for tap or double tap
            if (this.canBeTap && this.mouseDownPoint.IsCloseTo(point, this.parameters.TapDistance)
                && LastTapDuration <= this.parameters.TapTimePeriod)
            {
                this.RaiseTap(this.mouseDownPoint);
            }
            else if (this.canBeHold && LastTapDuration > this.parameters.TapTimePeriod)
            {
                this.RaiseHold();
            }
            else if (LastTapDuration <= this.parameters.FlickPeriod)
            {
                this.RaiseFlick(this.mouseDownPoint, point, LastTapDuration);
            }
            else
            {
                this.RaisePan(this.prevDownPoint, point, true);
            }
        }

        private void RaiseHold()
        {
            if (this.Hold != null)
            {
                this.Hold.Invoke(this.mouseDownPoint);
            }
        }

        private void RaisePan(Point from, Point to, bool done)
        {
            if (this.Pan != null)
            {
                this.Pan.Invoke(from, to, done);
            }
        }

        private void RaiseFlick(Point start, Point end, int millisecs)
        {
            if (this.Flick != null)
            {
                this.Flick.Invoke(start, end, millisecs);
            }
        }

        private void RaiseDoubleTap()
        {
            if (this.DoubleTap != null)
            {
                this.DoubleTap.Invoke(this.mouseDownPoint);
            }
        }

        private void RaiseTap(Point p)
        {
            if (Environment.TickCount - this.lastTapTicks <= this.parameters.DoubleTapTimePeriod
                && p.IsCloseTo(this.lastTapPoint, this.parameters.TapDistance))
            {
                this.RaiseDoubleTap();
            }
            else
            {
                this.lastTapTicks = Environment.TickCount;
                this.lastTapPoint = p;
                if (this.Tap != null)
                {
                    this.Tap.Invoke(p);
                }
            }
        }

        private void RaisePressed(Point p)
        {
            if (this.Pressed != null)
            {
                this.Pressed.Invoke(p);
            }
        }

        private void RaiseReleased(Point start, Point end)
        {
            if (this.Released != null)
            {
                this.Released.Invoke(start, end);
            }
        }
    }
}
