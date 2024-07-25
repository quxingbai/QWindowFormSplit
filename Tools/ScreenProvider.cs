using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace QWindowFormSplit.Tools
{
    public class ScreenProvider
    {
        public class ScreenData : INotifyPropertyChanged
        {
            public System.Windows.Forms.Screen Screen { get; set; }
            public ScreenData(System.Windows.Forms.Screen ScreenSource, int Index)
            {
                this.Screen = ScreenSource;
                this.Index = Index;
            }
            public double Width => Screen.Bounds.Width;
            public double Height => Screen.Bounds.Height;
            public double Index { get; set; }
            public string Name => Screen.DeviceName;
            public event PropertyChangedEventHandler? PropertyChanged;
            public void Update()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Width"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Height"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Index"));
            }
        }


        public static readonly ScreenProvider Instance = new ScreenProvider();

        private Dictionary<int, ScreenData> Screens = new Dictionary<int, ScreenData>();
        private Lazy<double> TotalScreenWidth = null;
        private Lazy<double> TotalScreenHeight = null;
        public ScreenProvider()
        {
            ReloadScreens();
        }
        public void ReloadScreens()
        {
            TotalScreenWidth = new Lazy<double>(() => Screens.Values.Sum(s => s.Width));
            TotalScreenHeight = new Lazy<double>(() => Screens.Values.Sum(s => s.Height));
            Screens.Clear();
            int count = 0;
            foreach (var i in System.Windows.Forms.Screen.AllScreens)
            {
                Screens.Add(count, new ScreenData(i, count));
                count++;
            }
        }
        public ScreenData GetScreen(int count=0)
        {
            return Screens[count];
        }
        public IEnumerable<ScreenData> GetAllScreen() => Screens.Values;
        public double ScreenTotalWidth => TotalScreenWidth.Value;
        public double ScreenTotalHeight => TotalScreenHeight.Value;
    }
}
