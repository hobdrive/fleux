namespace FleuxDemo
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using Fleux.Controls;
    using Fleux.Core;
    using Fleux.Styles;
    using Fleux.Templates;
    using Fleux.UIElements;
    using Fleux.UIElements.Panorama;

    public class PanoramaPage : FleuxControlPage
    {
        private PanoramaElement panorama;
        private bool needsEntranceAnimation;

        public PanoramaPage()
        {
            RightMenu.DisplayText = "Exit";
            RightMenu.OnClickAction = () => this.Close();

            // Definition for Entrance and Exit Animations
            this.Control.EntranceDuration = 300;
            this.Control.ShadowedAnimationMode = FleuxControl.ShadowedAnimationOptions.FromLeft;

            // Panorama
            this.panorama = new PanoramaElement { Size = this.Size };

            // Title
            this.panorama.Title.AddElement(new DelegateUIElement
                        {
                            Size = new Size(1000, 300),
                            DrawingAction = gr => gr.MoveRel(0, -80).Style(MetroTheme.PhoneTextPanoramaTitleStyle)
                                                    .Color(Color.FromArgb(190, 221, 226))
                                                    .DrawText("panorama").MoveY(0)
                                                    .Style(MetroTheme.PhoneTextLargeStyle)
                                                    .Color(Color.FromArgb(136, 209, 255))
                                                    .DrawText("by fleux")
                        });

            // Background
            this.panorama.Background.AddElement(new ImageElement("background.jpg"));

            // Features
            this.panorama.AddSection(this.CreateMenuSection());

            // Images
            this.panorama.AddSection(this.CreateImagesSection());

            // Horizontal (wide)
            this.panorama.AddSection(this.CreateHorizontalImagesSection(), true);

            // Features
            this.panorama.AddSection(this.CreateMenuSection());

            this.Control.AddElement(this.panorama);
        }

        protected override void NavigateTo(FleuxPage page)
        {
            this.needsEntranceAnimation = true;
            this.panorama.SetAnimations();
            base.NavigateTo(page);
        }

        protected override void OnActivated()
        {
            if (this.needsEntranceAnimation)
            {
                this.Control.AnimateEntrance();
                this.needsEntranceAnimation = false;
            }
            base.OnActivated();
        }

        private PanoramaSection CreateMenuSection()
        {
            var style = new TextStyle(MetroTheme.PhoneTextLargeStyle.FontFamily,
                                      MetroTheme.PhoneFontSizeMediumLarge,
                                      MetroTheme.PanoramaNormalBrush);

            var section = new PanoramaSection("welcome");
            section.Body.AddElement(this.CreateMenuSectionElement(0, "gestures", p => this.NavigateToHandler(new GesturesTestPage())));
            section.Body.AddElement(this.CreateMenuSectionElement(80, "list page", p => this.NavigateToHandler(new ListPage())));
            section.Body.AddElement(this.CreateMenuSectionElement(160, "pivot", p => this.NavigateToHandler(new PivotPage())));
            section.Body.AddElement(this.CreateMenuSectionElement(240, "text samples", p => this.NavigateToHandler(new TextPage())));

            section.Body.AddElement(new TextElement("more is coming...")
            {
                Style = MetroTheme.PhoneTextNormalStyle,
                Location = new Point(20, 320),
                Size = new Size(300, 80)
            });

            return section;
        }

        private UIElement CreateMenuSectionElement(int y, string text, Func<Point, bool> tapHandler)
        {
            return new TextElement(text)
            {
                Style = new TextStyle(MetroTheme.PhoneTextLargeStyle.FontFamily, MetroTheme.PhoneFontSizeMediumLarge, MetroTheme.PanoramaNormalBrush),
                Location = new Point(20, y),
                Size = new Size(300, 80),
                TapHandler = tapHandler,
                PressFeedbackSupported = true
            };
        }

        private PanoramaSection CreateImagesSection()
        {
            var img1 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png");
            var img2 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("squareimg.png");
            var img3 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail2.png");

            var section = new PanoramaSection("small");
            section.Body.AddElement(new ListElement
            {
                DataTemplateSelector = item => i => (UIElement)i, // Our sample list items are all UIElements
                SourceItems = new BindingList<object> 
                {
                            this.CreatePictureItem(img1, "ONE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img2, "TWO", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img3, "THREE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img1, "FOUR", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img1, "ONE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img2, "TWO", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img3, "THREE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img1, "FOUR", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img1, "ONE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img2, "TWO", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img3, "THREE", "LOREM IPSUM LOREM"),
                            this.CreatePictureItem(img1, "FOUR", "LOREM IPSUM LOREM"),
                },
                Size = new Size(380, 466),
                ShowScrollbars = false
            });

            return section;
        }

        private UIElement CreatePictureItem(Image image, string title, string subtitle)
        {
            return new DelegateUIElement
            {
                Size = new Size(380, 120),
                DrawingAction = gr => gr.DrawImage(image, new Rectangle(0, 0, 100, 100))
                    .Style(new TextStyle(MetroTheme.PhoneFontFamilySemiBold, MetroTheme.PhoneFontSizeLarge, Color.White))
                    .MoveTo(120, 0).DrawText(title)
                    .Style(MetroTheme.PhoneTextBlockBase)
                    .MoveTo(120, 70).DrawText(subtitle),
                TapHandler = p => this.NavigateToHandler(new ImageDetailsPage(title, subtitle, image)),
                PressFeedbackSupported = true
            };
        }

        private PanoramaSection CreateHorizontalImagesSection()
        {
            var img1 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail.png");
            var img2 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("squareimg.png");
            var img3 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail2.png");
            var img4 = ResourceManager.Instance.GetBitmapFromEmbeddedResource("thumbnail3.png");

            var section = new PanoramaSection("images");
            section.Body.AddElement(this.CreateIcon(img1, 0, 0,
                p => this.NavigateToHandler(new ImageDetailsPage("one", "FLEUX SAMPLE", img1))));
            section.Body.AddElement(this.CreateIcon(img2, 200, 0,
                p => this.NavigateToHandler(new ImageDetailsPage("two", "FLEUX SAMPLE", img2))));
            section.Body.AddElement(this.CreateIcon(img3, 400, 0,
                p => this.NavigateToHandler(new ImageDetailsPage("three", "FLEUX SAMPLE", img3))));
            section.Body.AddElement(this.CreateIcon(img4, 600, 0,
                p => this.NavigateToHandler(new ImageDetailsPage("four", "FLEUX SAMPLE", img4))));
            section.Body.AddElement(this.CreateIcon(img4, 0, 160,
                p => this.NavigateToHandler(new ImageDetailsPage("four", "FLEUX SAMPLE", img4))));
            section.Body.AddElement(this.CreateIcon(img3, 200, 160,
                p => this.NavigateToHandler(new ImageDetailsPage("three", "FLEUX SAMPLE", img3))));
            section.Body.AddElement(this.CreateIcon(img1, 400, 160,
                p => this.NavigateToHandler(new ImageDetailsPage("one", "FLEUX SAMPLE", img1))));
            section.Body.AddElement(this.CreateIcon(img2, 600, 160,
                p => this.NavigateToHandler(new ImageDetailsPage("two", "FLEUX SAMPLE", img2))));

            return section;
        }

        private ImageElement CreateIcon(Image img, int x, int y, System.Func<Point, bool> tapHandler)
        {
            return new ImageElement(img)
            {
                Size = new Size(190, 150),
                Location = new Point(x, y),
                TapHandler = tapHandler,
                TransformationCenter = new Point(95,75),
                PressFeedbackSupported = true
            };
        }

        private bool NavigateToHandler(FleuxPage page)
        {
            this.NavigateTo(page);
            return true;
        }
    }
}