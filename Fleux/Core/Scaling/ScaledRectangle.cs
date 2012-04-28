namespace Fleux.Core.Scaling
{
    using System.Drawing;

    public class ScaledRectangle
    {
        public ScaledRectangle()
        {
            this.Location = new ScaledPoint();
            this.Height = new ScaledInt();
            this.Width = new ScaledInt();
        }

        public ScaledPoint Location { get; set; }
        
        public ScaledInt Width { get; set; }
        
        public ScaledInt Height { get; set; }

        public Rectangle Scaled
        {
            get
            {
                return new Rectangle(this.Location.Scaled.X, this.Location.Scaled.Y, this.Width.Scaled, this.Height.Scaled);
            }

            set
            {
                this.Location.Scaled = value.Location;
                this.Width.Scaled = value.Width;
                this.Height.Scaled = value.Height;
            }
        }

        public Rectangle Logical
        {
            get
            {
                return new Rectangle(this.Location.Logical.X, this.Location.Logical.Y, this.Width.Logical, this.Height.Logical);
            }

            set
            {
                this.Location.Logical = value.Location;
                this.Width.Logical = value.Width;
                this.Height.Logical = value.Height;
            }
        }

        public ScaledInt X
        {
            get { return this.Location.X; }
            set { this.Location.X = value; }
        }

        public ScaledInt Y
        {
            get { return this.Location.Y; }
            set { this.Location.Y = value; }
        }

        public ScaledInt Bottom
        {
            get { return this.Location.Y + this.Height; }
        }

        public ScaledInt Right
        {
            get { return this.Location.X + this.Width; }
        }
    }
}
