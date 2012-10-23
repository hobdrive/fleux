namespace Fleux.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Animations;
    using Core;
    using Core.Dim;
    using Core.GraphicsHelpers;
    using Core.Scaling;
    using Gestures;
    using UIElements;

    public class FleuxControl : DoubleBufferedControl
    {
        protected int shadowImageX;

        private readonly List<UIElement> elements = new List<UIElement>();
        private readonly StoryBoard storyboard = new StoryBoard();
        private readonly GesturesEngine gestures = new GesturesEngine();

        private bool invalidating = false;
        private bool active;
        private UIElement pressedHandledBy;

        public FleuxControl()
        {
            this.EntranceDuration = 600;
            this.gestures.Tap = this.Tap;
            this.gestures.DoubleTap = this.DoubleTap;
            this.gestures.Hold = this.Hold;
            this.gestures.Pan = this.Pan;
            this.gestures.Flick = this.Flick;
            this.gestures.Pressed = this.Pressed;
            this.gestures.Released = this.Released;
            this.shadowImageX = 1000; // TODO: Review this field initialization.
        }

        Color SafeBackColor = Color.Black;
        public override Color BackColor{
            set{
                base.BackColor = value;
                this.SafeBackColor = value;
            }
        }

        public enum ShadowedAnimationOptions
        {
            None,
            FromLeft,
            FromRight,
        }

        public ShadowedAnimationOptions ShadowedAnimationMode { get; set; }

        public int EntranceDuration { get; set; }

        public Func<Point, bool> TapHandler { get; set; }

        public Func<Point, bool> DoubleTapHandler { get; set; }

        public Func<Point, bool> HoldHandler { get; set; }

        public Func<Point, Point, int, Point, bool> FlickHandler { get; set; }

        public Func<Point, Point, bool, Point, bool> PanHandler { get; set; }

        public Func<Point, UIElement> PressedHandler { get; set; }

        public Func<Point, Point, bool> ReleasedHandler { get; set; }

        public Action OnExitAnimationCompleted { get; set; }

        public void AddElement(UIElement element)
        {
            this.elements.Add(element);
            this.offUpdated = false;
            element.Updated = this.ForcedInvalidate;
        }

        public void RemoveElement(UIElement element)
        {
            this.elements.Remove(element);
            this.offUpdated = false;
        }

        public void RemoveAllElements()
        {
            this.elements.Clear();
            this.offUpdated = false;
        }

        public void Flick(Point from, Point to, int millisecs)
        {
            var handled = false;
            foreach (var el in this.elements.Where(e => e.Bounds.Contains(this.gestures.GestureStartPoint)))
            {
                if (el.Flick(from.ClientTo(el.Location), to.ClientTo(el.Location), millisecs, this.gestures.GestureStartPoint.ClientTo(el.Location)))
                {
                    handled = true;
                    break;
                }
            }
            if (!handled && this.FlickHandler != null)
            {
                this.FlickHandler(from, to, millisecs, this.gestures.GestureStartPoint);
            }
        }

        public void Pan(Point from, Point to, bool done)
        {
            var handled = false;
            foreach (var el in this.elements.Where(e => e.Bounds.Contains(this.gestures.GestureStartPoint)))
            {
                if (el.Pan(from.ClientTo(el.Location), to.ClientTo(el.Location), done, this.gestures.GestureStartPoint.ClientTo(el.Location)))
                {
                    handled = true;
                    break;
                }
            }
            if (!handled && this.PanHandler != null)
            {
                this.PanHandler(from, to, done, this.gestures.GestureStartPoint);
            }
        }

        public void Pressed(Point start)
        {
            this.pressedHandledBy = null;

            foreach (var el in this.elements.Where(e => e.Bounds.Contains(start)))
            {
                this.pressedHandledBy = el.Pressed(start.ClientTo(el.Location));
                if (this.pressedHandledBy != null)
                {
                    break;
                }
            }
            if (this.pressedHandledBy == null && this.PressedHandler != null)
            {
                this.pressedHandledBy = this.PressedHandler(start);
            }
        }

        public void Released(Point start, Point end)
        {
            if (this.pressedHandledBy != null)
            {
                this.pressedHandledBy.Released();
            }
        }

        public void AnimateEntrance()
        {
            var entranceSb = new StoryBoard();
            foreach (var element in this.elements.Where(e => e.EntranceAnimation != null))
            {
                element.EntranceAnimation.Duration = this.EntranceDuration;
                entranceSb.AddAnimation(element.EntranceAnimation);
            }
            if (this.ShadowedAnimationMode != ShadowedAnimationOptions.None)
            {
                entranceSb.AddAnimation(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.Linear)
                {
                    Duration = this.EntranceDuration,
                    To = this.ShadowedAnimationMode == ShadowedAnimationOptions.FromRight ? this.offBmp.Width : -this.offBmp.Width * 3,
                    From = this.ShadowedAnimationMode == ShadowedAnimationOptions.FromRight ? -this.offBmp.Width : -this.offBmp.Width * 1,
                    OnAnimation = v => this.shadowImageX = v
                });
            }
            entranceSb.AddAnimation(new CommitStoryboardAnimation { Duration = this.EntranceDuration, CommitAction = () => this.ForcedInvalidate() });
            entranceSb.BeginAnimate();
        }

        public void AnimateExit()
        {
            var exitSb = new StoryBoard();
            foreach (var element in this.elements.Where(e => e.ExitAnimation != null))
            {
                element.ExitAnimation.Duration = this.EntranceDuration;
                exitSb.AddAnimation(element.ExitAnimation);
            }
            if (this.ShadowedAnimationMode != ShadowedAnimationOptions.None)
            {
                exitSb.AddAnimation(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.Linear)
                {
                    Duration = this.EntranceDuration,
                    From = this.ShadowedAnimationMode == ShadowedAnimationOptions.FromRight ? this.offBmp.Width : -this.offBmp.Width * 3,
                    To = this.ShadowedAnimationMode == ShadowedAnimationOptions.FromRight ? -this.offBmp.Width : -this.offBmp.Width * 1,
                    OnAnimation = v => this.shadowImageX = v
                });
            }
            exitSb.AddAnimation(new CommitStoryboardAnimation { Duration = this.EntranceDuration, CommitAction = () => this.ForcedInvalidate() });
            exitSb.AnimateSync();

            if (this.OnExitAnimationCompleted != null)
            {
                this.OnExitAnimationCompleted();
            }
        }

        protected override void Draw(System.Windows.Forms.PaintEventArgs e)
        {
            if (this.active)
            {
                e.Graphics.Clear(this.SafeBackColor);

                var gr = DrawingGraphics.FromGraphicsAndRect(this.offGr, this.offBmp, new Rectangle(0, 0, this.offBmp.Width, this.offBmp.Height));
                try{
                    this.elements.ForEach(element => element.Draw(gr.CreateChild(element.Location, element.TransformationScaling, element.TransformationCenter)));
                }catch(Exception ex){
                    System.Console.Error.WriteLine("Fleux Failure: "+ex.StackTrace);
                }
                if (this.ShadowedAnimationMode != ShadowedAnimationOptions.None
                    && this.shadowImageX < this.offBmp.Width
                    && this.shadowImageX > -this.offBmp.Width * 3)
                {
                    gr.DrawAlphaImage("righttransition.png", new Rectangle(this.shadowImageX, 0, this.offBmp.Width * 3, this.offBmp.Height).ToLogic());
                }
            }
            else
            {
                this.AnimateEntrance();
                this.active = true;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            var p = e.Point();
            if (FleuxApplication.HorizontalMirror)
            {
                p.X = offBmp.Width - p.X - 1;
            }
            if (FleuxApplication.VerticalMirror)
            {
                p.Y = offBmp.Height - p.Y - 1;
            }
            this.gestures.MouseDown(p.ToLogic());
            if (FleuxSettings.HapticFeedbackMode == FleuxSettings.HapticOptions.AnyPress)
            {
                FleuxApplication.Led.Vibrate();
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var p = e.Point();
            if (FleuxApplication.HorizontalMirror)
            {
                p.X = offBmp.Width - p.X - 1;
            }
            if (FleuxApplication.VerticalMirror)
            {
                p.Y = offBmp.Height - p.Y - 1;
            }
            this.gestures.MouseMove(p.ToLogic());
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var p = e.Point();
            if (FleuxApplication.HorizontalMirror)
            {
                p.X = offBmp.Width - p.X - 1;
            }
            if (FleuxApplication.VerticalMirror)
            {
                p.Y = offBmp.Height - p.Y - 1;
            }
            this.gestures.MouseUp(p.ToLogic());
            base.OnMouseDown(e);
        }

        protected override void ForcedInvalidate()
        {
            if (!this.invalidating)
            {
                lock (this)
                {
                    if (!this.invalidating)
                    {
                        this.invalidating = true;
                        base.ForcedInvalidate();
                        this.invalidating = false;
                    }
                }
            }
        }

        private void Hold(Point p)
        {
            var handled = false;
            foreach (var el in this.elements.Where(e => e.Bounds.Contains(p)))
            {
                if (el.Hold(p.ClientTo(el.Location)))
                {
                    handled = true;
                    break;
                }
            }
            if (!handled && this.HoldHandler != null)
            {
                this.HoldHandler(p);
            }
        }

        private void Tap(Point p)
        {
            this.Invoke(
                    new Action(() =>
                    {
                        if (this.pressedHandledBy != null && FleuxSettings.HapticFeedbackMode == FleuxSettings.HapticOptions.Tap)
                        {
                            FleuxApplication.Led.Vibrate();
                        }
                        var handled = false;
                        foreach (var el in this.elements.Where(e => e.Bounds.Contains(p)))
                        {
                            if (el.Tap(p.ClientTo(el.Location)))
                            {
                                handled = true;
                                break;
                            }
                        }
                        if (!handled && this.TapHandler != null)
                        {
                            handled = this.TapHandler(p);
                        }
                    }));
        }

        private void DoubleTap(Point p)
        {
            var handled = false;
            foreach (var el in this.elements.Where(e => e.Bounds.Contains(p)))
            {
                if (el.DoubleTap(p.ClientTo(el.Location)))
                {
                    handled = true;
                    break;
                }
            }
            if (!handled && this.DoubleTapHandler != null)
            {
                this.DoubleTapHandler(p);
            }
        }
    }
}
