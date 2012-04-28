using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Fleux.Core;

namespace WindowsPhone7Sample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [MTAThread]
        static void Main()
        {
            FleuxApplication.TargetDesignDpi = 192; // Default HTC HD2 Res!

            FleuxApplication.Run(new SplashScreen());
        }
    }
}