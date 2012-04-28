namespace FleuxDemo
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;
    using Fleux.Animations;
    using Fleux.Controls;
    using Fleux.Core;
    using Fleux.UIElements;

    public class SplashScreen : FleuxControlPage
    {
        public SplashScreen()
            : base(true)
        {
            this.Control.AddElement(
                DefaultAnimations.SetVerticalDefaultAnimations(
                new ImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("fleuxsplashbg.png"))
                {
                    Location = new Point(0, this.Size.Height - 800)
                },
                false,
                FunctionBasedAnimation.Functions.SoftedFluid,
                FunctionBasedAnimation.Functions.CubicReverse));
            this.Control.AddElement(new TransparentImageElement(ResourceManager.Instance.GetIImageFromEmbeddedResource("fleuxsplash.png"))
                {
                    Location = new Point(0, this.Size.Height - 800)
                }
                .AnimateHorizontalEntrance(true));
            this.Control.EntranceDuration = 1200;
            var timer = new Timer { Interval = 1500, Enabled = true };
            timer.Tick += (s, e) =>
                {
                    timer.Enabled = false;
                    this.Control.EntranceDuration = 800;
                    this.NavigateTo(new PanoramaPage());
                    this.Control.EntranceDuration = 0;
                    this.theForm.Hide();
                };
            this.Control.OnExitAnimationCompleted = () => { this.TheForm.Activated += (s, e) => Application.Exit(); };
        }
    }
}
