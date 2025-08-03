using System.Drawing;

namespace System.Windows.Forms;

public class PaintEventArgs
{
    public PaintEventArgs(Graphics g, Rectangle r)
    {
        Graphics = g;
        ClipRectangle = r;
    }

    public Graphics Graphics { get; set; }
    public Rectangle ClipRectangle { get; set; }
}