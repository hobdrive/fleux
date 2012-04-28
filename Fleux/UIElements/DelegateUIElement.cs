namespace Fleux.UIElements
{
    using System;
    using Core.GraphicsHelpers;

    public class DelegateUIElement : UIElement
    {
        public Action<IDrawingGraphics> DrawingAction { get; set; }

        public override void Draw(Fleux.Core.GraphicsHelpers.IDrawingGraphics drawingGraphics)
        {
            if (this.DrawingAction != null)
            {
                this.DrawingAction(drawingGraphics);
            }
        }
    }
}
