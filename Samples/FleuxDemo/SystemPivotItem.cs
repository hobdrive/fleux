using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements.Pivot;
using Fleux.UIElements;
using Fleux.Templates;
using Fleux.Styles;
using Fleux.Animations;
using System.Drawing;

namespace FleuxDemo
{
    public class SystemPivotItem : PivotItem
    {
        public SystemPivotItem()
        {
            this.Title = "system";
            var items = new StackPanel();

            items.AddElement(this.CreateGroup("theme", "blue"));

            items.AddElement(this.CreateGroup("date & time", "UTC-03 Buenos Aires"));

            items.AddElement(this.CreateGroup("something longer enough to force multiline text in the title!",
                      "Something longer enough to force multiline text in the body! Something longer enough to force multiline text in the body!"));

            items.AddElement(this.CreateGroup("region & language", "United States"));

            this.Body = items;

            this.DelayInTransitions.AddRange(items.ChildrenEnumerable.Select(x => (x as TitledGroup).Content));
        }

        private TitledGroup CreateGroup(string title, string content)
        {
            return new TitledGroup
            {
                TitleStyle = MetroTheme.PhoneTextNormalStyle,
                Title = title,
                Content = new TextElement(content)
                {
                    Style = MetroTheme.PhoneTextSmallStyle,
                    AutoSizeMode = TextElement.AutoSizeModeOptions.WrapText,
                }
            };
        }
    }
}
