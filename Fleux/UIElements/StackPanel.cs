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
        
        int columns = 1;
        
        public int Columns{
            get{
                return columns;
            }
            set{
                columns = value;
                Relayout();
            }
        }
    
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
            int no = 0;
            UIElement lastch = null;
            foreach (var i in this.Children)
            {
                var cwidth = (double)this.Width/columns - Padding;
                int x = (int)((no == 0 ? i.Location.X : 0) + (cwidth+Padding*2)*(no % columns));
                i.Location = new Point(x, y);
                i.ResizeForWidth((int)cwidth);
                
                no = (no+1) % columns;
                if (no % columns == 0)
                    y += i.Size.Height + Padding;
                lastch = i;
            }
            if (lastch != null && lastch.Location.Y + lastch.Size.Height > this.Size.Height)
                this.Height = lastch.Location.Y + lastch.Size.Height + Padding;
        }
    }
}
