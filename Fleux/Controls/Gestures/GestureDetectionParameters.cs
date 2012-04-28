namespace Fleux.Controls.Gestures
{
    public class GestureDetectionParameters
    {
        // Before this, it can be a tap, otherwise it can be a pan or hold
        public int TapTimePeriod;

        // Up to this distance, it can be a tap or hold, otherwise, it can be a flick or pan
        public int TapDistance;

        // Up to this threshold panning will not be detected
        public int PanThreshold;

        // How long to wait for the second tap
        public int DoubleTapTimePeriod;

        // Before this, it can be a flick, otherwise it's a pan
        public int FlickPeriod;

        private static GestureDetectionParameters current;

        public static GestureDetectionParameters Current
        {
            get
            {
                return current ?? (current = new GestureDetectionParameters
                                                 {
                                                     TapTimePeriod = 500,
                                                     TapDistance = 18,
                                                     PanThreshold = 5,
                                                     DoubleTapTimePeriod = 700,
                                                     FlickPeriod = 500,
                                                 });
            }
        }
    }
}
