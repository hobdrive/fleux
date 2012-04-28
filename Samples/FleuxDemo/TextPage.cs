namespace FleuxDemo
{
    using System;
    using Fleux.Templates;
    using Fleux.UIElements;
    using Fleux.Core.GraphicsHelpers;
    using Fleux.Animations;
    using Fleux.Styles;
    using System.Drawing;

    public class TextPage : WindowsPhone7Page
    {
        public TextPage()
            : base("FLEUX DEMO", "text sample")
        {
            this.LeftMenu.DisplayText = "Back";
            this.LeftMenu.OnClickAction = this.Close;

            var sv = new ScrollViewer
            {
                Size = new Size(this.Content.Size.Width - 19, this.Content.Size.Height),
                Location = new Point(21, 0),
                ShowScrollbars = true,
                VerticalScroll = true
            };

            sv.Content = new DelegateUIElement
            {
                Size = new Size(sv.Size.Width, 950),
                DrawingAction = this.DrawTextSamples
            };

            this.Content.AddElement(sv.AnimateHorizontalEntrance(true));

            this.Control.ShadowedAnimationMode = Fleux.Controls.FleuxControl.ShadowedAnimationOptions.FromRight;
        }

        private void DrawTextSamples(IDrawingGraphics gr)
        {
            var text = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            var textWidth = this.Content.Size.Width - 24;

            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawText(text).MoveTo(0, gr.Bottom + 10);
            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawMultiLineText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawMultiLineText(text, textWidth).MoveTo(0, gr.Bottom + 10);

            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawText(text).MoveTo(0, gr.Bottom + 10);
            gr.Style(MetroTheme.PhoneTextTitle3Style).DrawText("DrawMultiLineText()").MoveTo(0, gr.Bottom);
            gr.Style(MetroTheme.PhoneTextSmallStyle).DrawMultiLineText(text, textWidth).MoveTo(0, gr.Bottom + 10);
        }
    }
}
