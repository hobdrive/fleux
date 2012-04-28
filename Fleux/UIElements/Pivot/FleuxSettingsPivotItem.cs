namespace Fleux.UIElements.Pivot
{
    using System;
    using System.Linq;
    using Styles;
    using Templates;

    public class FleuxSettingsPivotItem : PivotItem
    {
        public FleuxSettingsPivotItem()
        {
            this.Title = "fleux";
            var items = new StackPanel();

            items.AddElement(this.CreateItem("kinetic scrolling", "physics inertia 2x"));
            items.AddElement(this.CreateItem("haptic feedback", "tap on items marked for feedback"));
            items.AddElement(this.CreateItem("background on panorama", "enabled"));
            items.AddElement(this.CreateItem("shadowed transitions", "enabled"));

            this.Body = items;
        }

        private TitledGroup CreateItem(string name, string value)
        {
            var content = new TextElement(value)
                {
                    Style = MetroTheme.PhoneTextSmallStyle,
                    AutoSizeMode = TextElement.AutoSizeModeOptions.OneLineAutoHeight
                };
            this.DelayInTransitions.Add(content);
            return new TitledGroup
            {
                TitleStyle = MetroTheme.PhoneTextNormalStyle,
                Title = name,
                Content = content
            };
        }
    }
}
