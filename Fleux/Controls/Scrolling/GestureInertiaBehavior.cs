namespace Fleux.Controls.Scrolling
{
    using System;
    using Animations;
    using Core;

    public class GestureInertiaBehavior : IGestureScrollingBehavior
    {
        private readonly Action<int> applyAction;
        private IAnimation animation;

        public GestureInertiaBehavior(Action<int> applyAction)
        {
            this.applyAction = applyAction;
        }

        public int Current { get; set; }

        public int Max { get; set; }

        public int Min { get; set; }

        /* If true, pans and flicks will not shift content over than minimums */
        public bool TrimPanning = false;

        public void Pan(int from, int to, bool done)
        {
            this.CancelCurrentAnimation();
            this.ApplyNewCurrent(this.Current + to - from);
            if (done)
            {
                this.CheckForBouncing();
            }
        }

        public void Pressed()
        {
            this.CancelCurrentAnimation();
        }

        public void Flick(int from, int to, int millisecs)
        {
            if (FleuxSettings.InertiaMode != FleuxSettings.InertiaModeOptions.RealisticPhysics)
            {
                millisecs = millisecs / ((FleuxSettings.InertiaMode == FleuxSettings.InertiaModeOptions.Physics2x) ? 2 : 3);
            }
            if (!this.CheckForBouncing())
            {
                var velocity = (double)(to - from) / (double)millisecs;
                this.StoryBoardPlay(
                        new DeceleratedAnimation
                        {
                            InitialValue = this.Current,
                            MinValue = this.Min,
                            MaxValue = this.Max,
                            InitialVelocity = velocity,
                            LimitsBehavior = DeceleratedAnimation.LimitsBehaviorOptions.None,
                            Deceleration = 0.000075,
                            MaxVelocity = 10,
                            OnAnimation = v => this.ApplyNewCurrent(v)
                        });
            }
        }

        public void Dispose()
        {
            if (this.animation != null)
            {
                this.animation.Cancel();
            }
        }

        private void ApplyNewCurrent(int current)
        {
            if (TrimPanning){
                if (current < this.Min) current = this.Min;
                if (current > this.Max) current = this.Max;
            }
            this.Current = current;
            this.applyAction(this.Current);
        }

        private void StoryBoardPlay(IAnimation animation)
        {
            this.CancelCurrentAnimation();
            this.animation = animation;
            StoryBoard.BeginPlay(this.animation);
        }

        private void CancelCurrentAnimation()
        {
            if (this.animation != null)
            {
                this.animation.Cancel();
            }
        }

        private bool CheckForBouncing()
        {
            if (this.Current < this.Min)
            {
                this.StoryBoardPlay(
                    new FunctionBasedAnimation(FunctionBasedAnimation.Functions.CubicReverse)
                    {
                        From = this.Current,
                        To = this.Min,
                        Duration = 250,
                        OnAnimation = v => this.ApplyNewCurrent(v)
                    });
                return true;
            }
            else if (this.Current > this.Max)
            {
                this.StoryBoardPlay(
                    new FunctionBasedAnimation(FunctionBasedAnimation.Functions.CubicReverse)
                    {
                        From = this.Current,
                        To = this.Max,
                        Duration = 250,
                        OnAnimation = v => this.ApplyNewCurrent(v)
                    });
                return true;
            }
            return false;
        }
    }
}
