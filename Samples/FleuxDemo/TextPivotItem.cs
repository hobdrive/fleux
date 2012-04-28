namespace FleuxDemo
{
    using System;
    using Fleux.UIElements.Pivot;
    using Fleux.UIElements;
    using System.Drawing;
    using Fleux.Styles;
    using Fleux.Core.GraphicsHelpers;

    public class TextPivotItem : PivotItem
    {
        private int textWidth;
        private int multilineTextHeight;

        public TextPivotItem(int width)
        {
            this.Title = "text";
            this.textWidth = width - 3;

            var sv = new ScrollViewer
            {
                Location = new Point(21, 0),
                Size = new Size(width, 10), // Height will be set by the Pivot
                ShowScrollbars = true,
                VerticalScroll = true
            };

            sv.Content = new DelegateUIElement
            {
                Size = new Size(sv.Size.Width, 950),
                DrawingAction = this.DrawTextSamples
            };
            
            this.Body = sv;
        }

        private void DrawTextSamples(IDrawingGraphics gr)
        {
            var text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawText(text).MoveTo(0, gr.Bottom + 10);
            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawMultiLineText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle);
            if (this.multilineTextHeight == 0)
            {
                this.multilineTextHeight = gr.CalculateMultilineTextHeight(text,this.textWidth);
            }
            gr.DrawMultiLineText(text, this.textWidth, this.multilineTextHeight).MoveTo(0, gr.Bottom + 10);

            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawText(text).MoveTo(0, gr.Bottom + 10);
            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawMultiLineText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawMultiLineText(text, this.textWidth, this.multilineTextHeight).MoveTo(0, gr.Bottom + 10);
        }
    }
}
