namespace Fleux.Animations
{
    using System.Collections.Generic;

    public class AnimationGroup : List<IAnimation>, IAnimation
    {
        private int duration;
        private bool cancelled;

        public AnimationGroup(IEnumerable<IAnimation> collection) : base(collection)
        {
        }

        public int Duration
        {
            get
            {
                return this.duration;
            }

            set
            {
                this.duration = value;
                this.ForEach(e => e.Duration = this.duration);
            }
        }

        public void Reset()
        {
            this.ForEach(a => a.Reset());
        }

        public bool Animate()
        {
            if (this.cancelled) 
            { 
                return false; 
            }

            bool keepAnimating = false;
            this.ForEach(a => keepAnimating = a.Animate());
            return keepAnimating;
        }

        public void Cancel()
        {
            this.cancelled = true;
        }
    }
}
