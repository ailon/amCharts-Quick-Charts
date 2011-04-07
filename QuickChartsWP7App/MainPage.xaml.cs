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
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace QuickChartsWPApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private ObservableCollection<TestDataItem> _data = new ObservableCollection<TestDataItem>()
        {
            new TestDataItem() { cat1 = "cat1", val1=5, val2=15, val3=12},
            new TestDataItem() { cat1 = "cat2", val1=15.2, val2=1.5, val3=2.1M},
            new TestDataItem() { cat1 = "cat3", val1=25, val2=5, val3=2},
            new TestDataItem() { cat1 = "cat4", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat5", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat6", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat7", val1=4.1, val2=4, val3=2},
            new TestDataItem() { cat1 = "cat8", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat9", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat10", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat11", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat12", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat13", val1=4.1, val2=4, val3=2},
            new TestDataItem() { cat1 = "cat14", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat15", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat16", val1=8.1, val2=1, val3=22},
            new TestDataItem() { cat1 = "cat17", val1=8.1, val2=1, val3=22},
        };

        public ObservableCollection<TestDataItem> Data { get { return _data; } }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _data.Clear();
            _data.Add(new TestDataItem() { cat1 = "cat1", val1 = 5, val2 = 15, val3 = 12 });
            _data.Add(new TestDataItem() { cat1 = "cat2", val1=15.2, val2=1.5, val3=2.1M});
            _data.Add(new TestDataItem() { cat1 = "cat3", val1=25, val2=5, val3=2});
            _data.Add(new TestDataItem() { cat1 = "cat4", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat5", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat6", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat7", val1=4.1, val2=4, val3=2});
            _data.Add(new TestDataItem() { cat1 = "cat8", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat9", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat10", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat11", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat12", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat13", val1=4.1, val2=4, val3=2});
            _data.Add(new TestDataItem() { cat1 = "cat14", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat15", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat16", val1=8.1, val2=1, val3=22});
            _data.Add(new TestDataItem() { cat1 = "cat17", val1=8.1, val2=1, val3=22});
        }
    }

    public class TestDataItem
    {
        public string cat1 { get; set; }
        public double val1 { get; set; }
        public double val2 { get; set; }
        public decimal val3 { get; set; }
    }
}
