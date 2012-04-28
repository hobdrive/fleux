namespace Fleux.Core.Scaling
{
    using System;
    using System.Drawing;

    public static class PointExtensions
    {
        public static Point ToLogic(this Point scaled)
        {
            return new Point(scaled.X.ToLogic(), scaled.Y.ToLogic());
        }

        public static Point ToPixels(this Point logic)
        {
            return new Point(logic.X.ToPixels(), logic.Y.ToPixels());
        }
    }
}
