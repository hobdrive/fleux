using System;
using Fleux.Core.GraphicsHelpers;
using System.Threading;
using Fleux.Core;

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
            try{
                if (DecorateBefore != null)
                    DecorateBefore(drawingGraphics);
            }catch(Exception e)
            {
                FleuxApplication.Log(e);
            }
            if (Target.Visible)
                Target.Draw(drawingGraphics.CreateChild(Target.Location, Target.Transformation));
            try{
                if (DecorateAfter != null)
                    DecorateAfter(drawingGraphics);
            }catch(Exception e)
            {
                FleuxApplication.Log(e);
            }
        }
    }
}

