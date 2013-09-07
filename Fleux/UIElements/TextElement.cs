
using Fleux.Core.Scaling;
using System;
using System.Drawing;

namespace Fleux.UIElements
{
    using Core;
    using Core.GraphicsHelpers;
    using Styles;

    public class TextElement : UIElement
    {
        protected string text;
        protected bool needUpdate = true;
        public Size TextSize;
        
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
            var ctext = text;
            
            if (this.needUpdate)
            {
                DoRelayout(drawingGraphics);
                this.needUpdate = false;
            }

            drawingGraphics.Style(this.Style);

            /* WTF????
            if (this.AutoSizeMode == AutoSizeModeOptions.None ||
                this.AutoSizeMode == AutoSizeModeOptions.OneLineAutoHeightFixedWidth)
            {
                while (TextSize.Width.ToPixels() > this.Width)
                {
                    ctext = ctext.Substring(0, ctext.Length-3)+"..";
                    Relayout(drawingGraphics, ctext);
                }
            }    
            */
            
            switch (this.AutoSizeMode)
            {
                case AutoSizeModeOptions.None:
                case AutoSizeModeOptions.OneLineAutoHeightFixedWidth:
                case AutoSizeModeOptions.OneLineAutoHeight:
                    drawingGraphics.DrawText(ctext);
                    break;
                case AutoSizeModeOptions.WrapText:
                    drawingGraphics.DrawMultiLineText(ctext, this.Size.Width, this.Size.Height);
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
            Relayout(dg, this.text);
        }

        protected virtual void Relayout(IDrawingGraphics dg, string text)
        {
            TextSize = dg.Style(this.Style).CalculateTextSize(text);
            if (this.AutoSizeMode != AutoSizeModeOptions.None)
            {
                int height = 0;
                switch (this.AutoSizeMode)
                {
                    case AutoSizeModeOptions.OneLineAutoHeight:
                        this.Size = new Size(TextSize.Width, TextSize.Height);
                        break;
                    case AutoSizeModeOptions.OneLineAutoHeightFixedWidth:
                        height = TextSize.Height;
                        this.Size = new Size(this.Size.Width, height);
                        break;
                    case AutoSizeModeOptions.WrapText:
                        height = dg.Style(this.Style)
                            .CalculateMultilineTextHeight(text, this.Size.Width);
                        this.Size = new Size(this.Size.Width, height);
                        break;
                }
            }
        }
    }
}
