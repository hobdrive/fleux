using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Fleux.UIElements;
using Fleux.Core;
using System.Drawing;
using Fleux.Animations;

namespace WindowsPhone7Sample.Elements
{
    public class Arrow : ImageElement
    {
        public Arrow()
            : base(ResourceManager.Instance.GetBitmapFromEmbeddedResource("right.png"))
        {
            this.Size = new Size(45, 45);
        }
    }
}
