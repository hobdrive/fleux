using System;
using System.Windows.Forms;
using System.Drawing;

namespace Fleux.Controls
{
    public class FleuxControlPage : IDisposable
    {
        public Form TheForm;
        public FleuxControl Control;
        public Size Size;

        public virtual void Dispose()
        {
            Control.Dispose();
        }
        public virtual void Close(){
            Control.AnimateExit();
        }
    }
}