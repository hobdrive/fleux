namespace Fleux.UIElements.Panorama
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Animations;
    using Core.GraphicsHelpers;
    using Styles;

    public class PanoramaElement : Canvas
    {
        protected IAnimation animation;
        protected StoryBoard sb = new StoryBoard();
        protected int position = 0;
        protected int finePosition;
        protected bool isPanning;
        protected int currentSectionIndex;

        public bool IsPanoramaAnimating{ get; protected set; }
        public static bool RubberEdgesDefault = true;
        public bool RubberEdges{ get; set; }

        public PanoramaElement()
        {
            this.RubberEdges = RubberEdgesDefault;
            this.Background = new Canvas();
            this.Title = new Canvas();
            this.Sections = new Canvas { Location = new Point(0, 130), Size = new Size(1800, 600) };
            this.AddElement(this.Background);
            this.AddElement(this.Title);
            this.AddElement(this.Sections);

            this.SetAnimations();
        }

        int sectionSpace = 425;
        public int SectionSpace {
            get{ return sectionSpace; }
            set{ sectionSpace = value; }
        }

        public Canvas Background { get; set; }

        public Canvas Title { get; set; }

        public Canvas Sections { get; set; }

        public event Action<PanoramaElement, PanoramaSection> OnSectionChange;

        public int FinePosition
        {
            get
            {
                return this.finePosition;
            }

            set
            {
                this.finePosition = value;
                this.CalculateFactors();
            }
        }

        public int CurrentSectionIndex
        {
            get
            {
                return this.currentSectionIndex;
            }

            set
            {
                if (value >= 0 && value < this.Sections.ChildrenCount)
                {
                    this.currentSectionIndex = value;
                }
                this.IsPanoramaAnimating = true;
                var vv = this.currentSectionIndex * this.sectionSpace;
                var atype = (RubberEdges && (FinePosition < 0 || FinePosition > (this.Sections.ChildrenCount-1)*this.SectionSpace)) ?
                               FunctionBasedAnimation.Functions.BounceEntranceSin : FunctionBasedAnimation.Functions.SoftedFluid;
                this.StoryBoardPlay(
                    new FunctionBasedAnimation(atype)
                    {
                        From = this.finePosition,
                        To = this.currentSectionIndex * this.sectionSpace,
                        Duration = 400,
                        OnAnimation = v => { this.FinePosition = v; this.Update(); }
                    }
                );
                
                // TODO!
                // TODO - moveout into OnAnimationEnd!
                if (this.OnSectionChange != null)
                    this.OnSectionChange(this, CurrentSection);
            }
        }

        public PanoramaSection CurrentSection {
            get{
                if (CurrentSectionIndex >= this.Sections.ChildrenCount)
                    return null;
                return (PanoramaSection)this.Sections[CurrentSectionIndex];
            }
            set{
                for(int idx = 0; idx < this.Sections.ChildrenCount; idx++)
                    if (this.Sections[idx] == value){
                        CurrentSectionIndex = idx;
                        return;
                    }
            }
        }

        public void SetAnimations()
        {
            this.EntranceAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                From = -this.Sections.Size.Width,
                To = this.FinePosition,
                OnAnimation = v => { this.FinePosition = v; this.Update(); }
            };
            this.ExitAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.CubicReverse)
            {
                From = this.FinePosition,
                To = -this.Sections.Size.Width,
                OnAnimation = v => { this.FinePosition = v; this.Update(); }
            };
        }

        public override bool Flick(Point from, Point to, int millisecs, Point startPoint)
        {
            if (this.animation != null)
            {
                this.animation.Cancel();
            }
            if (!base.Flick(from, to, millisecs, startPoint) && (Math.Abs(to.X - from.X) > Math.Abs(to.Y - from.Y)))
            {
                this.CurrentSectionIndex += -Math.Sign(to.X - from.X);
            }
            else if (this.isPanning)
            {
                this.isPanning = false;
                this.CurrentSectionIndex = this.CurrentSectionIndex;
            }

            return true;
        }

        public override bool Tap(Point p)
        {
            if (this.isPanning)
            {
                this.isPanning = false;
                this.CurrentSectionIndex = this.CurrentSectionIndex;
            }
            return base.Tap(p);
        }
        public override bool Pan(Point from, Point to, bool done, Point startPoint)
        {
            if (this.animation != null)
            {
                this.animation.Cancel();
            }
            if (!base.Pan(from, to, done, startPoint))
            {
                this.isPanning = !done;
                this.IsPanoramaAnimating = !done;

                // Validate if should we handle this Pan
                if (Math.Abs(to.X - from.X) > Math.Abs(to.Y - from.Y))
                {
                    var diff = to.X - from.X;
                    if (this.RubberEdges && (this.FinePosition <= 0 || this.FinePosition >= (this.Sections.ChildrenCount-1)*this.SectionSpace))
                        diff = diff/10;
                    this.FinePosition -= diff;
                    this.Update();
                }
                if (done)
                {
                    this.CurrentSectionIndex = this.CurrentSectionIndex;
                }
            }
            return true;
        }

        public void AddSection(PanoramaSection newSection)
        {
            this.AddSection(newSection, false);
        }

        public void AddSection(PanoramaSection newSection, bool wider)
        {
            if (this.Sections.ChildrenEnumerable.Contains<UIElement>(newSection))
                return;
            newSection.Size = new Size(this.Size.Width * (wider ? 2 : 1), this.Size.Height - this.Sections.Location.Y);
            newSection.Location = new Point(this.sectionSpace * this.Sections.ChildrenCount, 0);
            this.Sections.AddElement(newSection);
            // hack for double sections, think all this should be refactored do we need wide sections???
            if (wider)
                this.Sections.AddElement(new Canvas());
            this.SetAnimations();
        }

        public void RemoveSection(PanoramaSection section)
        {
            UIElement[] elementArray = this.Sections.ChildrenEnumerable.ToArray<UIElement>();
            int num = 0;
            for (int i = 0; i < elementArray.Length; i++)
            {
                var s = elementArray[i];
                if (s == section)
                {
                    this.Sections.RemoveElement(section);
                    if ((this.CurrentSectionIndex >= i) && (this.CurrentSectionIndex > 0))
                    {
                        this.currentSectionIndex--;
                        this.FinePosition = this.CurrentSectionIndex * this.sectionSpace;
                    }
                    num = 1;
                }
                else
                {
                    s.Location = new Point(this.sectionSpace * (i - num), 0);
                }
            }
            this.SetAnimations();
        }

        protected void CalculateFactors()
        {
            var titlef = (double)(this.Title.Size.Width - this.Size.Width) / (double)(this.Sections.Size.Width - this.Size.Width);
            var backgroundf = (double)(this.Background.Size.Width - this.Size.Width) / (double)(this.sectionSpace * (this.Sections.ChildrenCount - 1));
            if (backgroundf < 0)
                backgroundf = 0;
            this.Background.Location = new Point((int)(-this.finePosition * backgroundf), 0);
            this.Title.Location = new Point((int)(-this.finePosition * titlef), this.Title.Location.Y);
            this.Sections.Location = new Point((int)(-this.finePosition * 1), this.Sections.Location.Y);
        }

        private void StoryBoardPlay(IAnimation animation)
        {
            if (this.animation != null)
            {
                this.animation.Cancel();
            }
            this.animation = animation;
            sb.CancelAsyncAnimate();
            sb.Clear();
            sb.AddAnimations(this.animation,
                // hack? implement it better?
                new CommitStoryboardAnimation(){
                    StartsAt = 400,
                    Duration = 500,
                    CommitAction = () => { this.IsPanoramaAnimating = false; }
                }
            );
            sb.BeginAnimate();
        }
    }
}
