﻿using System;
using System.Linq;

namespace Fleux.UIElements
{
    using System.Drawing;

    /// <summary>
    /// If AutoSize=true, panel will expect already sized children, and will adapt its own size.
    /// if AutoSize=False, panel should get its size fixed from outside, children will be equally autofitted into panel.
    /// Column width is anyway always forced (child width for vertical, height for horizontal panel)
    /// </summary>
    public class StackPanel : Canvas
    {
        private bool _noNeedReloyout;

        public int Padding{ get; set; }

        int columns = 1;
        private bool _isVertical;

        public int Columns
        {
            get { return columns; }
            set
            {
                columns = value;
                Relayout();
            }
        }

        public bool IsVertical
        {
            get { return _isVertical; }
            set
            {
                _isVertical = value;
                this.Relayout();
            }
        }

        public StackPanel() : this(true)
        {
        }

        public StackPanel(bool isVertical)
        {
            _isVertical = isVertical;

            base.AutoResize = false;
            this.Padding = 0;
            this.SizeChanged += OnSizeChanged;
        }

        void OnSizeChanged(object sender, Events.SizeChangedEventArgs e)
        {
            this.Relayout();
        }

        public override void AddElement(UIElement element)
        {
            base.AddElement(element);
            element.VisibleChanged += element_VisibleChanged;
            this.Relayout();
        }

        public override void RemoveElement(UIElement element)
        {
            base.RemoveElement(element);
            element.VisibleChanged -= element_VisibleChanged;
            this.Relayout();
        }

        private void element_VisibleChanged(object sender, System.EventArgs e)
        {
            Relayout();
        }

        public void Relayout()
        {
            if (_noNeedReloyout)
                return;

            var nextLineLocation = 0;
            var nextColumnIndex = 0;

            foreach (var child in Children.Where(child => child.Visible).ToArray())
            {
                var nextColumnLocation = GetNextColumnLocation(nextColumnIndex, child);

                child.Location = GetLocationPoint(nextColumnLocation, nextLineLocation);

                ResizeChild(child);

                nextColumnIndex = (nextColumnIndex + 1)%columns;

                nextLineLocation = GetNewLineLocation(nextLineLocation, child, nextColumnIndex);
            }

            WithoutRelayout(ChangeSizeIfNeed);
        }

        private void ResizeChild(UIElement child)
        {
            var rows = base.Children.Count(child => child.Visible) / Columns;
            if (rows == 0)
                rows = 1;
            if (IsVertical)
            {
                if (!AutoResize)
                {
                    child.Size = new Size(child.Size.Width, this.Size.Height / rows - Padding);
                }
                if (child.Size.Width == 0)
                {
                    child.Size = new Size((int)GetChildColumnSize(), child.Size.Height);
                }
                child.ResizeForWidth((int) GetChildColumnSize());
            }
            else
            {
                if (!AutoResize)
                {
                    child.Size = new Size(this.Size.Width / rows - Padding, child.Size.Height);
                }
                if (child.Size.Height == 0)
                {
                    child.Size = new Size(child.Size.Width, (int)GetChildColumnSize());
                }
                child.ResizeForHeight((int) GetChildColumnSize());
            }
        }

        private void ChangeSizeIfNeed()
        {
            if (!Children.Any())
                return;

            var desiredSize = GetDesiredSize();

            if (AutoResize)
            {
                Size = desiredSize;
            }
            else
            {
                /*
                if (IsVertical)
                {
                    if (desiredSize.Height > Size.Height)
                        Height = desiredSize.Height;

                }
                else
                {
                    if (desiredSize.Width > Size.Width)
                        Width = desiredSize.Width;

                }
                */
            }
        }

        private void WithoutRelayout(Action action)
        {
            _noNeedReloyout = true;

            try
            {
                action();
            }
            finally
            {
                _noNeedReloyout = false;
            }
        }

        private Size GetDesiredSize()
        {
            var visibleChildren = Children.Where(ch => ch.Visible);

            if (visibleChildren.Count() == 0)
                return new Size(0,0);

            var desiredHeight = visibleChildren.Max(ch => ch.Location.Y + ch.Height) + Padding * 2;
            var desiredWidth = visibleChildren.Max(ch => ch.Location.X + ch.Width) + Padding * 2;

            return new Size(desiredWidth, desiredHeight);
        }

        private double GetChildColumnSize()
        {
            var size = IsVertical ? Width : Height;

            return (double) size/columns - Padding*columns - Padding;
        }

        private int GetNextColumnLocation(int nextColumnIndex, UIElement child)
        {
            var childColumnSize = GetChildColumnSize();

            var initialLocation = 0;
            if (nextColumnIndex == 0)
            {
                initialLocation = IsVertical ? child.Location.X : child.Location.Y;
            }

            var padding = Padding*nextColumnIndex;
            var columnLocation = childColumnSize*nextColumnIndex;

            return (int) (initialLocation + columnLocation + padding);
        }

        private int GetNewLineLocation(int currentLineLocation, UIElement previousChild, int nextColumnIndex)
        {
            if (nextColumnIndex != 0)
                return currentLineLocation;

            var previousChildSize = IsVertical ? previousChild.Size.Height : previousChild.Size.Width;

            return currentLineLocation + previousChildSize + Padding;
        }

        private Point GetLocationPoint(int nextColumnLocation, int nextLineLocation)
        {
            return IsVertical
                ? new Point(nextColumnLocation, nextLineLocation)
                : new Point(nextLineLocation, nextColumnLocation);
        }
    }
}
