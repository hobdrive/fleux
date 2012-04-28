namespace Fleux.UIElements.Grid
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Core.Dim;
    using Core.GraphicsHelpers;
    using Core.Scaling;

    public class Grid : Canvas
    {
        private readonly List<GridCell> cells = new List<GridCell>();
        private MeasureDefinition[] columns;
        private MeasureDefinition[] rows;

        public MeasureDefinition[] Columns
        {
            get { return this.columns ?? (this.columns = new MeasureDefinition[] { "Auto" }); }
            set { this.columns = value; }
        }

        public MeasureDefinition[] Rows
        {
            get { return this.rows ?? (this.rows = new MeasureDefinition[] { "Auto" }); }
            set { this.rows = value; }
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public UIElement this[int row, int column]
        {
            set { this.Add(row, column, value); }
        }

        public void Add(int row,
                        int column,
                        int rowSpan,
                        int columnSpan,
                        UIElement element)
        {
            // Remove any other content for this cell
            // This doesn't include row or column spans.
            this.cells.Where(c => c.Row == row && c.Column == column).ToList().ForEach(c => this.cells.Remove(c));
            this.cells.Add(new GridCell
                               {
                                   Column = column,
                                   Row = row,
                                   ColumnSpan = columnSpan,
                                   RowSpan = rowSpan,
                                   Content = element
                               });
            this.AddElement(element);
            RelocateCells();
        }

        public void RelocateCells()
        {
            int x = this.Location.X.ToPixels();
            for (var column = 0; column < this.columns.Count(); column++)
            {
                int y = this.Location.Y.ToPixels();
                for (var row = 0; row < this.rows.Count(); row++)
                {
                    this.cells.Where(c => (c.Column == column) && (c.Row == row)).ToList()
                        .ForEach(c =>
                                     {
                                         c.Content.Location = new Point(x, y);
                                         c.Content.Size = new Size(this.Columns[column].Value, this.Rows[row].Value);
                                     });
                    y += this.Rows[row].Value;
                }
                x += this.Columns[column].Value;
            }
        }

        public void Add(int row, int column, UIElement element)
        {
            this.Add(row, column, 1, 1, element);
        }
    }
}
