using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QuickCharts
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private ObservableCollection<TestDataItem> _data = new ObservableCollection<TestDataItem>()
        {
            new TestDataItem() { cat1 = "cat1", val1=5, val2=15, val3=12},
            new TestDataItem() { cat1 = "cat2", val1=15.2, val2=1.5, val3=2.1},
            new TestDataItem() { cat1 = "cat3", val1=25, val2=5, val3=2},
            new TestDataItem() { cat1 = "cat4", val1=8.1, val2=1, val3=22},
        };

        public ObservableCollection<TestDataItem> Data { get { return _data; } }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _data.Insert(2, new TestDataItem() { cat1 = "cat2.2", val1 = 1, val2 = 2, val3 = 3 });
            _data.Insert(4, new TestDataItem() { cat1 = "cat3.2", val1 = 31, val2 = 32, val3 = 33 });
            _data.Add(new TestDataItem() { cat1 = "cat_new", val1 = 12, val2 = 22, val3 = 32 });
        }
    }

    public class TestDataItem
    {
        public string cat1 { get; set; }
        public double val1 { get; set; }
        public double val2 { get; set; }
        public double val3 { get; set; }
    }
}
