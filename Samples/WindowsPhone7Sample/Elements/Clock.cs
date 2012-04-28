using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements;
using System.Threading;
using Fleux.Styles;

namespace WindowsPhone7Sample.Elements
{
    public class Clock : TextElement
    {
        private readonly Timer clockTimer;

        public Clock()
            : base("")
        {
            this.clockTimer = new Timer(new TimerCallback(o => { this.text = DateTime.Now.ToString("hh:mm"); this.Update();})
                ,null, TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(1));

            this.Style = MetroTheme.TileTextStyle;
            this.Style.Foreground = MetroTheme.PhoneForegroundBrush;
        }
    }
}
