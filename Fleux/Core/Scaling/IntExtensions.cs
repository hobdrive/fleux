namespace Fleux.Core.Scaling
{
    public static class IntExtensions
    {
        public static int ToLogic(this int scaled)
        {
            return FleuxApplication.ScaleToLogic(scaled);
        }

        public static int ToPixels(this int logic)
        {
            return FleuxApplication.ScaleFromLogic(logic);
        }

        public static double ToLogic(this double scaled)
        {
            return FleuxApplication.ScaleToLogic(scaled);
        }

        public static int ToPixels(this double logic)
        {
            return (int)FleuxApplication.ScaleFromLogic(logic);
        }
    }
}
