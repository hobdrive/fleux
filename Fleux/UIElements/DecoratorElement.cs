using System;
using Fleux.Core.GraphicsHelpers;
using System.Threading;

namespace Fleux.UIElements
{
    public class DecoratorElement : UIElement
    {
    
        public UIElement Target;
        
        public Action<IDrawingGraphics> DecorateBefore;
        public Action<IDrawingGraphics> DecorateAfter;
        
        public DecoratorElement (UIElement target)
        {
            Target = target;
            base.Children.Add(target);
            target.Parent = this;
            target.Updated = this.Update;
            this.SizeChanged += HandleSizeChanged;;
        }

        protected virtual void HandleSizeChanged (object sender, Fleux.UIElements.Events.SizeChangedEventArgs e)
        {
            Target.Size = e.New;
        }
        
        public override void Draw(IDrawingGraphics drawingGraphics)
        {
            if (DecorateBefore != null)
                DecorateBefore(drawingGraphics);
            Target.Draw(drawingGraphics.CreateChild(Target.Location, Target.TransformationScaling, Target.TransformationCenter));
            if (DecorateAfter != null)
                DecorateAfter(drawingGraphics);
        }
    }
}

