namespace Fleux.Animations
{
    using System;
    using Fleux.Core;

    public class FunctionBasedAnimation : IAnimation
    {
        public int From;
        public int To;
        public Action<int> OnAnimation;

        private bool keepAnimating;
        private int initialTicks;

        // From 0.0 to 1.0
        private double proportionalValue;

        public FunctionBasedAnimation(Func<double, double> easeFunction)
        {
            this.EaseFunction = easeFunction;
        }

        public int Duration { get; set; }

        public Func<double, double> EaseFunction { get; set; }

        public int StartAt { get; set; }

        public int StopsAt { get; set; }

        public void Reset()
        {
            if (this.StopsAt == 0)
            {
                this.StopsAt = this.Duration;
            }

            this.keepAnimating = true;
            this.proportionalValue = 0;
            this.initialTicks = Environment.TickCount;
        }

        public bool Animate()
        {
            var ellapsed = Environment.TickCount - this.initialTicks;

            if (ellapsed >= this.StartAt)
            {
                if (this.keepAnimating)
                {
                    var t = (ellapsed - this.StartAt) / (double)(this.StopsAt - this.StartAt);
                    if (t > 1.0 || !FleuxSettings.AnimationMode)
                    {
                        this.keepAnimating = false;
                        this.OnAnimation(this.To);
                    }
                    else
                    {
                        this.proportionalValue = this.EaseFunction(t);
                        this.OnAnimation((int)(this.From + ((this.To - this.From) * this.proportionalValue)));
                    }
                }
            }
            return this.keepAnimating;
        }

        public void Cancel()
        {
            this.keepAnimating = false;
        }

        public static class Functions
        {
            public static Func<double, double> SoftedFluid = x => (1 - Math.Pow((1 - ((x + 1) / 2)) / ((x + 1) / 2), 2));

            public static Func<double, double> SoftedFluidExtended = x => (1 - Math.Pow((1 - ((x + 1) / 2)) / ((x + 0.35) / 0.7), 2));

            public static Func<double, double> Cubic = x => Math.Pow(x - 1, 3) + 1;

            public static Func<double, double> CubicReverse = x => -Math.Pow((1 - x) - 1, 3);

            public static Func<double, double> Square = x => 1 - Math.Pow(x - 1, 2);

            public static Func<double, double> Sin = x => Math.Sin(x * 1.7);

            public static Func<double, double> ExpressiveSin = x => Math.Sin(Math.Sqrt(x) * 1.7);

            public static Func<double, double> BounceEntranceSin = x => Math.Sin(Math.Sqrt(x) * 2) / Math.Sin(2);

            public static Func<double, double> BounceExitSin = x => 1-BounceEntranceSin(1-x);

            public static Func<double, double> Linear = x => x;
        }
    }
}
