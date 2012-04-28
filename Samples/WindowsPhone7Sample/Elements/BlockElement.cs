using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements;

namespace WindowsPhone7Sample.Elements
{
    public class BlockElement : UIElement
    {
        public override void Draw(Fleux.Core.GraphicsHelpers.IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.Color(System.Drawing.Color.Blue).FillRectangle(0,0,this.Size.Width, this.Size.Height);
        }
    }
}
