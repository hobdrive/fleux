namespace Fleux.UIElements.Panorama
{
    using System.Drawing;
    using Core.GraphicsHelpers;
    using Styles;

    public class PanoramaSection : Canvas
    {
        private string title;
        public TextStyle Style{ get; set; }

        public PanoramaSection()
        {
            this.Body = new Canvas { Size = new Size(380, 480), Location = new Point(0, 100) };
            this.AddElement(this.Body);
            Style = MetroTheme.PhoneTextPanoramaSectionTitleStyle;
        }

        public PanoramaSection(string title)
            : this()
        {
            this.title = title;
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }

        public Canvas Body { get; private set; }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.Style(this.Style)
                .DrawText(this.title)
                .MoveTo(0, 0);

            base.Draw(drawingGraphics);
        }
    }
}
