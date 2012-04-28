namespace Fleux.Controls.Gestures
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    public static class GestureExtensions
    {
        public static Point Point(this MouseEventArgs e)
        {
            return new Point(e.X, e.Y);
        }
    }
}
