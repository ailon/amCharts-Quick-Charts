using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace QuickCharts
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart : Window
    {
        public PieChart()
        {
            InitializeComponent();
        }
        ObservableCollection<PieDataItem> data = new ObservableCollection<PieDataItem>()
            {
                new PieDataItem() { Title = "s1", Value = 10 },
                //new PieDataItem() { Title = "s2", Value = 30 },
                //new PieDataItem() { Title = "s3", Value = 20 },
                //new PieDataItem() { Title = "s4", Value = 80 }
            };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pie1.DataSource = data;
        }

        private void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            data.Add(new PieDataItem() { Title = "s5", Value = 12.56 });
            data.Add(new PieDataItem() { Title = "s6", Value = 25 });
        }

        private void RealTimeDataChanges_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        Random rnd = new Random();
        int sliceCounter = 10;
        void timer_Tick(object sender, EventArgs e)
        {
            double action = rnd.NextDouble();
            if (action > 0.85 && data.Count > 0)
            {
                data.RemoveAt(rnd.Next(data.Count));
            }
            else if (action > 0.8)
            {
                data.Add(new PieDataItem() { Title = "slice " + sliceCounter.ToString(), Value = rnd.NextDouble() * 50 });
                sliceCounter++;
            }
            else
            {
                foreach (PieDataItem di in data)
                {
                    di.Value = rnd.NextDouble() * 50;
                }
            }
        }
    }

    public class PieDataItem : INotifyPropertyChanged
    {
        public string Title { get; set; }


        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
