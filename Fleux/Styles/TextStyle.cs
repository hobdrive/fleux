namespace Fleux.Styles
{
    using System.Drawing;
    using Core.Scaling;
    using Core.GraphicsHelpers;

    public class TextStyle : Style
    {
        public TextStyle(string fontFamily, int fontSize, Color foreGround, ThicknessStyle thickness)
        {
            this.FontSize = fontSize;
            this.FontFamily = fontFamily;
            this.Foreground = foreGround;
            this.Thickness = thickness;
        }

        public TextStyle(string fontFamily, int fontSize, Color foreGround)
            : this(fontFamily, fontSize, foreGround, null)
        {
        }

        public TextStyle(TextStyle parent)
            : this(parent.FontFamily, parent.FontSize, parent.Foreground, parent.Thickness)
        {
        }


        public int FontSize { get; set; }

        public string FontFamily { get; set; }

        public Color Foreground { get; set; }

        public ThicknessStyle Thickness { get; set; }

        public bool Bold { get; set; }

        public void ApplyTo(IDrawingGraphics gr)
        {
            gr.FontName(this.FontFamily);
            gr.FontSize(this.FontSize);
            gr.Color(this.Foreground);
            gr.Bold(this.Bold);
        }
    }
}
