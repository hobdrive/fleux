namespace FleuxDemo
{
    using System.Drawing;
    using System.Windows.Forms;
    using Fleux.Animations;
    using Fleux.Controls;
    using Fleux.Controls.Gestures;
    using Fleux.Templates;

    public class GesturesTestPage : WindowsPhone7Page
    {
        public GesturesTestPage()
            : base("FLEUX SAMPLE", "gestures")
        {
            LeftMenu.DisplayText = "Back";
            LeftMenu.OnClickAction = () => this.Close();

            this.Content.AddElement(new GesturesTestElement { Size = this.Content.Size });

            this.Control.ShadowedAnimationMode = FleuxControl.ShadowedAnimationOptions.FromRight;
        }
    }
}
