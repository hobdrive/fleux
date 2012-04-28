namespace Fleux.Templates
{
    using System;
    using System.Drawing;
    using Core;
    using Styles;
    using UIElements;

    public class TitledGroup : UIElement
    {
        private string title;
        private int textHeight;
        private UIElement content;

        public TitledGroup()
        {
            this.SizeChanged += (s, e) => 
                                    { 
                                        if (e.Old.Width != e.New.Width) 
                                        { 
                                            this.Relayout(); 
                                        } 
                                    };
            this.TitleStyle = MetroTheme.PhoneTextSmallStyle;
        }

        public string Title
        {
            get
            {
                return this.title;
            }

            set
            {
                this.title = value;
                this.Relayout();
            }
        }

        public UIElement Content
        {
            get
            {
                return this.content;
            }

            set
            {
                if (this.content != value)
                {
                    this.content = value;
                    this.Relayout();
                }
            }
        }

        public TextStyle TitleStyle { get; set; }

        public override void ResizeForWidth(int width)
        {
            this.Size = new Size(width, this.Size.Height);
        }

        public override void Draw(Fleux.Core.GraphicsHelpers.IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.Style(this.TitleStyle)
                .DrawMultiLineText(this.title, this.Size.Width, this.textHeight);
            if (this.Content != null)
            {
                this.Content.Draw(drawingGraphics.CreateChild(this.Content.Location));
            }
        }

        private void Relayout()
        {
            if (this.Size != null && this.Size.Width > 0 && this.title != null)
            {
                this.textHeight = FleuxApplication.DummyDrawingGraphics
                    .Style(this.TitleStyle)
                    .CalculateMultilineTextHeight(this.title, this.Size.Width);
                var newHeight = this.textHeight + 10;
                if (this.Content != null)
                {
                    this.Content.ResizeForWidth(this.Size.Width);
                    this.Content.Location = new Point(0, this.textHeight);
                    newHeight += this.Content.Size.Height;
                }
                this.Size = new Size(this.Size.Width, newHeight);
            }
        }
    }
}
