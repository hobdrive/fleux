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
        private bool _inRelayout;

        public TextElement(string text)
        {
            this.text = text;
            this.Style = MetroTheme.PhoneTextNormalStyle;
            SizeChanged += delegate
                {
                    if (_inRelayout) return; // to avoid creating FleuxApplication.DummyDrawingGraphics
                    DoRelayout(FleuxApplication.DummyDrawingGraphics);
                };
        }

        public enum AutoSizeModeOptions
        {
            /// <summary>
            /// Fixed size
            /// </summary>
            None, 
            /// <summary>
            /// Both height and width are calculated
            /// </summary>
            OneLineAutoHeight,
            /// <summary>
            /// Auto height, fixed width
            /// </summary>
            OneLineAutoHeightFixedWidth,
            /// <summary>
            /// Wrap text over fixed width
            /// </summary>
            WrapText,
        }
  
        AutoSizeModeOptions _AutoSizeMode;
        public AutoSizeModeOptions AutoSizeMode {
            get{
                return _AutoSizeMode;
            }
            set{
                _AutoSizeMode = value;
                DoRelayout(FleuxApplication.DummyDrawingGraphics);
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
                this.AutoSizeMode == AutoSizeModeOptions.OneLineAutoHeight ||
                this.AutoSizeMode == AutoSizeModeOptions.OneLineAutoHeightFixedWidth)
            {
                this.Size = new Size(width, 10); // Height will be calculated later
                try
                {
                    DoRelayout(FleuxApplication.DummyDrawingGraphics);
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
                DoRelayout(drawingGraphics);
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

        private void DoRelayout(IDrawingGraphics drawingGraphics)
        {
            if (_inRelayout) return;

            _inRelayout = true;
            try
            {
                Relayout(drawingGraphics);
            }
            finally
            {
                _inRelayout = false;
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
