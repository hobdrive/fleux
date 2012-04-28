namespace Fleux.Core
{
    using System;

    public static class FleuxSettings
    {
        /// <summary>
        /// Sets or gets how long the Haptic feedback should keep vibrating
        /// </summary>
        public static int HapticFeedbackMillisecs = 30;
     
        public enum InertiaModeOptions
        {
            RealisticPhysics,
            Physics2x,
            Physics3x,

            // We would add some new
            // modes like an optimized one
            // or an adaptable one
        }

        public enum HapticOptions
        {
            Disabled,               // Do not vibrate on touch events
            AnyPress,               // Any touch event in a FleuxControl should vibrate
            FeedbackEnabledPress,   // Only vibrate in pressed events on elements marked for providing feedback
            Tap,                    // Only vibrate in tap events on elements marked for providing feedback
        }

        /// <summary>
        /// Sets or gets how the kinetic scrolling implementation of Inertia should behave in all
        /// the Fleux elements
        /// </summary>
        public static InertiaModeOptions InertiaMode { get; set; }

        /// <summary>
        /// Sets or gets how fleux elements should provide haptic feedback or not using the
        /// device vibration.
        /// </summary>
        public static HapticOptions HapticFeedbackMode { get; set; }
        
        /// <summary>
        /// Enables or disables animations globally
        /// </summary>
        public static bool AnimationMode { get { return animationMode; } set { animationMode = value;} }
        static bool animationMode = true;
    }
}
