namespace Fleux.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class StoryBoard
    {
        private readonly List<IAnimation> animations = new List<IAnimation>();
        private static StoryBoard sb;
        private Thread animationThread;
        private bool stopAnimation;

        public static void Play(IAnimation animation)
        {
            var sb = new StoryBoard();
            sb.AddAnimation(animation);
            sb.AnimateSync();
        }

        public static void Play(params IAnimation[] animations)
        {
            var sb = new StoryBoard();
            sb.AddAnimations(animations);
            sb.AnimateSync();
        }

        public static void CancelAnimations()
        {
            if (sb != null)
                sb.CancelAsyncAnimate();
        }

        public static void BeginPlay(IAnimation animation)
        {
            BeginPlay(new IAnimation[]{ animation });
        }

        public static void BeginPlay(params IAnimation[] animations)
        {
            bool processed = false;

            if (sb != null)
            {
                lock (sb)
                {
                    if (sb != null)
                    {
                        sb.AddAnimations(animations);
                        processed = true;
                    }
                }
            }
            if (!processed)
            {
                sb = new StoryBoard();
                sb.AddAnimations(animations);
                sb.BeginAnimate(() => sb = null);
            }
        }

        public void AddAnimation(IAnimation a)
        {
            lock(animations)
                this.animations.Add(a);
            if (this.animationThread != null)
            {
                a.Reset();
            }
        }

        public void BeginAnimate(Action onAnimateCompleted)
        {
            if (this.animationThread == null)
            {
                var at = new Thread(() =>
                {
                    this.AnimateSync();
                    onAnimateCompleted();
                    this.animationThread = null;
                });
                this.animationThread = at;
                at.Start();
            }
        }

        public void BeginAnimate()
        {
            this.BeginAnimate(() => { });
        }

        public void AnimateSync()
        {
            IAnimation[] lanimations = null;
            lock(this.animations)
                lanimations = this.animations.ToArray();
            
            foreach(var a in lanimations)
                a.Reset();
            var keepAnimating = true;
            while (!this.stopAnimation && keepAnimating)
            {
                lock (this)
                {
                    keepAnimating = lanimations.Aggregate(false, (current, animation) => (animation.Animate() || current));
                }
                // why that???
                //System.Windows.Forms.Application.DoEvents();
            }
            this.stopAnimation = false;
        }

        public void CancelAsyncAnimate()
        {
            Thread at = this.animationThread;
            if (at != null)
            {
                this.stopAnimation = true;
                at.Join(1000);
                if (this.stopAnimation)
                {
                    // If the animation thread takes longer
                    // than 1 sec to stop, then we kill it
                    at.Abort();
                    this.stopAnimation = false;
                }
                this.animationThread = null;
            }
        }

        public void AddAnimations(params IAnimation[] newAnimations)
        {
            lock (this)
            {
                foreach (var animation in newAnimations)
                {
                    this.AddAnimation(animation);
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                this.animations.Clear();
            }
        }
    }
}
