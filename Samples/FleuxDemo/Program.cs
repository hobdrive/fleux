namespace FleuxDemo
{
    using System;
    using Fleux.Core;

    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        public static void Main()
        {
            FleuxSettings.InertiaMode = FleuxSettings.InertiaModeOptions.Physics2x;
            FleuxSettings.HapticFeedbackMode = FleuxSettings.HapticOptions.Tap;
            FleuxApplication.TargetDesignDpi = 192; // Default HTC HD2 Res!
            FleuxApplication.Run(new SplashScreen());
        }
    }
}