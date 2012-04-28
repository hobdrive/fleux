namespace Fleux.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class Animation : IAnimation
    {
        // Animation Values
        public int InitialValue = 0;
        public double InitialVelocity = 0;
        public int TargetValue = 0;
        public double Acceleration = 0.0005;
        public Action<int> OnAnimation;

        private int initialTicks = 0;
        private double currentVelocity = 0;
        private bool lastKeepAnimating;

        public int CurrentValue { get; private set; }

        public int Duration { get; set; }

        public void Reset()
        {
            // Initialize values
            this.CurrentValue = this.InitialValue;
            this.currentVelocity = this.InitialVelocity;
            this.initialTicks = Environment.TickCount;
            this.lastKeepAnimating = true;

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

            bool keepAnimating = !(Math.Abs(this.CurrentValue - this.InitialValue) >= Math.Abs(this.TargetValue - this.InitialValue));
            
            if (!keepAnimating)
            {
                this.CurrentValue = this.TargetValue;
            }
            else
            {
                this.currentVelocity = this.InitialVelocity + (this.Acceleration * (Environment.TickCount - this.initialTicks));
                this.CurrentValue = this.InitialValue + (int)((double)(0.5 * (this.InitialVelocity + this.currentVelocity) * (Environment.TickCount - this.initialTicks)));
            }

            this.OnAnimation(this.CurrentValue);

            this.lastKeepAnimating = keepAnimating;
            
            return keepAnimating;
        }
    }
}
