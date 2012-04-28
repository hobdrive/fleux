namespace Fleux.Controls.Scrolling
{
    using System;
    using System.Drawing;

    public interface IGestureScrollingBehavior : IDisposable
    {
        int Current { get; set; }

        int Max { get; set; }

        int Min { get; set; }

        void Pan(int from, int to, bool done);

        void Flick(int from, int to, int millisecs);

        void Pressed();
    }
}
