using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

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
                    RemoveIndicator(graph);
                }
            }

            if (e.NewItems != null)
            {
                foreach (SerialGraph graph in e.NewItems)
                {
                    graph.ValueMemberPathChanged += new EventHandler<DataPathEventArgs>(OnGraphValueMemberPathChanged);
                    if (graph.Brush == null && PresetBrushes.Count > 0)
                    {
                        graph.Brush = PresetBrushes[_graphs.IndexOf(graph) % PresetBrushes.Count];
                    }
                    AddGraphToCanvas(graph);
                    AddIndicator(graph);
                }
            }
        }

        private void AddIndicator(SerialGraph graph)
        {
            Indicator indicator = new Indicator();
            _indicators.Add(graph, indicator);
            AddIndicatorToCanvas(indicator);
        }

        private void AddIndicatorToCanvas(Indicator indicator)
        {
            if (_graphCanvas != null)
            {
                _graphCanvas.Children.Add(indicator);
            }
        }

        private void RemoveIndicator(SerialGraph graph)
        {
            if (_graphCanvas != null)
            {
                _graphCanvas.Children.Remove(_indicators[graph]);
            }
            _indicators.Remove(graph);
        }

        private void AddIndicatorsToCanvas()
        {
            foreach (Indicator indicator in _indicators.Values)
            {
                AddIndicatorToCanvas(indicator);
            }
        }

        private void AddGraphToCanvas(SerialGraph graph)
        {
            if (_graphCanvas != null && !_graphCanvas.Children.Contains(graph))
            {
                _graphCanvas.Children.Add(graph);
            }
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

        private Dictionary<SerialGraph, Indicator> _indicators = new Dictionary<SerialGraph, Indicator>();

        public override void OnApplyTemplate()
        {
            _graphCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_GraphCanvasDecorator", this);
            _graphCanvasDecorator.SizeChanged += new SizeChangedEventHandler(_graphCanvasDecorator_SizeChanged);
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
            AddGraphsToCanvas();
            AddIndicatorsToCanvas();

            _graphCanvas.MouseEnter += new MouseEventHandler(_graphCanvas_MouseEnter);
            _graphCanvas.MouseMove += new System.Windows.Input.MouseEventHandler(_graphCanvas_MouseMove);
            _graphCanvas.MouseLeave += new MouseEventHandler(_graphCanvas_MouseLeave);

            _valueAxis = (ValueAxis)TreeHelper.TemplateFindName("PART_ValueAxis", this);
            _valueGrid = (ValueGrid)TreeHelper.TemplateFindName("PART_ValueGrid", this);

            _categoryAxis = (CategoryAxis)TreeHelper.TemplateFindName("PART_CategoryAxis", this);

            _legend = (Legend)TreeHelper.TemplateFindName("PART_Legend", this);
            _legend.LegendItemsSource = this.Graphs.Cast<ILegendItem>(); // TODO: handle changes in Graphs
        }

        void _graphCanvas_MouseEnter(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(_graphCanvas);
            PositionIndicators(position);
        }

        void _graphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(_graphCanvas);
            PositionIndicators(position);
            SetToolTips(position);
        }

        void _graphCanvas_MouseLeave(object sender, MouseEventArgs e)
        {
            HideIndicators();
        }

        private void SetToolTips(Point position)
        {
            int index = GetIndexByCoordinate(position.X);
            for (int i = 0; i < _graphs.Count; i++)
            {
                string tooltipContent = _graphs[i].Title + ": " + _categoryValues[index] + " / " + _values[_graphs[i].ValueMemberPath][index].ToString();
                ToolTipService.SetToolTip(_indicators[_graphs[i]], tooltipContent);
                ToolTipService.SetToolTip(_graphs[i], tooltipContent);
            }
        }

        private void PositionIndicators(Point position)
        {
            int index = GetIndexByCoordinate(position.X);
            foreach (SerialGraph graph in _graphs)
            {
                _indicators[graph].SetPosition(_locations[graph.ValueMemberPath][index]);
                _indicators[graph].Visibility = Visibility.Visible;
            }
        }

        private void HideIndicators()
        {
            foreach (SerialGraph graph in _graphs)
            {
                _indicators[graph].Visibility = Visibility.Collapsed;
            }
        }

        void _graphCanvasDecorator_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _plotAreaInnerSize = new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight);
            SetLocations();
        }

        private Border _graphCanvasDecorator;
        private Canvas _graphCanvas;

        private Size _plotAreaInnerSize;

        private CategoryAxis _categoryAxis;
        private ValueAxis _valueAxis;
        private ValueGrid _valueGrid;

        private Legend _legend;

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

        public static readonly DependencyProperty CategoryValuePathProperty = DependencyProperty.Register(
            "CategoryValuePath", typeof(string), typeof(SerialChart),
            new PropertyMetadata(null)
            );

        public string CategoryValuePath
        {
            get { return (string)GetValue(CategoryValuePathProperty); }
            set { SetValue(CategoryValuePathProperty, value); }
        }

        private Dictionary<string, List<double>> _values = new Dictionary<string, List<double>>();
        private Dictionary<string, PointCollection> _locations = new Dictionary<string, PointCollection>();
        private List<string> _categoryValues = new List<string>();
        private List<double> _categoryLocations = new List<double>();

        private double _minimumValue;
        private double _maximumValue;
        private double _adjustedMinimumValue;
        private double _adjustedMaximumValue;
        private double _groundValue; // 0 or closest to 0

        private double _valueGridStep;
        private List<double> _valueGridValues = new List<double>();
        private List<double> _valueGridLocations = new List<double>();

        private List<string> _categoryGridValues = new List<string>();
        private List<double> _categoryGridLocations = new List<double>();

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

                ProcessCategoryData();
            }
            else
            {
                _values.Clear();
            }
            InvalidateMinMax();
        }

        private void ProcessCategoryData()
        {
            _categoryValues.Clear();
            if (this.DataSource != null && !string.IsNullOrEmpty(CategoryValuePath))
            {
                BindingEvaluator eval = new BindingEvaluator(CategoryValuePath);
                foreach (object dataItem in this.DataSource)
                {
                    _categoryValues.Add(eval.Eval(dataItem).ToString());
                }
            }
            else
            {
                _categoryValues.Clear();
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

        private void SetMinMax()
        {
            if (_values.Count > 0)
            {
                _minimumValue = (from vs in _values.Values
                                 select vs.Min()).Min();
                _maximumValue = (from vs in _values.Values
                                 select vs.Max()).Max();

                AdjustMinMax(5); // TODO: add logic to set grid count automatically based on chart size

                SetValueGridValues();

                SetLocations();

                RenderGraphs();
            }
        }

        private void SetLocations()
        {
            if (_valueAxis != null && _valueGrid != null && _categoryAxis != null)
            {
                // in Silverlight event sequence is different and SetValueGridValues() is called too early for the first time
                if (_valueGridValues.Count == 0)
                {
                    SetValueGridValues();
                }

                SetPointLocations();
                SetValueGridLocations();
                _valueAxis.SetLocations(_valueGridLocations);
                _valueGrid.SetLocations(_valueGridLocations);

                SetCategoryGridLocations();
                _categoryAxis.SetValues(_categoryGridValues);
                _categoryAxis.SetLocations(_categoryGridLocations);
            }
        }

        private void SetCategoryGridLocations()
        {
            int gridCountHint = 5; // TODO: intelligent algorithm
            int gridCount = GetCategoryGridCount(gridCountHint);
            if (gridCount != 0)
            {
                int gridStep = _categoryValues.Count / gridCount;

                _categoryGridValues.Clear();
                _categoryGridLocations.Clear();

                if (gridStep > 0)
                {
                    for (int i = 0; i < _categoryValues.Count; i += gridStep)
                    {
                        _categoryGridValues.Add(_categoryValues[i]);
                        _categoryGridLocations.Add(_categoryLocations[i]);
                    }
                }
            }
        }

        private int GetCategoryGridCount(int gridCountHint)
        {
            int gridCount = gridCountHint;
            if (gridCountHint >= _categoryValues.Count)
            {
                gridCount = _categoryValues.Count;
            }
            else
            {
                int hint = gridCountHint;
                while ((_categoryValues.Count - 1) % hint != 0 && hint > 1)
                    hint--;

                if (hint == 1)
                {
                    hint = gridCountHint;
                    while ((_categoryValues.Count - 1) % hint != 0 && hint < Math.Min(_categoryValues.Count, gridCountHint * 2))
                        hint++;
                }

                if (hint < gridCountHint * 2)
                    gridCount = hint;
            }
            return gridCount;
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

            // ground value (starting point for column an similar graphs)
            _groundValue = (min <= 0 && 0 <= max) ? 0 : (max > 0 ? min : max);
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
            double step = _plotAreaInnerSize.Width / count;

            return step * index + step / 2;
        }

        private double GetYCoordinate(double value)
        {
            return _plotAreaInnerSize.Height - _plotAreaInnerSize.Height * ((value - _adjustedMinimumValue) / (_adjustedMaximumValue - _adjustedMinimumValue));
        }

        private int GetIndexByCoordinate(double x)
        {
            int count = _values[_values.Keys.First()].Count;
            double step = _plotAreaInnerSize.Width / count;

            int index = (int)Math.Round((x - step / 2) / step);
            index = Math.Max(0, index);
            index = Math.Min(count - 1, index);

            return index;
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
                    graph.SetPointLocations(_locations[graph.ValueMemberPath], GetYCoordinate(_groundValue));
                }
            }

            SetCategoryLocations();
        }

        private void SetCategoryLocations()
        {
            _categoryLocations.Clear();

            for (int i = 0; i < _categoryValues.Count; i++)
            {
                _categoryLocations.Add(GetXCoordinate(i));
            }
        }

        private void SetValueGridValues()
        {
            if (_valueAxis != null && _valueGridStep > 0)
            {
                _valueGridValues.Clear();
                for (double d = _adjustedMinimumValue; d <= _adjustedMaximumValue; d += _valueGridStep)
                {
                    _valueGridValues.Add(d);
                }
                _valueAxis.SetValues(_valueGridValues);
            }
        }

        private void SetValueGridLocations()
        {
            _valueGridLocations.Clear();
            foreach (double value in _valueGridValues)
            {
                _valueGridLocations.Add(GetYCoordinate(value));
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


        private List<Brush> _presetBrushes = new List<Brush>()
        {
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x66, 0x00)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFC, 0xD2, 0x02)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xB0, 0xDE, 0x09)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x0D, 0x8E, 0xCF)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x2A, 0x0C, 0xD0)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xCD, 0x0D, 0x74)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0x00, 0x00)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xCC, 0x00)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0xCC)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0x33)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x00, 0x00))
        };

        public List<Brush> PresetBrushes
        {
            get { return _presetBrushes; }
        }
    }
}
