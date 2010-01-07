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

            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
        }

        void _graphs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (SerialGraph graph in e.OldItems)
                {
                    graph.ValueMemberPathChanged -= OnGraphValueMemberPathChanged;
                    RemoveGraphFromCanvas(graph);
                }
            }

            if (e.NewItems != null)
            {
                foreach (SerialGraph graph in e.NewItems)
                {
                    graph.ValueMemberPathChanged += new EventHandler<DataPathEventArgs>(OnGraphValueMemberPathChanged);
                    AddGraphToCanvas(graph);
                }
            }
        }

        private void AddGraphToCanvas(SerialGraph graph)
        {
            if (_graphCanvas != null && !_graphCanvas.Children.Contains(graph))
                _graphCanvas.Children.Add(graph);
        }

        private void RemoveGraphFromCanvas(SerialGraph graph)
        {
            if (_graphCanvas != null && _graphCanvas.Children.Contains(graph))
                _graphCanvas.Children.Remove(graph);
        }

        private void AddGraphsToCanvas()
        {
            foreach (SerialGraph graph in _graphs)
                AddGraphToCanvas(graph);
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


        public override void OnApplyTemplate()
        {
            _graphCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_GraphCanvasDecorator", this);
            _graphCanvasDecorator.SizeChanged += new SizeChangedEventHandler(_graphCanvasDecorator_SizeChanged);

            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
            AddGraphsToCanvas();
        }

        void _graphCanvasDecorator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _plotAreaInnerSize = new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight);
            SetPointLocations();
        }

        private Border _graphCanvasDecorator;
        private Canvas _graphCanvas;

        private Size _plotAreaInnerSize;

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

        private double _minimumValue;
        private double _maximumValue;
        private double _adjustedMinimumValue;
        private double _adjustedMaximumValue;

        private double _valueGridStep;

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
            InvalidateMinMax();
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

        private void SetMinMax()
        {
            if (_values.Count > 0)
            {
                _minimumValue = (from vs in _values.Values
                                 select vs.Min()).Min();
                _maximumValue = (from vs in _values.Values
                                 select vs.Max()).Max();

                AdjustMinMax(5); // TODO: add logic to set grid count automatically based on chart size

                SetPointLocations();

                RenderGraphs();
            }
        }

        private void AdjustMinMax(int desiredGridCount)
        {
            // TODO: refactor into something more comprehensible
            double min = _minimumValue;
            double max = _maximumValue;

            if (min == 0 && max == 0)
            {
                max = 9;
            }

            if (min > max)
            {
                min = max - 1;
            }

            // "beautify" min/max
            double initial_min = min;
            double initial_max = max;

            double dif = max - min;
            double dif_e;

            if (dif == 0)
            {
                // difference is 0 if all values of the period are equal
                // then difference will be
                dif_e = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(max)) * Math.Log10(Math.E))) / 10;
            }
            else
            {
                dif_e = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(dif)) * Math.Log10(Math.E))) / 10;
            }

            // new min and max
            max = Math.Ceiling(max / dif_e) * dif_e + dif_e;
            min = Math.Floor(min / dif_e) * dif_e - dif_e;

            // new difference
            dif = max - min;
            dif_e = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(dif)) * Math.Log10(Math.E))) / 10;

            // aprox size of the step
            double step = Math.Ceiling((dif / desiredGridCount) / dif_e) * dif_e;
            double step_e = Math.Pow(10, Math.Floor(Math.Log(Math.Abs(step)) * Math.Log10(Math.E)));

            double temp = Math.Ceiling(step / step_e);	//number from 1 to 10

            if (temp > 5)
                temp = 10;

            if (temp <= 5 && temp > 2)
                temp = 5;

            //real step
            step = Math.Ceiling(step / (step_e * temp)) * step_e * temp;

            min = step * Math.Floor(min / step); //final max
            max = step * Math.Ceiling(max / step); //final min

            if (min < 0 && initial_min >= 0)
            { //min is zero if initial min > 0
                min = 0;
            }

            if (max > 0 && initial_max <= 0)
            { //min is zero if initial min > 0
                max = 0;
            }

            _valueGridStep = step;

            _adjustedMinimumValue = min;
            _adjustedMaximumValue = max;
        }


        private void InvalidateMinMax()
        {
            SetMinMax();
        }

        private Point GetPointCoordinates(int index, double value)
        {
            return new Point(GetXCoordinate(index), GetYCoordinate(value));
        }

        private double GetXCoordinate(int index)
        {
            int count = _values[_values.Keys.First()].Count;

            return (_plotAreaInnerSize.Width / (count - 1)) * index;
        }

        private double GetYCoordinate(double value)
        {
            return _plotAreaInnerSize.Height - _plotAreaInnerSize.Height * ((value - _adjustedMinimumValue) / (_adjustedMaximumValue - _adjustedMinimumValue));
        }

        private void SetPointLocations()
        {
            _locations.Clear();

            if (_values.Count > 0)
            {
                var paths = GetDistinctPaths();

                foreach (string path in paths)
                {
                    _locations.Add(path, new PointCollection());
                    for (int i = 0; i < _values[path].Count; i++)
                    {
                        _locations[path].Add(GetPointCoordinates(i, _values[path][i]));
                    }
                }

                foreach (SerialGraph graph in this._graphs)
                {
                    graph.SetPointLocations(_locations[graph.ValueMemberPath]);
                }
            }
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            RenderGraphs();
        }

        private void RenderGraphs()
        {
            foreach (SerialGraph graph in this._graphs)
            {
                graph.Render();
            }
        }

    }
}
