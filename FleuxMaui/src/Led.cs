using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Fleux.Core.NativeHelpers
{

    public class Led
    {

        public Led()
        {
        }


        public void Vibrate(int millisecondsTimeout)
        {
            try{
                //vibrator.Vibrate(millisecondsTimeout);
            }catch(Exception){}
        }

        public void Vibrate()
        {
            this.Vibrate(FleuxSettings.HapticFeedbackMillisecs);
        }

    }
}
