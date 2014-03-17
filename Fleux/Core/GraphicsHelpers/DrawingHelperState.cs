namespace Fleux.Core.GraphicsHelpers
{
    using System;
    using System.Drawing;

    public class DrawingHelperState
    {
        public int FontSize = 10;
        public FontStyle FontStyle = FontStyle.Regular;

        private int currentX;
        private int currentY;
        private string fontName = "Tahoma";

        public DrawingHelperState(Action<int, int> calculateExtendsAction)
        {
            this.FontSize = 10;
            this.FontStyle = System.Drawing.FontStyle.Regular;
            this.CalculateExtendsAction = calculateExtendsAction;
            this.Color = Color.White;
            this.PenWidth = 1;
        }

        public int CurrentX
        {
            get
            {
                return this.currentX;
            }

            set
            {
                this.currentX = value;
                this.RaiseCalculateExtends();
            }
        }

        public int CurrentY
        {
            get
            {
                return this.currentY;
            }

            set
            {
                this.currentY = value;
                this.RaiseCalculateExtends();
            }
        }

        public Action<int, int> CalculateExtendsAction { get; private set; }

        public Font CurrenFont
        {
            get { return ResourceManager.Instance.GetFont(this.FontName, this.FontStyle, this.FontSize); }
        }

        public bool FontUpdated { get; set; }

        public string FontName
        {
            get
            {
                return this.fontName;
            }

            set
            {
                this.fontName = value;
                this.FontUpdated = false;
            }
        }

        public Color Color { get; set; }

        public int PenWidth { get; set; } // PenWidth in pixels (already scaled)

        public Brush CurrentBrush
        {
            get { return ResourceManager.Instance.GetBrush(this.Color); }
        }

        public Pen CurrentPen
        {
            get { return ResourceManager.Instance.GetPen(this.Color, this.PenWidth); }
        }

        public void SetFontStyle(FontStyle flag, bool enable)
        {
            // TODO: Review this code b/c we're currently hardcoding BOLD
            if (enable)
            {
                this.FontStyle = System.Drawing.FontStyle.Bold;
            }
            else
            {
                this.FontStyle = System.Drawing.FontStyle.Regular;
            }
            this.FontUpdated = false;    
        
            ////if (enable)
            ////{
            ////    this.FontStyle |= flag;
            ////}
            ////else
            ////{
            ////    this.FontStyle ^= flag;
            ////}
            ////this.FontUpdated = false;
        }

        private void RaiseCalculateExtends()
        {
            this.CalculateExtendsAction(this.currentX, this.currentY);
        }
    }
}
