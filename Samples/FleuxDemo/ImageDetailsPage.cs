namespace FleuxDemo
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Fleux.Animations;
    using Fleux.Core;
    using Fleux.Templates;
    using Fleux.UIElements;

    public class ImageDetailsPage : WindowsPhone7Page
    {
        public ImageDetailsPage(string title, string subtitle, Image image)
            : base(subtitle, title.ToLower())
        {
            LeftMenu.DisplayText = "Back";
            LeftMenu.OnClickAction = () => this.Close();

            this.Control.ShadowedAnimationMode = Fleux.Controls.FleuxControl.ShadowedAnimationOptions.FromRight;

            var sv = new ScrollViewer { Size = this.Background.Size, HorizontalScroll = true, VerticalScroll = true };
            
            sv.Content = new ImageElement(image)
            {
                Size = this.Background.Size
            };
            this.Background.AnimateHorizontalEntrance(true);

            this.Background.AddElement(sv);
            this.Background.AddElement(new TransparentImageElement(ResourceManager.Instance.GetIImageFromEmbeddedResource("topshadow.png"))
                                            {
                                                Size = new Size(this.Size.Width, 150)
                                            }
                                            .AnimateVerticalEntrance(true));
        }
    }
}
