namespace Fleux.UIElements
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using Core.GraphicsHelpers;
    using Events;

    public class ListElement : ScrollViewer
    {
        protected BindingList<object> sourceItems;
        protected Canvas contentCanvas;
        protected int currentFirstItem = 0;

        public ListElement()
        {
            this.VerticalScroll = true;
            // fix: this.Size should be correctly initialized to create items!
            this.SizeChanged += this.ListSizeChanged;
        }

        void ListSizeChanged(object list, SizeChangedEventArgs args)
        {
            // recreate items
            this.SourceItems = this.SourceItems;
        }

        public virtual BindingList<object> SourceItems
        {
            get
            {
                return this.sourceItems;
            }

            set
            {
                // fix: Threading makes a race condition here
                this.sourceItems = value;
                int y = 0;
                this.contentCanvas = new Canvas();
                foreach (var o in this.sourceItems)
                {
                    var uiE = this.DataTemplateSelector(o)(o);
                    uiE.Location = new Point(0, y);
                    this.contentCanvas.AddElement(uiE);
                    y += uiE.Size.Height;
                }
                this.contentCanvas.Size = new Size(this.Size.Width, y);
                this.Content = this.contentCanvas;
            }
        }

        public Func<object, Func<object, UIElement>> DataTemplateSelector { get; set; }
    }
}
