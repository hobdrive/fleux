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

        public static Size MeasureString(Graphics gr, Font font, string text, int width)
        {
            return gr.MeasureStringWidth(text, font, width);
        }
    }
    public class FleuxPage
    {
        public object TheForm;
    }
}
