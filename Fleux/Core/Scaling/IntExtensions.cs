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
    }
}
