namespace Fleux.Core.Scaling
{
    public class ScaledInt
    {
        private int scaled;

        public int Scaled
        {
            get { return this.scaled; }
            set { this.scaled = value; }
        }

        public int Logical
        {
            get { return FleuxApplication.ScaleToLogic(this.scaled); }
            set { this.scaled = FleuxApplication.ScaleFromLogic(value); }
        }

        public static ScaledInt operator +(ScaledInt left, ScaledInt right)
        {
            return new ScaledInt { Scaled = left.Scaled + right.Scaled };
        }

        public static ScaledInt operator -(ScaledInt left, ScaledInt right)
        {
            return new ScaledInt { Scaled = left.Scaled - right.Scaled };
        }

        public static ScaledInt operator *(ScaledInt left, ScaledInt right)
        {
            return new ScaledInt { Scaled = left.Scaled * right.Scaled };
        }

        public static ScaledInt operator /(ScaledInt left, ScaledInt right)
        {
            return new ScaledInt { Scaled = left.Scaled / right.Scaled };
        }
    }
}
