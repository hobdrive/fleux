namespace Fleux.Animations
{
    using System;

    public class CommitStoryboardAnimation : IAnimation
    {
        private int animationStarted;
        private bool lastKeepAnimating;

        public Action CommitAction { get; set; }

        public int Duration { get; set; }

        public int StartsAt { get; set; }

        public int StopsAt { get; set; }

        public void Reset()
        {
            this.animationStarted = Environment.TickCount;
            this.lastKeepAnimating = true;
        }

        public void Cancel()
        {
            this.lastKeepAnimating = false;
        }

        public bool Animate()
        {
            if (!this.lastKeepAnimating)
            {
                return false;
            }

            var ellapsed = Environment.TickCount - this.animationStarted;
            if (ellapsed >= this.StartsAt)
            {
                this.CommitAction.Invoke();
            }

            this.lastKeepAnimating = (this.StopsAt == 0 && ellapsed < this.Duration) || (ellapsed <= this.StopsAt);
            return this.lastKeepAnimating;
        }
    }
}
