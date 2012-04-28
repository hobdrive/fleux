using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Drawing;

namespace WindowsPhone7Sample.Elements
{
    public class PeopleTile : Tile
    {
        private int currentInnerTile = 0;

        public PeopleTile(int x, int y)
            : base("People", "People.png", x, y)
        {
            // this.StartTileAnimation();
        }

        protected override void DrawTile(Fleux.Core.GraphicsHelpers.IDrawingGraphics drawingGraphics)
        {
            base.DrawTile(drawingGraphics);
            var tileX = this.currentInnerTile / 3;
            var tileY = this.currentInnerTile % 3;
            drawingGraphics.Color(Color.FromArgb(107,194,236)).FillRectangle(new Rectangle(58 * tileX, 58 * tileY, 57, 57));
        }

        private void StartTileAnimation()
        {
            var timer = new Timer(new TimerCallback(o => { this.MoveInnerTile(); })
                , null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(500));
            ;
        }

        private void MoveInnerTile()
        {
            this.currentInnerTile = new Random().Next(8);
            this.Update();
        }
    }
}
