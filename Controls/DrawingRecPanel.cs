using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace QWindowFormSplit.Controls
{
    public class DrawingRecPanel : Panel
    {




        public int MaxBreakCount
        {
            get { return (int)GetValue(MaxBreakCountProperty); }
            set { SetValue(MaxBreakCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxBreakCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxBreakCountProperty =
            DependencyProperty.Register("MaxBreakCount", typeof(int), typeof(DrawingRecPanel), new PropertyMetadata(-1));




        public (Rect, Size) DrawRelation
        {
            get { return ((Rect, Size))GetValue(DrawRelationProperty); }
            set { SetValue(DrawRelationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawRelation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawRelationProperty =
            DependencyProperty.Register("DrawRelation", typeof((Rect, Size)), typeof(DrawingRecPanel), new PropertyMetadata(null));



        public Brush DrawLineBrush
        {
            get { return (Brush)GetValue(DrawLineBrushProperty); }
            set { SetValue(DrawLineBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawLineBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DrawLineBrushProperty =
            DependencyProperty.Register("DrawLineBrush", typeof(Brush), typeof(DrawingRecPanel), new PropertyMetadata(Brushes.White));



        public static (Rect, Size) GetDrawRelation(UIElement target) => ((Rect, Size))target.GetValue(DrawRelationProperty);
        public static void SetDrawRelation(UIElement target, (Rect, Size) data) => target.SetValue(DrawRelationProperty, data);

        private (DrawingVisual, DrawingVisualHost) DrawingTarget = (null, null);
        private Pen DrawingPen =>new Pen(DrawLineBrush,2);
        private (Point, Point) LastDrawingLine = (new Point(), new Point());
        private List<(Rect, Size)> SplitRects = new List<(Rect, Size)>();
        private bool BreakState = true;
        private bool CanBreak => (MaxBreakCount == -1 || SplitRects.Count < MaxBreakCount) && BreakState;
        public DrawingRecPanel()
        {
        }


        protected override Size ArrangeOverride(Size finalSize)
        {
            var ranges = GetAllRelativeRects().ToArray();
            int rangeIdx = 0;
            if (ranges.Length > 0)
                foreach (UIElement i in Children)
                {
                    Rect r = new Rect();
                    if (rangeIdx >= ranges.Length)
                    {
                        if (rangeIdx==0)
                        {
                        }
                    }
                    else
                    {
                        SetDrawRelation(i, ranges[rangeIdx].Item2);
                        r = ranges[rangeIdx++].Item1;
                    }
                    i.Arrange(r);
                }
            else
            {
                foreach (UIElement i in Children)
                {
                    if (rangeIdx++ == 0)
                    {
                        i.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
                        SetDrawRelation(i, (new Rect(0, 0, 1, 1), new Size(1, 1)));
                    }
                    else
                    {
                        i.Arrange(new Rect());
                    }
                }
            }
            return base.ArrangeOverride(finalSize);
        }

        /// <summary>
        /// 获取所有相对当前大小的范围
        /// </summary>
        public IEnumerable<(Rect, (Rect, Size))> GetAllRelativeRects()
        {
            List<(Rect, (Rect, Size))> recs = new List<(Rect, (Rect, Size))>();
            var rs = RenderSize;
            for (int i = 0; i < SplitRects.Count; i++)
            {
                var item = SplitRects[i];
                var rec = item.Item1;
                double widthChange = rs.Width - item.Item2.Width;
                double heightChange = rs.Height - item.Item2.Height;
                double wp = (widthChange / item.Item2.Width) + 1, hp = (heightChange / item.Item2.Height) + 1;
                recs.Add((new Rect(rec.X * wp, rec.Y * hp, rec.Width * wp, rec.Height * hp), item));
            }
            return recs;
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            BreakState = Keyboard.IsKeyDown(Key.LeftCtrl);
            InvalidateVisual();
            base.OnPreviewMouseMove(e);
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!CanBreak) return;
            var nowPos = e.GetPosition(this);
            var nowSize = RenderSize;
            var line = this.LastDrawingLine;
            var rec = GetPosRect(nowPos);
            SplitRect(line.Item1, line.Item2, rec.Item1, out var r1, out var r2);

            this.SplitRects.Add((r1, nowSize));
            this.SplitRects.Add((r2, nowSize));
            this.SplitRects.Remove(rec.Item2);
            InvalidateVisual();
            base.OnPreviewMouseLeftButtonDown(e);
        }
        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            SplitRects.Clear();
            InvalidateVisual();
            base.OnPreviewMouseRightButtonDown(e);
        }
        protected override void OnRender(DrawingContext drawingContext)
        {
            if (IsMouseOver && CanBreak)
            {
                DrawingSplitImages(drawingContext);
            }
            base.OnRender(drawingContext);
            return;
            var pen = GetDrawingPen();
            foreach (var i in GetAllRelativeRects())
            {
                drawingContext.DrawRectangle(null, pen, i.Item1);
            }
            base.OnRender(drawingContext);
        }
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            InvalidateVisual();
            base.OnMouseLeave(e);
        }
        private Pen GetDrawingPen() => DrawingPen;
        /// <summary>
        /// 用一条线切割一个矩形（到头）
        /// </summary>
        /// <param name="Start">开始位</param>
        /// <param name="End">结束位</param>
        /// <param name="Source">源矩形</param>
        /// <param name="R1">返回第一个</param>
        /// <param name="R2">返回第二个</param>
        private void SplitRect(Point Start, Point End, Rect Source, out Rect R1, out Rect R2)
        {
            bool IsHoriz = Start.Y == End.Y;
            //如果是横线
            if (IsHoriz)
            {
                Point lineRight = End.X > Start.X ? End : Start;
                Point lineLeft = End.X > Start.X ? Start : End;
                R1 = new Rect(Source.TopLeft, lineRight);
                R2 = new Rect(lineLeft, Source.BottomRight);
            }
            else
            {
                Point lineTop = Start.Y < End.Y ? Start : End;
                Point lineBottom = Start.Y < End.Y ? End : Start;
                R1 = new Rect(Source.TopLeft, lineBottom);
                R2 = new Rect(lineTop, Source.BottomRight);
            }
        }
        /// <summary>
        /// 判断位置是否存在某个区域
        /// </summary>
        private (Rect, (Rect, Size)) GetPosRect(Point pos)
        {
            foreach (var i in GetAllRelativeRects())
            {
                var rec = i.Item1.Contains(pos);
                if (rec) return i;
            }
            return (new Rect(0, 0, RenderSize.Width, RenderSize.Height), (new Rect(0, 0, RenderSize.Width, RenderSize.Height), RenderSize));
        }
        private void DrawingSplitImages(DrawingContext drawing)
        {
            var pen = GetDrawingPen();
            var mousePos = Mouse.GetPosition(this);
            var nowRec = GetPosRect(mousePos).Item1;
            double xySplitRange = 30;
            //如果是横向模式
            if (mousePos.X - xySplitRange <= nowRec.X || mousePos.X + xySplitRange >= nowRec.Right)
            {
                LastDrawingLine = DrawingTestLine(0);
            }
            else
            {
                LastDrawingLine = DrawingTestLine(1);
            }

            //direction 0横线 1竖线
            (Point, Point) DrawingTestLine(int direction = 0)
            {
                Point start = new Point(), end = new Point();
                switch (direction)
                {
                    case 0:
                        {
                            start.Y = mousePos.Y;
                            start.X = nowRec.X;
                            end.Y = mousePos.Y;
                            end.X = nowRec.Right;
                        }; break;
                    case 1:
                        {
                            start.X = mousePos.X;
                            start.Y = nowRec.Y;
                            end.X = mousePos.X;
                            end.Y = nowRec.Bottom;
                        }; break;
                }
                drawing.DrawLine(pen, start, end);
                return (start, end);
            }
        }

    }
}
