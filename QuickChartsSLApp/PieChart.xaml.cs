using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace QuickChartsSLApp
{
    public partial class PieChart : UserControl
    {
        public PieChart()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ObservableCollection<PieDataItem> data = new ObservableCollection<PieDataItem>()
            {
                new PieDataItem() { Title = "s1", Value = 10 },
                new PieDataItem() { Title = "s2", Value = 30 },
                new PieDataItem() { Title = "s3", Value = 20 },
                new PieDataItem() { Title = "s4", Value = 80 }
            };

            pie1.DataSource = data;
        }
    }

    public class PieDataItem
    {
        public string Title { get; set; }
        public double Value { get; set; }
    }
}
