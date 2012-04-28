namespace Fleux.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text;
    using UIElements;

    public static class DefaultAnimations
    {
        public static UIElement SetHorizontalDefaultAnimations(UIElement target, bool fromLeft)
        {
            var random = new Random();
            var x = target.Location.X;
            target.EntranceAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.BounceEntranceSin)
            {
                From = x + ((fromLeft ? 1 : -1) * (-1000 + random.Next(1000 - x) - target.Size.Width)),
                To = x,
                OnAnimation = v => target.Location = new Point(v, target.Location.Y)
            };
            target.ExitAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.CubicReverse)
            {
                To = x + ((fromLeft ? 1 : -1) * (-1000 + random.Next(1000 - x) - target.Size.Width)),
                From = x,
                OnAnimation = v => target.Location = new Point(v, target.Location.Y)
            };
            return target;
        }

        public static UIElement SetVerticalDefaultAnimations(UIElement target, bool fromTop)
        {
            return SetVerticalDefaultAnimations(target,
                fromTop,
                FunctionBasedAnimation.Functions.BounceEntranceSin,
                FunctionBasedAnimation.Functions.CubicReverse);
        }

        public static UIElement SetVerticalDefaultAnimations(UIElement target, bool fromTop, Func<double, double> entranceFunc, Func<double, double> exitFunc)
        {
            var random = new Random();
            var y = target.Location.Y;
            //cail: exception: 1000-y is negative
            target.EntranceAnimation = new FunctionBasedAnimation(entranceFunc)
            {
                From = y + ((fromTop ? 1 : -1) * (-1000 + random.Next(1000) - y - target.Size.Height)),
                To = y,
                OnAnimation = v => target.Location = new Point(target.Location.X, v)
            };
            target.ExitAnimation = new FunctionBasedAnimation(exitFunc)
            {
                To = y + ((fromTop ? 1 : -1) * (-1000 + random.Next(1000) - y - target.Size.Height)),
                From = y,
                OnAnimation = v => target.Location = new Point(target.Location.X, v)
            };
            return target;
        }

        public static UIElement AnimateHorizontalEntrance(this UIElement target, bool fromLeft)
        {
            return SetHorizontalDefaultAnimations(target, fromLeft);
        }

        public static UIElement AnimateVerticalEntrance(this UIElement target, bool fromTop)
        {
            return SetVerticalDefaultAnimations(target, fromTop);
        }
    }
}
