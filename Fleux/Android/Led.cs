using System;
using System.Runtime.InteropServices;
using System.Threading;
using Android.OS;
using Android.Content;

namespace Fleux.Core.NativeHelpers
{

    public class Led
    {

        static Vibrator vibrator;

        public Led()
        {
        }

        public static void Init(Context root)
        {
            vibrator = (Vibrator)root.GetSystemService(Context.VibratorService);
        }

        public void Vibrate(int millisecondsTimeout)
        {
            try{
                vibrator.Vibrate(millisecondsTimeout);
            }catch(Exception){}
        }

        public void Vibrate()
        {
            this.Vibrate(FleuxSettings.HapticFeedbackMillisecs);
        }

    }
}
