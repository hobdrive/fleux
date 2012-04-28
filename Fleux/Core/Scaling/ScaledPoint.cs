namespace Fleux.Core.Scaling
{
    using System.Drawing;

    public class ScaledPoint
    {
        public ScaledPoint()
        {
            this.X = new ScaledInt();
            this.Y = new ScaledInt();
        }

        public ScaledInt X { get; set; }
        
        public ScaledInt Y { get; set; }

        public Point Scaled
        {
            get
            {
                return new Point(this.X.Scaled, this.Y.Scaled);
            }

            set
            {
                this.X.Scaled = value.X;
                this.Y.Scaled = value.Y;
            }
        }

        public Point Logical
        {
            get
            {
                return new Point(this.X.Logical, this.Y.Logical);
            }

            set
            {
                this.X.Logical = value.X;
                this.Y.Logical = value.Y;
            }
        }
    }
}
