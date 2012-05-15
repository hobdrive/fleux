using System;
using System.Drawing;
using System.Windows.Forms;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Fleux.Core;

namespace Fleux.Controls
{

    public class FleuxControlPage : IDisposable
    {
        public FleuxControl Control = new Fleux.Controls.FleuxControl();

        public virtual void Dispose(){
            Control.Dispose();
        }
        public virtual void Close(){
            Control.AnimateExit();
        }
    }
}