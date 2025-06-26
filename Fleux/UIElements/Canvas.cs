using Fleux.UIElements.Grid;
using System.Collections.Generic;
using System.Drawing;
using Fleux.Core.GraphicsHelpers;

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
             

        public virtual void AddElement(UIElement element)
        {
            lock(child_lock)
            {
                if (this.Children.Contains(element))
                    return;
                this.Children.Add(element);
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

        //Rectangle lastVisibleRect;
        public static int drawtime;
        public static int CanvasDrawExceptions;

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            //if (lastVisibleRect != drawingGraphics.VisibleRect
            int ctime = System.Environment.TickCount;

            var visible = this.SafeChildren.Where(i => i.Visible && i.Bounds.IntersectsWith(drawingGraphics.VisibleRect));
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
                try
                {
#if xDEBUG
                    System.Console.WriteLine("Canvas draw " + e.GetType().ToString() + " vis: "+visible.Count + "tot: "+this.ChildrenCount);
#endif
                    /*if (e.Transformation != null)
                    {
                        //Bitmap b = new Bitmap(e.Size.Width, e.Size.Height);
                        //Graphics g =  Graphics.FromImage(b);
                        //var cdg = DrawingGraphics.FromGraphicsAndRect(g, b, new Rectangle(e.Location.X, e.Location.Y, e.Size.Width, e.Size.Height));
                        e.Draw(cdg);
                        //b.Dispose();
                        //g.Dispose();
                    }else*/
                    e.Draw(drawingGraphics.CreateChild(e.Location, e.Transformation));
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Canvas draw exception " + ex);
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    CanvasDrawExceptions++;
                }
            }
            ctime = System.Environment.TickCount - ctime;
#if xxDEBUG
            drawingGraphics.Color(Color.Red);
            drawingGraphics.MoveTo(0,0);
            drawingGraphics.DrawRectangle(0, 0, this.Size.Width, this.Size.Height);
            drawingGraphics.MoveTo(0,10);
            drawingGraphics.DrawText("ctime: " + ctime);
#endif
            drawtime += ctime;
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