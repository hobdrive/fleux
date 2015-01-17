namespace Fleux.Core.Dim
{
    using System;
    using System.Drawing;

    public static class PointExtensions
    {
        public static int Distance(this Point p1, Point p2)
        {
            return (int)Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        public static bool IsCloseTo(this Point p1, Point p2, int tollerance)
        {
            return p1.Distance(p2) <= tollerance;
        }

        public static Point ClientTo(this Point p1, int x, int y)
        {
            return new Point(p1.X - x, p1.Y - y);
        }

        public static Point ClientTo(this Point p1, Point p2)
        {
            return p1.ClientTo(p2.X, p2.Y);
        }

        public static Point Add(this Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point ToParent(this Point p1, int x, int y)
        {
            return new Point(p1.X + x, p1.Y + y);
        }

        public static Point ToParent(this Point p1, Point p2)
        {
            return p1.ToParent(p2.X, p2.Y);
        }
    }
}
