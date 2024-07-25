using QWindowFormSplit.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QWindowFormSplit.Controls
{
    public class ScreenItem : ContentControl
    {
        public double WidthPercentage
        {
            get { return (double)GetValue(WidthPercentageProperty); }
            set { SetValue(WidthPercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WidthPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WidthPercentageProperty =
            DependencyProperty.Register("WidthPercentage", typeof(double), typeof(ScreenItem), new PropertyMetadata(100.0));



        public double HeightPercentage
        {
            get { return (double)GetValue(HeightPercentageProperty); }
            set { SetValue(HeightPercentageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeightPercentage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeightPercentageProperty =
            DependencyProperty.Register("HeightPercentage", typeof(double), typeof(ScreenItem), new PropertyMetadata(100.0));




        static ScreenItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScreenItem), new FrameworkPropertyMetadata(typeof(ScreenItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var sg = new Size()
            {
                Width = constraint.Width * (WidthPercentage / 100),
                Height = constraint.Height * (HeightPercentage / 100)
            };
            return sg;
        }

    }
}
