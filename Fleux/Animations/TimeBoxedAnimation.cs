namespace Fleux.Animations
{
    using System;

    public class TimeBoxedAnimation : IAnimation
    {
        public int InitialValue;
        public int TargetValue;
        public int StopsAt;
        public int StartsAt;
        public int TargetDuration = 300; // Total animation duration, even if it starts delayed (It includes the wait time)
        public bool CalculateDeAcceleration;
        public Action<int> OnAnimation;

        protected double deacceleration = 0.001; // Will be calculated

        private double velocity;
        private int initialTicks;
        private bool lastKeepAnimating;

        public int CurrentValue { get; private set; }

        public int Duration { get; set; }

        public void Reset()
        {
            // Initialize values
            this.lastKeepAnimating = true;
            this.CurrentValue = this.InitialValue;
            if (this.StopsAt == 0)
            {
                this.StopsAt = this.TargetDuration;
            }

            if (this.CalculateDeAcceleration)
            {
                this.deacceleration = (-2.0 * (double)(this.TargetValue - this.InitialValue)) / ((this.StopsAt - this.StartsAt) * (this.StopsAt - this.StartsAt));
                this.velocity = -this.deacceleration * (this.StopsAt - this.StartsAt);
            }
            else
            {
                this.velocity = (double)(this.TargetValue - this.InitialValue) / (double)(this.StopsAt - this.StartsAt);
                this.deacceleration = 0;
            }
            this.initialTicks = Environment.TickCount;

            // TODO: Check viability
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

            if (Environment.TickCount >= this.initialTicks + this.StopsAt)
            {
                this.lastKeepAnimating = false;
                this.CurrentValue = this.TargetValue;
            }
            else if (Environment.TickCount - this.initialTicks >= this.StartsAt)
            {
                this.CurrentValue = (int)(this.InitialValue + (0.5 * ((this.velocity * 2) + (this.deacceleration * ((Environment.TickCount - this.StartsAt) - this.initialTicks)))
                                * ((Environment.TickCount - this.StartsAt) - this.initialTicks)));
            }

            this.OnAnimation(this.CurrentValue);
            return this.lastKeepAnimating;
        }
    }
}
