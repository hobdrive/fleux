namespace Fleux.Core.Dim
{
    using System;

    public static class IntExtensions
    {
        public static int Distance(this int p1, int p2)
        {
            return Math.Abs(p1 - p2);
        }

        public static bool IsCloseTo(this int p1, int p2, int tollerance)
        {
            return p1.Distance(p2) <= tollerance;
        }
    }
}
