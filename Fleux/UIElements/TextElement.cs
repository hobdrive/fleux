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
            /// Fixed size
            None, 
            /// Both height and width are calculated
            OneLineAutoHeight,
            /// auto height, fixed width
            OneLineAutoHeightFixedWidth,
            /// Wrap text over fixed width
            WrapText,
        }
  
        AutoSizeModeOptions _AutoSizeMode;
        public AutoSizeModeOptions AutoSizeMode {
            get{
                return _AutoSizeMode;
            }
            set{
                _AutoSizeMode = value;
                Relayout(FleuxApplication.DummyDrawingGraphics);
            }
        }

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
            if (this.AutoSizeMode == AutoSizeModeOptions.WrapText ||
                this.AutoSizeMode == AutoSizeModeOptions.OneLineAutoHeight)
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
                case AutoSizeModeOptions.OneLineAutoHeightFixedWidth:
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
                int height = 0;
                switch (this.AutoSizeMode)
                {
                    case AutoSizeModeOptions.OneLineAutoHeight:
                        var sz = dg.Style(this.Style).CalculateTextSize(this.text ?? "");
                        this.Size = new Size(sz.Width, sz.Height);
                        break;
                    case AutoSizeModeOptions.OneLineAutoHeightFixedWidth:
                        height = dg.Style(this.Style).CalculateTextSize(this.text ?? "").Height;
                        this.Size = new Size(this.Size.Width, height);
                        break;
                    case AutoSizeModeOptions.WrapText:
                        height = dg.Style(this.Style)
                            .CalculateMultilineTextHeight(this.text ?? "", this.Size.Width);
                        this.Size = new Size(this.Size.Width, height);
                        break;
                }
            }
        }
    }
}
