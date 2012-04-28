namespace Fleux.UIElements
{
    using System;
    using System.Drawing;
    using System.Linq;
    using Animations;
    using Core;
    using Core.GraphicsHelpers;

    public class Canvas : UIElement
    {
        public Canvas()
        {
            this.EntranceAnimation = new ForwarderAnimation(() => new AnimationGroup(this.Children.Where(e => e.EntranceAnimation != null).Select(e => e.EntranceAnimation)));
            this.ExitAnimation = new ForwarderAnimation(() => new AnimationGroup(this.Children.Where(e => e.ExitAnimation != null).Select(e => e.ExitAnimation)));
        }

        public override System.Drawing.Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
        }

        public virtual void AddElement(UIElement element)
        {
            if (this.Children.Contains(element))
                return;
            this.Children.Add(element);
            element.Parent = this;
            this.Size = new Size(Math.Max(element.Bounds.Right, this.Size.Width), Math.Max(element.Bounds.Bottom, this.Size.Height));
            element.Updated = this.Update;
        }

        public virtual void RemoveElement(UIElement element)
        {
            this.Children.Remove(element);
            element.Parent = null;
            /*
            this.Size = new Size(0,0);
            this.Children.ForEach( (e) => {
                this.Size = new Size(Math.Max(e.Bounds.Right, this.Size.Width), Math.Max(e.Bounds.Bottom, this.Size.Height));
            });
            */
            element.Updated = null;
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            this.Children
                .Where(i => i.Bounds.IntersectsWith(drawingGraphics.VisibleRect))
                .ToList()
                .ForEach(e => e.Draw(drawingGraphics.CreateChild(e.Location, e.TransformationScaling, e.TransformationCenter)));
        }

        public virtual void AddElementAfter(UIElement element, UIElement sibling)
        {
            if (!base.Children.Contains(element))
            {
                int index = base.Children.IndexOf(sibling);
                if (index == -1)
                {
                    index = base.Children.Count - 1;
                }
                base.Children.Insert(index + 1, element);
                element.Parent = this;
                base.Size = new Size(Math.Max(element.Bounds.Right, base.Size.Width), Math.Max(element.Bounds.Bottom, base.Size.Height));
                element.Updated = new Action(this.Update);
            }
        }

        public virtual UIElement Child(string id)
        {
            return base.Children.FirstOrDefault<UIElement>(c => (c.ID == id));
        }

        public void Clear()
        {
            this.Children.Clear();
        }
    }
}