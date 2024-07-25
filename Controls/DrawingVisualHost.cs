using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QWindowFormSplit.Controls
{
    public class DrawingVisualHost: FrameworkElement
    {

        public DrawingVisual Target { get; set; }
        public DrawingVisualHost()
        {

        }
        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index)
        {
            return Target;
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawDrawing(Target.Drawing);
            base.OnRender(drawingContext);
        }

    }
}
