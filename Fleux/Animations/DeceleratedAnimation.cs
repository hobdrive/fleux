namespace Fleux.Animations
{
    using System;

    public class DeceleratedAnimation : IAnimation
    {
        // Animation Values
        public int InitialValue;
        public double InitialVelocity;
        public double Deceleration = 0.0005;
        public int MinValue;
        public int MaxValue = Int16.MaxValue;
        public Action<int> OnAnimation;

        private int direction;
        private int initialTicks;
        private double currentVelocity;
        private bool lastKeepingAnimating;
        private bool bouncing;
        private int bouncingStarted;

        public DeceleratedAnimation()
        {
            this.MaxVelocity = 5;
        }

        public enum LimitsBehaviorOptions
        {
            None,
            ForceDeceleration
        }

        public LimitsBehaviorOptions LimitsBehavior { get; set; }

        public int CurrentValue { get; private set; }

        public int Duration { get; set; }

        // TODO: Review these values b/c they should be something absolute depending on the current DPI
        public double MaxVelocity { get; set; }

        private static double DefaultDeceleration
        {
            get { return 0.0007; }
        }

        public void Reset()
        {
            // Initialize values
            this.CurrentValue = this.InitialValue;
            this.InitialVelocity = this.InitialVelocity > 0 ? Math.Min(this.InitialVelocity, this.MaxVelocity) : Math.Max(this.InitialVelocity, -this.MaxVelocity);
            this.currentVelocity = this.InitialVelocity;
            this.direction = Math.Sign(this.InitialVelocity);
            this.Deceleration = this.direction * -1 * DefaultDeceleration;
            this.initialTicks = Environment.TickCount;
            this.lastKeepingAnimating = true;

            if (this.LimitsBehavior == LimitsBehaviorOptions.ForceDeceleration)
            {
                // If the acceleration will exceed the limits we should
                // calculate a new acceleration that makes the final value
                // exactly the limit.
                var displacement = this.direction > 0
                                       ? this.MaxValue - this.InitialValue
                                       : this.MinValue - this.InitialValue;
                var forcedAcceleration = (-this.InitialVelocity * this.InitialVelocity) / (2 * displacement);
                if (Math.Abs(forcedAcceleration) > Math.Abs(this.Deceleration))
                {
                    this.Deceleration = forcedAcceleration;
                }
            }

            // TODO: Check viability
        }

        public void Cancel()
        {
            this.lastKeepingAnimating = false;
        }

        public bool Animate()
        {
            if (!this.lastKeepingAnimating)
            {
                return false;
            }

            if (this.bouncing)
            {
                this.currentVelocity = this.InitialVelocity + (this.Deceleration * (Environment.TickCount - this.initialTicks));
                
                // s = 1/2 * (u + v) * t
                this.CurrentValue = this.InitialValue + (int)(0.5 * (this.InitialVelocity + this.currentVelocity) * (Environment.TickCount - this.initialTicks));
                if (Math.Sign(this.currentVelocity) < 0 && this.CurrentValue < this.MaxValue)
                {
                    this.CurrentValue = this.MaxValue;
                    this.lastKeepingAnimating = false;
                }
                else if (Math.Sign(this.currentVelocity) > 0 && this.CurrentValue > this.MinValue)
                {
                    this.CurrentValue = this.MinValue;
                    this.lastKeepingAnimating = false;
                }
            }
            else if (!this.bouncing && this.bouncingStarted == 0 && ((this.CurrentValue > this.MaxValue) || (this.CurrentValue < this.MinValue)))
            {
                // Wait 100 milsec for start bouncing
                this.bouncingStarted = Environment.TickCount;
            }
            else if (this.bouncingStarted != 0 && !this.bouncing && Environment.TickCount - this.bouncingStarted > 100)
            {
                // Start bouncing
                this.InitialVelocity = -this.currentVelocity / 2;
                this.InitialValue = this.CurrentValue;
                this.initialTicks = Environment.TickCount;
                this.bouncing = true;
            }
            else if (!this.bouncing)
            {
                this.currentVelocity = this.InitialVelocity + (this.Deceleration * (Environment.TickCount - this.initialTicks));
                if (Math.Sign(this.currentVelocity) == this.direction)
                {
                    // s = 1/2 * (u + v) * t
                    this.CurrentValue = this.InitialValue + (int)(0.5 * (this.InitialVelocity + this.currentVelocity) * (Environment.TickCount - this.initialTicks));
                }
                else
                {
                    this.lastKeepingAnimating = false;
                }
            }
            this.OnAnimation(this.CurrentValue);
            return this.lastKeepingAnimating;
        }
    }
}