namespace FleuxDemo
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Fleux.Core.GraphicsHelpers;
    using Fleux.Styles;
    using Fleux.UIElements;

    public class GesturesTestElement : UIElement
    {
        private Action<IDrawingGraphics> drawAction = gr => { };

        public GesturesTestElement()
        {
            this.Draw(g => { });

            this.TapHandler = p => this.Draw(g =>
            {
                g.DrawText(string.Format("Tap {0},{1}", p.X, p.Y));
                g.DrawEllipse(new Rectangle(p.X - 20, p.Y - 20, 40, 40));
            });
            this.DoubleTapHandler = p => this.Draw(g =>
            {
                g.DrawText(string.Format("Double Tap {0},{1}", p.X, p.Y));
                g.DrawEllipse(new Rectangle(p.X - 20, p.Y - 20, 40, 40));
                g.DrawEllipse(new Rectangle(p.X - 30, p.Y - 30, 60, 60));
            });
            this.HoldHandler = p => this.Draw(g =>
            {
                g.DrawText(string.Format("Hold {0},{1}", p.X, p.Y));
                g.FillEllipse(new Rectangle(p.X - 30, p.Y - 30, 60, 60));
            });
            this.PanHandler = (f, t, d, p) => this.Draw(g =>
            {
                g.DrawText(string.Format("Pan {0},{1} to {2},{3} [{4}]", f.X, f.Y, t.X, t.Y, d));
                g.DrawLine(f.X, f.Y, t.X, t.Y);
            });
            this.FlickHandler = (f, t, m, p) => this.Draw(g =>
            {
                g.DrawText(string.Format("Flick {0},{1} to {2},{3}", f.X, f.Y, t.X, t.Y));
                g.DrawLine(f.X, f.Y, t.X, t.Y);
            });
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            drawingGraphics.Color(Color.FromArgb(50, 50, 50)).FillRectangle(0, 0, this.Size.Width, this.Size.Height)
                .Color(Color.FromArgb(220, 220, 220))
                .MoveTo(20, this.Size.Height - 65)
                .Style(MetroTheme.PhoneTextNormalStyle);
            this.drawAction(drawingGraphics);
        }

        public bool Draw(Action<IDrawingGraphics> g)
        {
            this.drawAction = g;
            this.Update();
            return true;
        }
    }
}
