namespace FleuxDemo
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using Fleux.Animations;
    using Fleux.Controls;
    using Fleux.Core;
    using Fleux.Styles;
    using Fleux.Templates;
    using Fleux.UIElements;

    public class ListPage : WindowsPhone7Page
    {
        public ListPage()
            : base("FLEUX SAMPLE", "list page")
        {
            LeftMenu.DisplayText = "Back";
            LeftMenu.OnClickAction = () => this.Close();

            this.Control.EntranceDuration = 350;
            this.Content.AddElement(
                DefaultAnimations.SetHorizontalDefaultAnimations(
                new ListElement
                    {
                        DataTemplateSelector = item => BuildItem,
                        SourceItems = GetSampleData(),
                        Size = this.Content.Size,
                        ShowScrollbars = true,
                    },
                    true));

            this.Control.ShadowedAnimationMode = FleuxControl.ShadowedAnimationOptions.FromRight;
        }

        internal static BindingList<object> GetSampleData()
        {
            return new BindingList<object>
                             {
                                 "1.First Item",
                                 "2.Second Item",
                                 "3.Third Item",
                                 "4.Fourth Item",
                                 "5.Fifth Item",
                                 CreateSampleImageItem(),
                                 "6.Sixth Item",
                                 "7.Seventh Item",
                                 "8.Eigth Item",
                                 "9.Nineth Item",
                                 "10.Tenth Item",
                                 CreateSampleImageItem(),
                                 "11.First Item",
                                 CreateSampleImageItem(),
                                 "12.Second Item",
                                 "13.Third Item",
                                 "14.Fourth Item",
                                 "15.Fifth Item",
                                 "16.Sixth Item",
                                 "17.Seventh Item",
                                 CreateSampleImageItem(),
                                 "18.Eigth Item",
                                 "19.Nineth Item",
                                 CreateSampleImageItem(),
                                 "20.Tenth Item",
                                 "21.First Item",
                                 "22.Second Item",
                                 "23.Third Item",
                                 CreateSampleImageItem(),
                                 "24.Fourth Item",
                                 "25.Fifth Item",
                                 CreateSampleImageItem(),
                                 "26.Sixth Item",
                                 CreateSampleImageItem(),
                                 "27.Seventh Item",
                                 CreateSampleImageItem(),
                                 "28.Eigth Item",
                                 "29.Nineth Item",
                                 "30.Tenth Item",
                                 "31.First Item",
                                 "32.Second Item",
                                 "33.Third Item",
                                 "34.Fourth Item",
                                 "35.Fifth Item",
                                 CreateSampleImageItem(),
                                 "36.Sixth Item",
                                 CreateSampleImageItem(),
                                 "37.Seventh Item",
                                 "38.Eigth Item",
                                 "39.Nineth Item",
                                 "40.Tenth Item",
                                 "41.First Item",
                                 CreateSampleImageItem(),
                                 "42.Second Item",
                                 CreateSampleImageItem(),
                                 "43.Third Item",
                                 "44.Fourth Item",
                                 "45.Fifth Item",
                                 "46.Sixth Item",
                                 CreateSampleImageItem(),
                                 "47.Seventh Item",
                                 "48.Eigth Item",
                                 "49.Nineth Item",
                                 CreateSampleImageItem(),
                                 "50.Tenth Item",
                                 "51.First Item",
                                 "52.Second Item",
                                 "53.Third Item",
                                 CreateSampleImageItem(),
                                 "54.Fourth Item",
                                 CreateSampleImageItem(),
                                 "55.Fifth Item",
                                 "56.Sixth Item",
                                 "57.Seventh Item",
                                 "58.Eigth Item",
                                 "59.Nineth Item",
                                 CreateSampleImageItem(),
                                 "60.Tenth Item",
                                 "61.First Item",
                                 CreateSampleImageItem(),
                                 "62.Second Item",
                                 "63.Third Item",
                                 CreateSampleImageItem(),
                                 "64.Fourth Item",
                                 "65.Fifth Item",
                                 CreateSampleImageItem(),
                                 "66.Sixth Item",
                                 "67.Seventh Item",
                                 CreateSampleImageItem(),
                                 "68.Eigth Item",
                                 "69.Nineth Item",
                                 "70.Tenth Item",
                                 "71.First Item",
                                 "72.Second Item",
                                 "73.Third Item",
                                 CreateSampleImageItem(),
                                 "74.Fourth Item",
                                 "75.Fifth Item",
                                 "76.Sixth Item",
                                 CreateSampleImageItem(),
                                 "77.Seventh Item",
                                 "78.Eigth Item",
                                 "79.Nineth Item",
                                 CreateSampleImageItem(),
                                 "80.Tenth Item",
                                 "81.First Item",
                                 "82.Second Item",
                                 CreateSampleImageItem(),
                                 "83.Third Item",
                                 "84.Fourth Item",
                                 "85.Fifth Item",
                                 CreateSampleImageItem(),
                                 "86.Sixth Item",
                                 "87.Seventh Item",
                                 CreateSampleImageItem(),
                                 "88.Eigth Item",
                                 "89.Nineth Item",
                                 "90.Tenth Item",
                                 CreateSampleImageItem(),
                                 "91.First Item",
                                 "92.Second Item",
                                 "93.Third Item",
                                 CreateSampleImageItem(),
                                 "94.Fourth Item",
                                 "95.Fifth Item",
                                 CreateSampleImageItem(),
                                 "96.Sixth Item",
                                 "97.Seventh Item",
                                 CreateSampleImageItem(),
                                 "98.Eigth Item",
                                 CreateSampleImageItem(),
                                 "99.Nineth Item",
                                 "100.Tenth Item",
                             };
        }

        internal static UIElement BuildItem(object textSource)
        {
            if (textSource is UIElement)
            {
                return textSource as UIElement;
            }
            else
            {
                var text = (string)textSource;

                var icon = ResourceManager.Instance.GetBitmapFromEmbeddedResource("item.icon.png", System.Reflection.Assembly.GetExecutingAssembly());
                var canvas = new Canvas() { Size = new Size(480, 90) };
                canvas.AddElement(new TextElement(text) { Style = MetroTheme.PhoneTextNormalStyle, Location = new Point(120, 0), Size = new Size(400, 50) });
                canvas.AddElement(new ImageElement(icon) { Location = new Point(40, 0), Size = new Size(60, 60) });
                var myStyle = MetroTheme.PhoneTextSmallStyle;
                myStyle.Foreground = MetroTheme.PhoneAccentBrush;
                canvas.AddElement(new TextElement("This is just a sample text!") { Style = myStyle, Location = new Point(120, 40), Size = new Size(400, 50) });
                return canvas;
            }
        }

        private static Canvas CreateSampleImageItem()
        {
            var sampleImageElement = new Canvas { Size = new Size(480, 200) };
            sampleImageElement.AddElement(new ImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png")) { Location = new Point(120, 0), Size = new Size(200, 150) });
            return sampleImageElement;
        }
    }
}
