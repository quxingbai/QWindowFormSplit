using QWindowFormSplit.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QWindowFormSplit.Controls
{
    /// <summary>
    /// ScreensPanel.xaml 的交互逻辑
    /// </summary>
    public partial class ScreensPanel : UserControl
    {
        private class ScreenDisplayItem : INotifyPropertyChanged
        {
            public double WidthPercentage { get; set; }
            public double HeightPercentage { get; set; }
            public double RealWidth => Data.Width;
            public double RealHeight => Data.Height;
            public String ScreenName => Data.Name;
            public BitmapImage BackgroundImage { get; set; }
            public ScreenProvider.ScreenData Data { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            public ScreenDisplayItem(ScreenProvider.ScreenData data)
            {
                this.Data = data;
            }
            public void Update()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WidthPercentage"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("HeightPercentage"));
                Data.Update();
            }
        }
        private ObservableCollection<ScreenDisplayItem> ScreenItems = new ObservableCollection<ScreenDisplayItem>();
        private static BitmapImage DesktopImage = null;

        public Func<ScreensPanel, IntPtr, bool> CanSetWindowPos;

        public ScreensPanel()
        {
            InitializeComponent();
            this.LIST.ItemsSource = ScreenItems;
            Reload();
        }
        public void Reload()
        {
            ScreenProvider.Instance.ReloadScreens();
            var ist = ScreenProvider.Instance;
            ScreenItems.Clear();
            foreach (var i in ScreenProvider.Instance.GetAllScreen())
            {
                var item = new ScreenDisplayItem(i);
                item.WidthPercentage = (i.Width * 1.0 / ist.ScreenTotalWidth) * 100;
                item.HeightPercentage = (i.Height * 1.0 / ist.ScreenTotalHeight) * 100;
                item.BackgroundImage = GetDesktopImage();
                ScreenItems.Add(item);
            }
        }

        private void ListBoxItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || e.ChangedButton != MouseButton.Left) return;
            ListBoxItem item = (sender as ListBoxItem);
            ListBox box = item.Parent as ListBox;
            ScreenDisplayItem data = item.DataContext as ScreenDisplayItem;
            var relation = DrawingRecPanel.GetDrawRelation(item);
            double wp = relation.Item1.Width / relation.Item2.Width, hp = relation.Item1.Height / relation.Item2.Height, xp = relation.Item1.X / relation.Item2.Width, yp = relation.Item1.Y / relation.Item2.Height;
            //double wp = item.RenderSize.Width / box.RenderSize.Width, hp = item.RenderSize.Height / box.RenderSize.Height;
            double realW = data.RealWidth * wp, realH = data.RealHeight * hp, realX = data.RealWidth * xp, realY = data.RealHeight * yp;
            Task.Run(() =>
            {
                IntPtr selectedWindow = IntPtr.Zero;
                while (true)
                {
                    var mousePos = System.Windows.Forms.Cursor.Position;
                    var windHandler = WindowFromPoint(mousePos);
                    selectedWindow = windHandler;
                    bool canNext = true;
                    Dispatcher.Invoke(() =>
                    {
                        canNext = Mouse.LeftButton == MouseButtonState.Pressed;
                    });
                    if (!canNext) break;
                    Thread.Sleep(100);
                }
                if (selectedWindow != IntPtr.Zero)
                {
                    IntPtr prt = selectedWindow;
                    while (prt != IntPtr.Zero)
                    {
                        prt = GetParent(prt);
                        if (prt != IntPtr.Zero) selectedWindow = prt;
                    }
                    if (CanSetWindowPos?.Invoke(this,prt)??true)
                    {
                        SetWindowPos(selectedWindow, IntPtr.Zero, (int)realX, (int)realY, (int)realW, (int)realH, 0x004);
                    }
                }
            });
        }

        public BitmapImage GetDesktopImage()
        {
            if (DesktopImage != null) return DesktopImage;
            StringBuilder s = new StringBuilder(300);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, 300, s, 0);
            string wallpaper_path = s.ToString(); //系统桌面背景图片路径
            BitmapImage img = new BitmapImage();
            MemoryStream ms = new MemoryStream();
            ms.Write(File.ReadAllBytes(wallpaper_path));
            ms.Position = 0;
            img.BeginInit();
            img.StreamSource = ms;
            img.EndInit();
            return (DesktopImage = img);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(System.Drawing.Point pt);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        public static extern int SystemParametersInfo(int uAction, int uParam, StringBuilder lpvParam, int fuWinIni);

        private const int SPI_GETDESKWALLPAPER = 0x0073;
        //public void AddScreen(ScreenProvider.ScreenData item)
        //{
        //    ScreenItems.Add(item);
        //}
    }
}
