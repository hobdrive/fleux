namespace Fleux.Animations
{
    using System;
    using System.Linq;

    public class ForwarderAnimation : IAnimation
    {
        private readonly Func<IAnimation> animationGetter;

        public ForwarderAnimation(Func<IAnimation> animationGetter)
        {
            this.animationGetter = animationGetter;
        }

        public int Duration
        {
            get 
            { 
                return this.animationGetter().Duration; 
            }

            set
            {
                var animation = this.animationGetter();
                if (animation != null)
                {
                    animation.Duration = value;
                }
            }
        }

        public void Reset()
        {
            var animation = this.animationGetter();
            if (animation != null)
            {
                animation.Reset();
            }
        }

        public bool Animate()
        {
            var animation = this.animationGetter();
            if (animation != null)
            {
                return animation.Animate();
            }
            else
            {
                return false;
            }
        }

        public void Cancel()
        {
            var animation = this.animationGetter();
            if (animation != null)
            {
                animation.Cancel();
            }
        }
    }
}
