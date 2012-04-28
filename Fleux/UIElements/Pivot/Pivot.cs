namespace Fleux.UIElements.Pivot
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using Animations;
    using Controls.Gestures;
    using Core;
    using Core.GraphicsHelpers;
    using Styles;

    public class Pivot : Canvas
    {
        private readonly int headerPadding = 20;
        private List<PivotItem> pivotItems = new List<PivotItem>();
        private int headersWidth, headersHeight;
        private int offsetForPanning;
        private int offsetForHeaders;
        private int offsetForBody;
        private Canvas headers;
        private PivotItem currentItem;
        private bool currentFromLeft;
        private Canvas body;

        public Point HeadersLocation = new Point(21, 30);
        public Point BodyLocation = new Point(21, 150);

        public TextStyle HeaderStyleActive  = MetroTheme.PhoneTextPageTitle2Style;
        public TextStyle HeaderStylePassive = new TextStyle(MetroTheme.PhoneTextPageTitle2Style){ Foreground = MetroTheme.PhoneSubtleBrush, };

        public Pivot(string title)
            : base()
        {
            // 1. Title Section
            base.AddElement(new TextElement(title)
            {
                Style = MetroTheme.PhoneTextPageTitle1Style,
                Location = new Point(24 - 3, 9)  // -3 is a correction for Segoe fonts
            });

            this.headers = new Canvas
                {
                    Location = HeadersLocation,
                };

            this.body = new Canvas
            {
                Location = BodyLocation,
            };
            base.AddElement(this.headers);
            base.AddElement(this.body);
        }

        public PivotItem CurrentItem
        {
            get
            {
                return this.currentItem;
            }

            set
            {
                this.offsetForPanning = 0;
                var previousHeader = this.currentItem.Title;
                this.currentItem = value;
                var index = this.pivotItems.LastIndexOf(value);
                var beforeItems = this.pivotItems.GetRange(0, index);
                this.pivotItems.RemoveRange(0, index);
                this.pivotItems.AddRange(beforeItems);
                this.RefreshHeaders();
                this.AnimatePivotItemTransition(this.currentFromLeft, previousHeader);
            }
        }

        public new void AddElement(UIElement element)
        {
            // Hides AddElement method from Canvas
        }

        public void AddPivotItem(PivotItem item)
        {
            this.pivotItems.Add(item);
            this.body.Size = new Size(this.Size.Width - BodyLocation.X, this.Size.Height - BodyLocation.Y);
            item.Body.Size = this.body.Size;
            item.Body.Location = new Point(0, 0);
            this.RefreshHeaders();
            if (this.pivotItems.Count == 1)
            {
                this.currentItem = item;
            }
        }

        public override bool Pan(Point from, Point to, bool done, Point startPoint)
        {
            this.offsetForHeaders = 0;
            this.offsetForBody = 0;
            if (done && this.offsetForPanning != 0)
            {
                this.AnimateCurrentReposition();
            }
            else if (GesturesEngine.IsHorizontal(from, to))
            {
                this.offsetForPanning += (to.X - from.X) / 2;
                this.Update();
                return true;
            }

            return base.Pan(from, to, done, startPoint);
        }

        public override bool Flick(Point from, Point to, int millisecs, Point startPoint)
        {
            if (GesturesEngine.IsHorizontal(from, to))
            {
                if (this.pivotItems.Count > 1)
                {
                    if (to.X - from.X < 0)
                    {
                        this.CurrentItem = this.pivotItems[this.pivotItems.IndexOf(this.currentItem) + 1];
                    }
                    else
                    {
                        this.currentFromLeft = true;
                        this.CurrentItem = this.pivotItems.Last();
                    }
                }
                else
                {
                    this.AnimateCurrentReposition();
                }
                return true;
            }
            else if (this.offsetForPanning != 0)
            {
                this.AnimateCurrentReposition();
            }

            return base.Flick(from, to, millisecs, startPoint);
        }

        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            this.headers.Location = new Point(this.offsetForPanning + this.offsetForHeaders + HeadersLocation.X, HeadersLocation.Y);
            this.body.Clear();
            this.CurrentItem.Body.Location = new Point(0, 0);
            this.CurrentItem.Body.Size = this.body.Size;
            this.body.Location = new Point(BodyLocation.X + this.offsetForBody + (this.offsetForPanning * 2), BodyLocation.Y);
            this.body.AddElement(this.CurrentItem.Body);

            base.Draw(drawingGraphics);

            this.DrawHeaders(drawingGraphics.CreateChild(new Point(HeadersLocation.X - this.headersWidth + this.offsetForHeaders + this.offsetForPanning, HeadersLocation.Y)));
        }

        private void AnimateCurrentReposition()
        {
            StoryBoard.BeginPlay(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 200,
                From = this.offsetForPanning,
                To = 0,
                OnAnimation = v => { this.offsetForPanning = v; this.Update(); }
            });
        }

        private void AnimatePivotItemTransition(bool fromLeft, string previousHeader)
        {
            this.currentFromLeft = false;
            StoryBoard.Play(new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluid)
            {
                Duration = 300,
                From = fromLeft ? -this.Size.Width : this.CalculateHeaderSize(previousHeader).Width, // this.Size.Width,
                To = 0,
                OnAnimation = v => this.offsetForHeaders = v
            },
            new FunctionBasedAnimation(FunctionBasedAnimation.Functions.SoftedFluidExtended)
            {
                Duration = 600,
                From = fromLeft ? -this.Size.Width : this.Size.Width,
                To = 0,
                OnAnimation = v => this.offsetForBody = v
            },
            this.CurrentItem.GetDelayedAnimation(fromLeft ? -this.Size.Width : this.Size.Width),
            new CommitStoryboardAnimation
            {
                Duration = 600,
                CommitAction = this.Update
            });
        }

        private void DrawHeaders(IDrawingGraphics gr)
        {
            foreach (var h in this.pivotItems)
            {
                gr.Style(this.HeaderStylePassive).DrawText(h.Title).MoveRelX(this.headerPadding);
            }
        }

        private void RefreshHeaders()
        {
            int x = 0;
            this.headers.Clear();
            foreach (var i in this.pivotItems)
            {
                var h = this.CreateHeader(i, this.CurrentItem);
                h.Location = new Point(x, 0);
                this.headers.AddElement(h);
                x += h.Size.Width + this.headerPadding;
                this.headersHeight = Math.Max(this.headersHeight, h.Size.Height);
            }
            this.headersWidth = x;
            foreach (var i in this.pivotItems)
            {
                var h = this.CreateHeader(i, null);
                h.Location = new Point(x, 0);
                this.headers.AddElement(h);
                x += h.Size.Width + this.headerPadding;
            }
            this.headers.Size = new Size(this.headersWidth, this.headersHeight);
        }

        private TextElement CreateHeader(PivotItem i, PivotItem selected)
        {
            var style = i == selected ? this.HeaderStyleActive : this.HeaderStylePassive;
            return new TextElement(i.Title)
            {
                Style = style,
                Size = this.CalculateHeaderSize(i.Title),
                TapHandler = p =>
                    {
                        if (this.currentItem != i)
                        {
                            this.CurrentItem = i;
                        }
                        return true;
                    }
            };
        }

        private Size CalculateHeaderSize(string p)
        {
            return FleuxApplication.DummyDrawingGraphics.MoveTo(0, 0)
                .Style(this.HeaderStyleActive)
                .CalculateTextSize(p);
        }
    }
}