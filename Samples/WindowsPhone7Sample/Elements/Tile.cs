using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements;
using Fleux.Core;
using System.Drawing;
using Fleux.Core.GraphicsHelpers;
using Fleux.Styles;

namespace WindowsPhone7Sample.Elements
{
    public class Tile : ImageElement
    {
        private readonly TextElement text;

        public Tile(string tileName, string imageName, int x, int y) 
            : base(ResourceManager.Instance.GetBitmapFromEmbeddedResource(imageName))
        {
            this.Size = new Size(173, 173);
            this.text = new TextElement(tileName) { Style = MetroTheme.TileTextStyle, Location = new Point(13, 140) };
            this.Location = new Point(x, y);
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            this.DrawTile(drawingGraphics);
            this.DrawTileText(drawingGraphics);
        }

        protected virtual void DrawTileText(IDrawingGraphics drawingGraphics)
        {
            this.text.Draw(drawingGraphics.CreateChild(this.text.Location, this.text.TransformationScaling, this.text.TransformationCenter));
        }

        protected virtual void DrawTile(IDrawingGraphics drawingGraphics)
        {
            base.Draw(drawingGraphics);
        }
    }
}
