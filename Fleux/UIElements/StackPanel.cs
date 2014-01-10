using System.Linq;

namespace Fleux.UIElements
{
    using System.Drawing;

    public class StackPanel : Canvas
    {
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
            this.SizeChanged += (s, e) => this.Relayout();
        }

        public override void AddElement(UIElement element)
        {
            base.AddElement(element);
            this.Relayout();
        }

        public void Relayout()
        {
            var nextLineLocation = 0;
            var nextColumnIndex = 0;

            foreach (var child in Children)
            {
                var nextColumnLocation = GetNextColumnLocation(nextColumnIndex, child);

                child.Location = GetLocationPoint(nextColumnLocation, nextLineLocation);

                ResizeChild(child);

                nextColumnIndex = (nextColumnIndex + 1)%columns;

                nextLineLocation = GetNewLineLocation(nextLineLocation, child, nextColumnIndex);
            }

            IncreaseSizeIfNeed();
        }

        private void ResizeChild(UIElement child)
        {
            if (IsVertical)
            {
                child.ResizeForWidth((int) GetChildColumnSize());
            }
            else
            {
                child.ResizeForHeight((int) GetChildColumnSize());
            }
        }

        private void IncreaseSizeIfNeed()
        {
            var lastChild = Children.LastOrDefault();

            if (lastChild == null)
                return;

            if (IsVertical)
            {
                if (lastChild.Location.Y + lastChild.Size.Height > Size.Height)
                    Height = lastChild.Location.Y + lastChild.Size.Height + Padding;
            }
            else
            {
                if (lastChild.Location.X + lastChild.Size.Width > Size.Width)
                    Width = lastChild.Location.X + lastChild.Size.Width + Padding;
            }
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