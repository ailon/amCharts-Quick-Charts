using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    public class SerialChart : Control
    {
        public SerialChart()
        {
            this.DefaultStyleKey = typeof(SerialChart);

            this._graphs.CollectionChanged += new NotifyCollectionChangedEventHandler(_graphs_CollectionChanged);
        }

        void _graphs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SerialGraph graph in e.OldItems)
                {
                    graph.ValueMemberPathChanged -= OnGraphValueMemberPathChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (SerialGraph graph in e.NewItems)
                {
                    graph.ValueMemberPathChanged += new EventHandler<DataPathEventArgs>(OnGraphValueMemberPathChanged);
                }
            }
        }

        void OnGraphValueMemberPathChanged(object sender, DataPathEventArgs e)
        {
        }

        private DiscreetClearObservableCollection<SerialGraph> _graphs = new DiscreetClearObservableCollection<SerialGraph>();
        public DiscreetClearObservableCollection<SerialGraph> Graphs
        {
            get { return _graphs; }
            set { throw new System.NotSupportedException("Setting Graphs collection is not supported"); }
        }

        public CategoryAxis CategoryAxis
        {
            get;
            set;
        }

        public ValueAxis ValueAxis
        {
            get;
            set;
        }


        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource", typeof(IEnumerable), typeof(SerialChart), 
            new PropertyMetadata(null, new PropertyChangedCallback(SerialChart.DataSourceProperty_Changed)));

        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        private static void DataSourceProperty_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SerialChart chart = d as SerialChart;
            DetachOldDataSourceCollectionChangedListener(chart, e.OldValue);
            AttachDataSourceCollectionChangedListener(chart, e.NewValue);
            chart.ProcessData();
        }

        private static void DetachOldDataSourceCollectionChangedListener(SerialChart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged)
            {
                (dataSource as INotifyCollectionChanged).CollectionChanged -= chart.DataSource_CollectionChanged;
            }
        }

        private static void AttachDataSourceCollectionChangedListener(SerialChart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged)
            {
                (dataSource as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(chart.DataSource_CollectionChanged);
            }
        }

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: implement intelligent mechanism to hanlde multiple changes in one batch
            ProcessData();
        }

        public string CategoryValuePath
        {
            get;
            set;
        }

        private Dictionary<string, List<double>> _values = new Dictionary<string, List<double>>();
        private Dictionary<string, PointCollection> _locations = new Dictionary<string, PointCollection>();

        private double MinimumValue { get; set; }
        private double MaximumValue { get; set; }

        private void ProcessData()
        {
            if (this.DataSource != null)
            {
                var paths = GetDistinctPaths();

                Dictionary<string, BindingEvaluator> bindingEvaluators = CreateBindingEvaluators(paths);
                ResetValues(paths);

                // add data items
                foreach (object dataItem in this.DataSource)
                {
                    AddSingleStepValues(paths, bindingEvaluators, dataItem);
                }
            }
            else
            {
                _values.Clear();
            }
        }

        private void AddSingleStepValues(IEnumerable<string> paths, Dictionary<string, BindingEvaluator> bindingEvaluators, object dataItem)
        {
            foreach (string path in paths)
            {
                _values[path].Add((double)bindingEvaluators[path].Eval(dataItem));
            }
        }

        private IEnumerable<string> GetDistinctPaths()
        {
            // get all distinct ValueMemberPath properties set on graphs
            var paths = (from g in this.Graphs
                         select g.ValueMemberPath).Distinct();
            return paths;
        }

        private Dictionary<string, BindingEvaluator> CreateBindingEvaluators(IEnumerable<string> paths)
        {
            Dictionary<string, BindingEvaluator> bindingEvaluators = new Dictionary<string, BindingEvaluator>();
            foreach (string path in paths)
            {
                bindingEvaluators.Add(path, new BindingEvaluator(path));
            }
            return bindingEvaluators;
        }

        private void ResetValues(IEnumerable<string> paths)
        {
            this._values.Clear();
            foreach (string path in paths)
            {
                _values.Add(path, new List<double>());
            }
        }
    }
}
