using Fleux.UIElements.Grid;
using System.Collections.Generic;
using System.Drawing;
using Fleux.Core.GraphicsHelpers;
using Fleux.Core.Scaling;

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
        public event Action ContentChanged;

        public bool AutoResize{ get; set; }

        object child_lock = new object();

        public static bool PerfData = false;

        public Canvas()
        {
            AutoResize = true;
            this.EntranceAnimation = new ForwarderAnimation(() => new AnimationGroup(this.SafeChildren.Where(e => e.EntranceAnimation != null).Select(e => e.EntranceAnimation)));
            this.ExitAnimation = new ForwarderAnimation(() => new AnimationGroup(this.SafeChildren.Where(e => e.ExitAnimation != null).Select(e => e.ExitAnimation)));
        }

        public override System.Drawing.Rectangle Bounds
        {
            get
            {
                return base.Bounds;
            }
        }

        public virtual List<UIElement> SafeChildren{
            get{
                lock(child_lock){
                    return this.Children.ToList();
                }
            }
        }
        
        void OnElementVisibleChanged(object sender, EventArgs e)
        {
            changedChildren = true;
        }

        public virtual void AddElement(UIElement element)
        {
            lock (child_lock)
            {
                if (this.Children.Contains(element))
                    return;
                this.Children.Add(element);
                element.VisibleChanged += OnElementVisibleChanged;
            }
            element.Parent = this;
            if (AutoResize)
                this.Size = new Size(Math.Max(element.Bounds.Right, this.Size.Width), Math.Max(element.Bounds.Bottom, this.Size.Height));
            element.Updated = this.Update;
            if (ContentChanged != null) ContentChanged();
        }

        public virtual void RemoveElement(UIElement element)
        {
            lock(child_lock)
            {
                this.Children.Remove(element);
                element.VisibleChanged -= OnElementVisibleChanged;
            }
            element.Parent = null;
            /*
            this.Size = new Size(0,0);
            this.Children.ForEach( (e) => {
                this.Size = new Size(Math.Max(e.Bounds.Right, this.Size.Width), Math.Max(e.Bounds.Bottom, this.Size.Height));
            });
            */
            element.Updated = null;
            if (ContentChanged != null) ContentChanged();
        }

        public static int drawtime;
        public static int CanvasDrawExceptions;

        List<UIElement> visibleChildren = null;
        Rectangle lastVisibleRect;
        bool changedChildren = false;
        List<UIElement> GetVisibleCachedChindren(Rectangle visibleRect)
        {
            if (lastVisibleRect == visibleRect && visibleChildren != null && !changedChildren)
                return visibleChildren;
            lastVisibleRect = visibleRect;
            visibleChildren = this.SafeChildren.Where(i => i.Visible && i.Bounds.IntersectsWith(visibleRect)).ToList();
            changedChildren = false;
            return visibleChildren;
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            // Optimized
            var visible = GetVisibleCachedChindren(drawingGraphics.VisibleRect);
#if xDEBUG
			if (this is Fleux.UIElements.Panorama.PanoramaSection)
			{
				var ch = this.Children.Where(i => i.Visible);
			}
			var visible = this.Children.Where(i => i.Visible).ToList();
#endif
#if xDEBUG
            if (this.ID == "PanoramaSections"){
                System.Console.WriteLine("section:"+visible.Count + " vrect:"+drawingGraphics.VisibleRect.ToString());
                foreach(var ch in visible)
                    System.Console.WriteLine("child: "+ch.Bounds.ToString());
            }
#endif

            foreach (var e in visible)
            {
                int ctime = System.Environment.TickCount;
                try
                {
#if xDEBUG
                    System.Console.WriteLine("Canvas draw " + e.GetType().ToString() + " vis: "+visible.Count + "tot: "+this.ChildrenCount);
#endif
                    e.Draw(drawingGraphics.CreateChild(e.Location, e.Transformation));
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Canvas draw exception " + ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    CanvasDrawExceptions++;
                }
                ctime = System.Environment.TickCount - ctime;
                if (PerfData)
                {
                    drawingGraphics.Color(Color.Red);
                    drawingGraphics.DrawRectangle(e.Location.X, e.Location.Y, e.Location.X + e.Size.Width, e.Location.Y + e.Size.Height);
                    drawingGraphics.MoveTo(e.Location.X, e.Location.Y);
                    drawingGraphics.FontSize(8.ToPixels());
                    drawingGraphics.DrawText("ct: " + ctime);
                }
                drawtime += ctime;
            }
        }

        /// <summary>
        /// No need to lock addition!
        /// </summary>
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
                if (ContentChanged != null) ContentChanged();
            }
        }

        public virtual UIElement Child(string id)
        {
            return SafeChildren.FirstOrDefault<UIElement>(c => (c.ID == id));
        }

        /// <summary>
        /// Finds the child recursively
        /// </summary>
        public virtual UIElement FindChild(string id)
        {
            foreach(var c in SafeChildren)
            {
                if (c is Canvas)
                {
                    var cc = (c as Canvas).FindChild(id);
                    if (cc != null)
                        return cc;
                }
                if (c.ID == id)
                    return c;
            }
            return null;
        }

        public void Clear()
        {
            lock(child_lock)
            {
                this.Children.Clear();
            }
            if (ContentChanged != null) ContentChanged();
        }
    }
}