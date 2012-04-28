namespace Fleux.UIElements.Grid
{
    using System.Drawing;
    using Core;
    using Core.GraphicsHelpers;
    using Core.Scaling;

    public class GridCell
    {
        public UIElement Content { get; set; }
        
        public int Column { get; set; }
        
        public int Row { get; set; }
        
        public int ColumnSpan { get; set; }
        
        public int RowSpan { get; set; }

        public Rectangle CalculateBounds()
        {
            // TODO: This code should be reviewed as it was a direct port from the IUIElement approach
            // if (!this.Content.TouchableState.BoundsUpdated)
            {
                var gr = DrawingGraphics.FromGraphicsLocationMaxWidth(
                    FleuxApplication.DummyGraphics,
                    FleuxApplication.DummyImage,
                    this.Content.Location.X,
                    this.Content.Location.Y,
                    this.Content.Size.Width);
                this.Content.Draw(gr);
                this.Content.Location = gr.Location;
                this.Content.Size = new Size(gr.Right, gr.Bottom);
            }

            return this.Content.Bounds;
        }
    }
}
