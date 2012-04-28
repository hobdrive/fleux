namespace Fleux.Core.Dim
{
    public class Length
    {
        private readonly int value;

        public Length(int length)
        {
            this.value = length;
        }

        public int Pixels
        {
            get { return this.value; }
        }
    }
}
