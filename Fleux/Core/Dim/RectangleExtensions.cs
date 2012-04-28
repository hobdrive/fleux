namespace Fleux.Core.Dim
{
    using System;
    using System.Drawing;

    public static class RectangleExtensions
    {
        public static Rectangle ClientTo(this Rectangle source, int x, int y)
        {
            return new Rectangle(source.X - x, source.Y - y, source.Width, source.Height);
        }

        public static Rectangle ClientTo(this Rectangle source, Point p)
        {
            return source.ClientTo(p.X, p.Y);
        }

        public static Rectangle ClientTo(this Rectangle source, Rectangle r)
        {
            return source.ClientTo(r.X, r.Y);
        }
    }
}
