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
    public class ScreenSplitRanges : Control
    {
        static ScreenSplitRanges()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScreenSplitRanges), new FrameworkPropertyMetadata(typeof(ScreenSplitRanges)));
        }
        public event Action<ScreenSplitRanges,Point, Point> SplitLineDrawing;
        private Point LastDrawingStart;
        private Point LastDrawingEnd;
        private Pen CreatePen()
        {
            return new Pen(Brushes.DimGray, 2);
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            InvalidateVisual();
            base.OnPreviewMouseMove(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if(e.ChangedButton == MouseButton.Left)
            {
                SplitLineDrawing?.Invoke(this, LastDrawingStart, LastDrawingEnd);
            }
            base.OnPreviewMouseDown(e);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var mousePos = Mouse.GetPosition(this);
            Pen pen = CreatePen();
            double xySplitRange = 80;
            //如果是横向模式
            if (mousePos.X < xySplitRange||mousePos.X>(RenderSize.Width- xySplitRange))
            {
                DrawingTestLine(0);
            }
            else
            {
                DrawingTestLine(1);
            }


            //direction 0横线 1竖线
            void DrawingTestLine(int direction = 0)
            {
                Point start = new Point(), end = new Point();
                switch (direction)
                {
                    case 0:
                        {
                            start.Y = mousePos.Y;
                            end.Y = mousePos.Y;
                            end.X = RenderSize.Width;
                        }; break;
                    case 1:
                        {
                            start.X = mousePos.X;
                            end.X = mousePos.X;
                            end.Y = RenderSize.Height;
                        }; break;
                }
                LastDrawingStart= start;
                LastDrawingEnd= end;
                drawingContext.DrawLine(pen, start, end);
            }
        }
    }
}
