namespace Fleux.UIElements.Grid
{
    using System;

    public class MeasureDefinition
    {
        public enum LayoutMode
        {
            Fixed,
            Auto,
            Fill
        }
        
        public LayoutMode Mode { get; private set; }
        
        public int Value { get; private set; }

        public int Size { get; set; }

        public int Position { get; set; }

        public static implicit operator MeasureDefinition(int value)
        {
            return new MeasureDefinition { Value = value, Mode = LayoutMode.Fixed };
        }

        public static implicit operator MeasureDefinition(string value)
        {
            // TODO: Remove this throw once auto and fill are supported.
            throw new InvalidOperationException("Not fixed definitions are not allowed yet.");
            
            ////switch (value)
            ////{
            ////    case "*":
            ////        return new MeasureDefinition { Mode = LayoutMode.Fill };
            ////    case "Auto":
            ////        return new MeasureDefinition { Mode = LayoutMode.Auto };
            ////}
            
            ////throw new ArgumentOutOfRangeException("value", "Measure definition can only be an int number, 'Auto' or '*'");
        }
    }
}
