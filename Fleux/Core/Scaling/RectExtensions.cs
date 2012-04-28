namespace Fleux.Core.Scaling
{
    using System;
    using System.Drawing;

    public static class RecttExtensions
    {
        public static Rectangle ToLogic(this Rectangle scaled)
        {
            return new Rectangle(scaled.X.ToLogic(), scaled.Y.ToLogic(), scaled.Width.ToLogic(), scaled.Height.ToLogic());
        }

        public static Rectangle ToPixels(this Rectangle logic)
        {
            return new Rectangle(logic.X.ToPixels(), logic.Y.ToPixels(), logic.Width.ToPixels(), logic.Height.ToPixels());
        }
    }
}
