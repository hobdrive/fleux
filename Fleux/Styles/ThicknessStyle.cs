namespace Fleux.Styles
{
    using Core.Dim;

    public class ThicknessStyle
    {
        public ThicknessStyle(int borderThickness, int margin, int padding)
        {
            this.BorderThickness = new Length(borderThickness);
            this.Margin = new Length(margin);
            this.Padding = new Length(padding);
        }

        public Length BorderThickness { get; set; }

        public Length Margin { get; set; }

        public Length Padding { get; set; }
    }
}
