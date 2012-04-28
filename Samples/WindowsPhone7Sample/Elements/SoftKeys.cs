using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements;
using System.Drawing;
using Fleux.Core;
using System.Windows.Forms;

namespace WindowsPhone7Sample.Elements
{
    public class SoftKeys : Canvas
    {
        public SoftKeys()
        {
            this.AddElement(new ImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("SKBack.png")) { Location = new Point(0, 0), Size = new Size(480, 60) });
            //this.AddElement(new NewImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("SKBack.png")) { Location = new Point(50, 0), Size = new Size(60, 50) });
            //this.AddElement(new NewImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("SKWindows.png")) { Location = new Point(210, 0), Size = new Size(60, 50) });
            //this.AddElement(new NewImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("SKSearch.png")) { Location = new Point(370, 0), Size = new Size(60, 50) });
        }

        public override bool  Tap(Point p)
        {
            Application.Exit();
            return true;
        }

        public override void Draw(Fleux.Core.GraphicsHelpers.IDrawingGraphics drawingGraphics)
        {
            //drawingGraphics.Color(Color.Black).FillRectangle(new Rectangle(0, 0, this.Size.Width, this.Size.Height));
            base.Draw(drawingGraphics);
        }
    }
}
