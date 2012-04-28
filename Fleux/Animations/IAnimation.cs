namespace Fleux.Animations
{
    public interface IAnimation
    {
        int Duration { get; set; }

        void Reset();

        bool Animate();

        void Cancel();
    }
}
