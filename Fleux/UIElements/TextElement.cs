namespace Fleux.UIElements
{
    using System;
    using System.Drawing;
    using Core;
    using Core.GraphicsHelpers;
    using Styles;

    public class TextElement : UIElement
    {
        protected string text;
        protected bool needUpdate = true;

        public TextElement(string text)
        {
            this.text = text;
            this.Style = MetroTheme.PhoneTextNormalStyle;
        }

        public enum AutoSizeModeOptions
        {
            None,
            OneLineAutoHeight,
            WrapText,
        }

        public AutoSizeModeOptions AutoSizeMode { get; set; }

        public string Text
        {
            get
            {
                return this.text;
            }

            set
            {
                this.text = value;
                this.needUpdate = true;
            }
        }

        public TextStyle Style { get; set; }

        public override void ResizeForWidth(int width)
        {
            if (this.AutoSizeMode != AutoSizeModeOptions.None)
            {
                this.Size = new Size(width, 10); // Height will be calculated later
                try
                {
                    this.Relayout(FleuxApplication.DummyDrawingGraphics);
                }
                catch (Exception)
                {
                }
                this.needUpdate = true;
            }
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            if (this.needUpdate)
            {
                this.Relayout(drawingGraphics);
                this.needUpdate = false;
            }

            drawingGraphics.Style(this.Style);

            switch (this.AutoSizeMode)
            {
                case AutoSizeModeOptions.None:
                case AutoSizeModeOptions.OneLineAutoHeight:
                    drawingGraphics.DrawText(this.text);
                    break;
                case AutoSizeModeOptions.WrapText:
                    drawingGraphics.DrawMultiLineText(this.text, this.Size.Width, this.Size.Height);
                    break;
            }
        }

        protected virtual void Relayout(IDrawingGraphics dg)
        {
            if (this.AutoSizeMode != AutoSizeModeOptions.None)
            {
                var height = 0;
                switch (this.AutoSizeMode)
                {
                    case AutoSizeModeOptions.OneLineAutoHeight:
                        height = dg.Style(this.Style)
                            .CalculateTextSize(this.text ?? "").Height;
                        break;
                    case AutoSizeModeOptions.WrapText:
                        height = dg.Style(this.Style)
                            .CalculateMultilineTextHeight(this.text ?? "", this.Size.Width);
                        break;
                }
                this.Size = new Size(this.Size.Width, height);
            }
        }
    }
}
