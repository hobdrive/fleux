namespace Fleux.UIElements
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;

    public class StackPanel : Canvas
    {
        
        public int Padding{ get; set; }
    
        public StackPanel()
        {
            base.AutoResize = false;
            this.Padding = 0;
            this.SizeChanged += (s, e) => this.Relayout();
        }

        public override void AddElement(UIElement element)
        {
            base.AddElement(element);
            this.Relayout();
        }

        public void Relayout()
        {
            int y = 0;
            UIElement lastch = null;
            foreach (var i in this.Children)
            {
                i.Location = new Point(i.Location.X, y);
                i.ResizeForWidth(this.Size.Width);
                y += i.Size.Height + Padding;
                lastch = i;
            }
            if (lastch != null && lastch.Location.Y + lastch.Size.Height > this.Size.Height)
                this.Height = lastch.Location.Y + lastch.Size.Height + Padding;
        }
    }
}
