namespace Fleux.Core.NativeHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public static class StringHelpers
    {
        private const int DTCALCRECT = 0x00000400;
        private const int DTWORDBREAK = 0x00000010;
        private const int DTEDITCONTROL = 0x00002000;

#if WINCE
        /// <summary>
        /// Measure a multiline string
        /// </summary>
        public static Size MeasureString(Graphics gr, Font font, string text, int width)
        {
            try{
                Rect bounds = new Rect() { Left = 0, Right = width, Bottom = 1, Top = 0 };
                IntPtr hDc = gr.GetHdc();
                int flags = DTCALCRECT | DTWORDBREAK;
                IntPtr controlFont = font.ToHfont();
                IntPtr originalObject = SelectObject(hDc, controlFont);
                DrawText(hDc, text, text.Length, ref bounds, flags);
                SelectObject(hDc, originalObject); // Release resources
                gr.ReleaseHdc(hDc);
    
                return new Size(bounds.Right - bounds.Left, bounds.Bottom - bounds.Top);
            }catch(Exception){
                return Size.Empty;
            }
        }
        [DllImport("coredll.dll")]
        private static extern int DrawText(IntPtr hdc, string lpstr, int ncount, ref Rect lprect, int wformat);

        [DllImport("coredll.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("coredll.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hobject);
#else
        public static Size MeasureString(Graphics gr, Font font, string text, int width)
        {
            // TODO
            return TextRenderer.MeasureText(gr, text, font, new Size(width, 1), TextFormatFlags.WordBreak);
            //return new Size(width, 40);
        }
#endif
        internal struct Rect
        {
            public int Left, Top, Right, Bottom;

            public Rect(Rectangle r)
            {
                this.Left = r.Left;
                this.Top = r.Top;
                this.Bottom = r.Bottom;
                this.Right = r.Right;
            }
        }
    }
}
