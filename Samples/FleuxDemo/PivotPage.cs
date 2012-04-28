namespace FleuxDemo
{
    using System;
    using Fleux.Animations;
    using Fleux.Controls;
    using Fleux.Core;
    using Fleux.UIElements;
    using Fleux.UIElements.Pivot;
    using System.Drawing;

    public class PivotPage : FleuxControlPage
    {
        public PivotPage()
        {
            LeftMenu.DisplayText = "Back";
            LeftMenu.OnClickAction = () => this.Close();

            this.Control.ShadowedAnimationMode = Fleux.Controls.FleuxControl.ShadowedAnimationOptions.FromRight;

            var pivot = new Pivot("SETTINGS") { Size = this.Control.Size };

            pivot.AddPivotItem(new SystemPivotItem());
            pivot.AddPivotItem(new FleuxSettingsPivotItem());

            pivot.AddPivotItem(new TextPivotItem(this.Size.Width - 21));

            this.Control.AddElement(pivot.AnimateHorizontalEntrance(true));
            this.Control.EntranceDuration = 300;
        }
    }
}
