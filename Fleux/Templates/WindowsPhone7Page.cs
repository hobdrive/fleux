namespace Fleux.Templates
{
    using System;
    using System.Drawing;
    using Animations;
    using Controls;
    using Styles;
    using UIElements;

    public class WindowsPhone7Page : FleuxControlPage
    {
        protected TextElement title1;
        protected TextElement title2;

        public WindowsPhone7Page(string title1, string title2)
        {
            this.Control.EntranceDuration = 300;

            this.title1 = new TextElement(title1)
            {
                Style = MetroTheme.PhoneTextPageTitle1Style,
                Location = new Point(24 - 3, 9)  // -3 is a correction for Segoe fonts
            };
            this.title2 = new TextElement(title2)
            {
                Style = MetroTheme.PhoneTextPageTitle2Style,
                Location = new Point(24 - 3, 30) // -3 is a correction for Segoe fonts
            };

            this.Content = new Canvas
            {
                Size = new Size(this.Size.Width, this.Size.Height - 150),
                Location = new Point(0, 150)
            };

            this.Background = new Canvas { Size = this.Size };

            this.Control.AddElement(this.Background);
            this.Control.AddElement(this.Content);
            this.Control.AddElement(this.title1.AnimateVerticalEntrance(true));
            this.Control.AddElement(this.title2.AnimateVerticalEntrance(true));
        }

        public Canvas Content { get; set; }

        public Canvas Background { get; set; }
    }
}
