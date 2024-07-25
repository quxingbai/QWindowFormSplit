using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace QWindowFormSplit.Controls
{
    public class ScreenItemPanel : Panel
    {
        protected override Size ArrangeOverride(Size finalSize)
        {
            int count = 0;
            double x = 0, y = 0;
            foreach (UIElement i in Children)
            {
                i.Measure(finalSize);   
                double width = i.DesiredSize.Width, height = i.DesiredSize.Height;
                y = finalSize.Height - height;
                i.Arrange(new Rect(x, y, width, height));
                x += i.DesiredSize.Width;
                count++;
            }
            return base.ArrangeOverride(finalSize);
        }
        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    availableSize = this.DesiredSize;
        //    foreach (UIElement i in Children)
        //    {
        //        i.Measure(availableSize);
        //    }
        //    return base.MeasureOverride(availableSize);
        //}
    }
}
