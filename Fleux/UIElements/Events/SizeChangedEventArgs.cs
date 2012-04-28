namespace Fleux.UIElements.Events
{
    using System;
    using System.Drawing;

    public class SizeChangedEventArgs : EventArgs
    {
        public Size Old { get; set; }

        public Size New { get; set; }
    }
}
