using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.Controls;
using Fleux.UIElements;
using Fleux.Core;
using System.Drawing;
using Fleux.Animations;
using WindowsPhone7Sample.Elements;
using Fleux.UIElements.Panorama;

namespace WindowsPhone7Sample
{
    public class HomeScreen : FleuxControlPage
    {
        private bool launching;
        private List<UIElement> tiles = new List<UIElement>();
        private Canvas homeScreenCanvas;
        private Arrow switchArrow;
        bool showingTiles = true;

        public HomeScreen()
            : base(true)
        {
            this.Control.EntranceDuration = 500;
            homeScreenCanvas = new Canvas { Size = new Size(960, 740) };
            var canvas = new Canvas() { Size = new Size(400, 1203) };

            tiles.Add(this.CreateTile("Phone", 28, 93));
            tiles.Add(this.SetEntranceAnimationFromLeft(new PeopleTile(213, 93)));
            tiles.Add(this.CreateTile("Messaging", 28, 278));
            tiles.Add(this.CreateTile("Email", 213, 278));
            tiles.Add(this.CreateTile("Internet", 28, 463));
            tiles.Add(this.CreateTile("Games", 213, 463));
            tiles.Add(this.CreateTile("Calendar", 28, 648, true));
            tiles.Add(this.CreateTile("Pictures", 28, 833, true));
            tiles.Add(this.CreateTile("Zune", 28, 1018));
            tiles.Add(this.CreateTile("Marketplace", 213, 1018));

            tiles.ForEach(el => { canvas.AddElement(el); this.SetExitAnimationToLeft(el, this.ShowGames); });

            var sv = new ScrollViewer
            {
                Content = canvas,
                Size = new Size(400, this.Size.Height - 62),
                Location = new Point(0, 0),
                VerticalScroll = true,
            };

            this.homeScreenCanvas.AddElement(sv);

            this.Control.AddElement(new SoftKeys()
            {
                Size = new Size(480, 60),
                Location = new Point(0, this.Size.Height - 60)
            });

            this.switchArrow = new Arrow()
            {
                Location = new Point(400, 93),
                TapHandler = this.TapOnArrow,
            };
            this.homeScreenCanvas.AddElement(this.switchArrow);

            this.homeScreenCanvas.PanHandler = this.Pan;
            this.homeScreenCanvas.FlickHandler = this.Flick;

            var programsSv = new ScrollViewer { Size = new Size(315, this.Size.Height - 62), Location = new Point(574, 0), VerticalScroll = true };
            programsSv.Content = new ImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource("Programs.png"))
            {
                Size = new Size(315, 893),
                EntranceAnimation = new NullAnimation(),
                ExitAnimation = new NullAnimation()
            };

            this.homeScreenCanvas.AddElement(programsSv);

            this.Control.AddElement(this.homeScreenCanvas);
            this.Control.AddElement(new Clock() { Location = new Point(425, 0) });

            this.homeScreenCanvas.TapHandler = p => { this.ReShow(); return true; };
        }

        private bool Pan(Point from, Point to, bool done, Point start)
        {
            this.homeScreenCanvas.Location = new Point(this.homeScreenCanvas.Location.X + to.X - from.X, 0);
            this.homeScreenCanvas.Update();
            if (done)
            {
                if (this.homeScreenCanvas.Location.X > -300 && this.homeScreenCanvas.Location.X != 0)
                {
                    this.GoToTiles();
                }
                else if (this.homeScreenCanvas.Location.X < -300 && this.homeScreenCanvas.Location.X != -480)
                {
                    this.GoToPrograms();
                }
            }
            return true;
        }

        private bool Flick(Point from, Point to, int millisecs, Point start)
        {
            if (to.X - from.X > 0)
            {
                this.GoToTiles();
            }
            else
            {
                this.GoToPrograms();
            }
            return true;
        }

        private void GoToTiles()
        {
            StoryBoard.BeginPlay(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 500,
                From = this.homeScreenCanvas.Location.X,
                To = 0,
                OnAnimation = v => { this.homeScreenCanvas.Location = new Point(v, 0); this.homeScreenCanvas.Update(); }
            });
            StoryBoard.BeginPlay(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 500,
                From = this.switchArrow.Location.X,
                To = 400,
                OnAnimation = v =>
                {
                    this.switchArrow.Location = new Point(v, this.switchArrow.Location.Y);
                    this.switchArrow.Update();
                }
            });
            this.switchArrow.Image = ResourceManager.Instance.GetBitmapFromEmbeddedResource("right.png");
            this.showingTiles = true;
        }

        private void GoToPrograms()
        {
            StoryBoard.BeginPlay(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 500,
                From = this.homeScreenCanvas.Location.X,
                To = -480,
                OnAnimation = v => { this.homeScreenCanvas.Location = new Point(v, 0); this.homeScreenCanvas.Update(); }
            });
            StoryBoard.BeginPlay(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 500,
                From = this.switchArrow.Location.X,
                To = 500,
                OnAnimation = v =>
                {
                    this.switchArrow.Location = new Point(v, this.switchArrow.Location.Y);
                    this.switchArrow.Update();
                }
            });
            this.switchArrow.Image = ResourceManager.Instance.GetBitmapFromEmbeddedResource("left.png");
            this.showingTiles = false;
        }

        public bool TapOnArrow(Point p)
        {
            if (this.showingTiles)
            {
                this.GoToPrograms();
            }
            else
            {
                this.GoToTiles();
            }
            return true;
        }

        private ImageElement CreateTile(string name, int x, int y)
        {
            return this.CreateTile(name, x, y, false);
        }

        private ImageElement CreateTile(string name, int x, int y, bool wide)
        {
            var image = new ImageElement(ResourceManager.Instance.GetBitmapFromEmbeddedResource(string.Format("{0}.png", name)))
            {
                Location = new Point(x, y),
                Size = new Size(wide ? 358 : 173, 173),
            };
            this.SetEntranceAnimationFromLeft(image);
            return image;
        }

        private UIElement SetEntranceAnimationFromLeft(UIElement target)
        {
            var random = new Random();
            var x = target.Location.X;
            target.EntranceAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.BounceEntranceSin)
            {
                From = x - 1000 + random.Next(1000 - x - 173),
                To = x,
                OnAnimation = v => target.Location = new Point(v, target.Location.Y)
            };
            return target;
        }

        private void SetExitAnimationToLeft(UIElement target, Action<UIElement> tapAction)
        {
            var random = new Random();
            var x = target.Location.X;
            target.ExitAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.BounceExitSin)
            {
                To = -target.Size.Width - random.Next(1000),
                From = x,
                OnAnimation = v => target.Location = new Point(v, target.Location.Y)
            };
            target.TapHandler = p =>
            {
                (target.ExitAnimation as FunctionBasedAnimation).EaseFunction = v => Math.Pow(v, 15);
                this.Control.AnimateExit();
                this.launching = true;
                tapAction(target);
                return true;
            };
        }

        protected override void OnActivated()
        {
            this.ReShow();
            base.OnActivated();
        }

        private UIElement SetEntranceAnimationFromBottom(UIElement target)
        {
            target.EntranceAnimation = new FunctionBasedAnimation(FunctionBasedAnimation.Functions.BounceEntranceSin)
            {
                From = target.Location.Y + target.Size.Height + 200 + new Random().Next(200),
                To = target.Location.Y,
                OnAnimation = v => target.Location = new Point(target.Location.X, v)
            };
            return target;
        }

        private void ReShow()
        {
            if (this.launching)
            {
                this.Control.AnimateEntrance();
                this.SetExitAnimationToLeft(this.launchedTile, this.ShowGames);
                this.launching = false;
            }
        }

        UIElement launchedTile;

        private void ShowGames(UIElement target)
        {
            this.launchedTile = target;
        }
    }
}
