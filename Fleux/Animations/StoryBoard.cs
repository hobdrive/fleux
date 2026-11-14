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
        private readonly object animationThreadLock = new object();
        private Thread animationThread;
        private volatile bool stopAnimation;

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
            lock (this.animationThreadLock)
            {
                if (this.animationThread != null)
                    return;

                this.stopAnimation = false;
                var at = new Thread(() =>
                {
                    try
                    {
                        this.AnimateSync();
                        onAnimateCompleted();
                    }
                    finally
                    {
                        lock (this.animationThreadLock)
                            this.animationThread = null;
                    }
                });
                // Mono's Join must not observe a thread while Start is still running.
                // The worker also takes this lock before clearing the published thread.
                at.Start();
                this.animationThread = at;
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
            Thread at;
            lock (this.animationThreadLock)
            {
                at = this.animationThread;
                if (at == null)
                    return;

                this.stopAnimation = true;
            }

            // Do not hold the lifecycle lock while waiting for the worker's finally.
            // Keep the stop request and thread until it actually exits after a timeout.
            if (at != Thread.CurrentThread)
                at.Join(1000);
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
