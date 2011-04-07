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
using System.Windows.Data;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Displays serial charts (line, column, etc.).
    /// </summary>
    public class SerialChart : Control
    {
        /// <summary>
        /// Instantiates SerialChart.
        /// </summary>
        public SerialChart()
        {
            this.DefaultStyleKey = typeof(SerialChart);

            this._graphs.CollectionChanged += new NotifyCollectionChangedEventHandler(OnGraphsCollectionChanged);

            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);

            Padding = new Thickness(10);
        }

        void OnGraphsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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

            Binding fillBinding = new Binding("Brush");
            fillBinding.Source = graph;
            indicator.SetBinding(Indicator.FillProperty, fillBinding);

            Binding strokeBinding = new Binding("PlotAreaBackground");
            strokeBinding.Source = this;
            indicator.SetBinding(Indicator.StrokeProperty, strokeBinding);

#if WINDOWS_PHONE
            indicator.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnIndicatorManipulationStarted);
#else
            indicator.MouseEnter += new MouseEventHandler(OnIndicatorMouseEnter);
            indicator.MouseLeave += new MouseEventHandler(OnIndicatorMouseLeave);
#endif

            _indicators.Add(graph, indicator);
            AddIndicatorToCanvas(indicator);
        }

#if WINDOWS_PHONE
        void OnIndicatorManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            DisplayBalloon((Indicator)sender);
            e.Handled = true;
        }
#else
        void OnIndicatorMouseEnter(object sender, MouseEventArgs e)
        {
            DisplayBalloon((Indicator)sender);
        }

        void OnIndicatorMouseLeave(object sender, MouseEventArgs e)
        {
            _balloon.Visibility = Visibility.Collapsed;
        }
#endif

        private void DisplayBalloon(Indicator indicator)
        {
            _balloon.Text = indicator.Text;
            _balloon.Visibility = Visibility.Visible;
            _balloon.Measure(new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight));
            double balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty) - _balloon.DesiredSize.Width / 2 + indicator.ActualWidth / 2;
            if (balloonLeft < 0)
            {
                balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty);
            }
            else if (balloonLeft + _balloon.DesiredSize.Width > _graphCanvasDecorator.ActualWidth)
            {
                balloonLeft = (double)indicator.GetValue(Canvas.LeftProperty) - _balloon.DesiredSize.Width;
            }
            double balloonTop = (double)indicator.GetValue(Canvas.TopProperty) - _balloon.DesiredSize.Height - 5;
            if (balloonTop < 0)
            {
                balloonTop = (double)indicator.GetValue(Canvas.TopProperty) + 17;
            }
            _balloon.SetValue(Canvas.LeftProperty, balloonLeft);
            _balloon.SetValue(Canvas.TopProperty, balloonTop);
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
        /// <summary>
        /// Gets collection of <see cref="SerialGraph"/> objects representing graphs for this chart.
        /// </summary>
        public DiscreetClearObservableCollection<SerialGraph> Graphs
        {
            get { return _graphs; }
            set { throw new System.NotSupportedException("Setting Graphs collection is not supported"); }
        }

        private Dictionary<SerialGraph, Indicator> _indicators = new Dictionary<SerialGraph, Indicator>();

        /// <summary>
        /// Assigns template parts
        /// </summary>
        public override void OnApplyTemplate()
        {
            AssignGraphCanvas();

            AddGraphsToCanvas();
            AddIndicatorsToCanvas();

            AssignGridParts();

            AssignLegend();

            AssignBalloon();
        }

        private void AssignGraphCanvas()
        {
            _graphCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_GraphCanvasDecorator", this);
            _graphCanvasDecorator.SizeChanged += new SizeChangedEventHandler(OnGraphCanvasDecoratorSizeChanged);
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);

#if WINDOWS_PHONE
            _graphCanvas.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGraphCanvasManipulationStarted);
#else
            _graphCanvas.MouseEnter += new MouseEventHandler(OnGraphCanvasMouseEnter);
            _graphCanvas.MouseMove += new System.Windows.Input.MouseEventHandler(OnGraphCanvasMouseMove);
            _graphCanvas.MouseLeave += new MouseEventHandler(OnGraphCanvasMouseLeave);
#endif
        }


        private void AssignGridParts()
        {
            _valueAxis = (ValueAxis)TreeHelper.TemplateFindName("PART_ValueAxis", this);
            _valueGrid = (ValueGrid)TreeHelper.TemplateFindName("PART_ValueGrid", this);

            Binding formatBinding = new Binding("ValueFormatString");
            formatBinding.Source = this;
            _valueAxis.SetBinding(ValueAxis.ValueFormatStringProperty, formatBinding);

            _categoryAxis = (CategoryAxis)TreeHelper.TemplateFindName("PART_CategoryAxis", this);
#if WINDOWS_PHONE
            _categoryAxis.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
            _valueAxis.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
#endif
        }

#if WINDOWS_PHONE
        void OnGridManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_legend != null)
            {
                if (_legend.Visibility == Visibility.Collapsed)
                {
                    _legend.Visibility = Visibility.Visible;
                    _valueAxis.Visibility = Visibility.Visible;
                }
                else
                {
                    _legend.Visibility = Visibility.Collapsed;
                    _valueAxis.Visibility = Visibility.Collapsed;
                }
            }
            HideIndicators();
        }
#endif

        private void AssignLegend()
        {
            _legend = (Legend)TreeHelper.TemplateFindName("PART_Legend", this);
            _legend.LegendItemsSource = this.Graphs.Cast<ILegendItem>(); // TODO: handle changes in Graphs
#if WINDOWS_PHONE
            _legend.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnGridManipulationStarted);
#endif
        }

        private void AssignBalloon()
        {
            _balloon = (Balloon)TreeHelper.TemplateFindName("PART_Balloon", this);
        }

#if WINDOWS_PHONE
        void OnGraphCanvasManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (_balloon != null)
            {
                _balloon.Visibility = Visibility.Collapsed;
            }
            PositionIndicators(e.ManipulationOrigin);
            SetToolTips(e.ManipulationOrigin);
        }
#else
        void OnGraphCanvasMouseEnter(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(_graphCanvas);
            PositionIndicators(position);
        }

        void OnGraphCanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(_graphCanvas);
            PositionIndicators(position);
            SetToolTips(position);
        }

        void OnGraphCanvasMouseLeave(object sender, MouseEventArgs e)
        {
            HideIndicators();
        }
#endif

        private void SetToolTips(Point position)
        {
            int index = GetIndexByCoordinate(position.X);
            if (index > -1)
            {
                for (int i = 0; i < _graphs.Count; i++)
                {
                    string tooltipContent = _graphs[i].Title + ": " + _categoryValues[index] + " | "
                        + (string.IsNullOrEmpty(ValueFormatString) ? _values[_graphs[i].ValueMemberPath][index].ToString() : _values[_graphs[i].ValueMemberPath][index].ToString(ValueFormatString));
                    //ToolTipService.SetToolTip(_indicators[_graphs[i]], tooltipContent);
                    //ToolTipService.SetToolTip(_graphs[i], tooltipContent);
                    _indicators[_graphs[i]].Text = tooltipContent;
                }
            }
        }

        private void PositionIndicators(Point position)
        {
            int index = GetIndexByCoordinate(position.X);
            if (index > -1)
            {
                foreach (SerialGraph graph in _graphs)
                {
                    _indicators[graph].Visibility = Visibility.Visible;
                    _indicators[graph].SetPosition(_locations[graph.ValueMemberPath][index]);
                }
            }
            //_balloon.Visibility = Visibility.Collapsed;
        }

        private void HideIndicators()
        {
            foreach (SerialGraph graph in _graphs)
            {
                _indicators[graph].Visibility = Visibility.Collapsed;
            }
            _balloon.Visibility = Visibility.Collapsed;
        }

        void OnGraphCanvasDecoratorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            _plotAreaInnerSize = new Size(_graphCanvasDecorator.ActualWidth, _graphCanvasDecorator.ActualHeight);
            AdjustGridCount();
            SetLocations();
            HideIndicators();
        }

        private void AdjustGridCount()
        {
            AdjustValueGridCount();
            AdjustCategoryGridCount();
        }

        private void AdjustCategoryGridCount()
        {
            int oldCount = _categoryGridCount;
            _categoryGridCount = (int)(_plotAreaInnerSize.Width / (MinimumCategoryGridStep * 1.1));
            _categoryGridCount = Math.Max(1, _categoryGridCount);

            if (oldCount != _categoryGridCount)
            {
                if (_categoryGridCount > 1)
                {
                    _categoryAxis.Visibility = Visibility.Visible;
                }
                else
                {
                    _categoryAxis.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AdjustValueGridCount()
        {
            int oldCount = _desiredValueGridCount;
            _desiredValueGridCount = (int)(_plotAreaInnerSize.Height / (MinimumValueGridStep * 1.1));
            _desiredValueGridCount = Math.Max(1, _desiredValueGridCount);

            if (oldCount != _desiredValueGridCount)
            {
                if (_desiredValueGridCount > 1)
                {
                    _valueAxis.Visibility = Visibility.Visible;
                    _valueGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    _valueAxis.Visibility = Visibility.Collapsed;
                    _valueGrid.Visibility = Visibility.Collapsed;
                }
                SetMinMax();
            }
        }

        private Border _graphCanvasDecorator;
        private Canvas _graphCanvas;

        private Size _plotAreaInnerSize;

        private CategoryAxis _categoryAxis;
        private ValueAxis _valueAxis;
        private ValueGrid _valueGrid;

        private Legend _legend;

        private Balloon _balloon;

        /// <summary>
        /// Identifies <see cref="DataSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource", typeof(IEnumerable), typeof(SerialChart), 
            new PropertyMetadata(null, new PropertyChangedCallback(SerialChart.OnDataSourcePropertyChanged)));

        /// <summary>
        /// Gets or sets data source for the chart.
        /// This is a dependency property.
        /// </summary>
        public IEnumerable DataSource
        {
            get { return (IEnumerable)GetValue(DataSourceProperty); }
            set { SetValue(DataSourceProperty, value); }
        }

        private static void OnDataSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
                (dataSource as INotifyCollectionChanged).CollectionChanged -= chart.OnDataSourceCollectionChanged;
            }
        }

        private static void AttachDataSourceCollectionChangedListener(SerialChart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged)
            {
                (dataSource as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(chart.OnDataSourceCollectionChanged);
            }
        }

        private void OnDataSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // TODO: implement intelligent mechanism to hanlde multiple changes in one batch
            ProcessData();
        }

        /// <summary>
        /// Identifies <see cref="CategoryValueMemberPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CategoryValueMemberPathProperty = DependencyProperty.Register(
            "CategoryValueMemberPath", typeof(string), typeof(SerialChart),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets path to the property holding category values in data source.
        /// This is a dependency property.
        /// </summary>
        public string CategoryValueMemberPath
        {
            get { return (string)GetValue(CategoryValueMemberPathProperty); }
            set { SetValue(CategoryValueMemberPathProperty, value); }
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
            if (this.DataSource != null && !string.IsNullOrEmpty(CategoryValueMemberPath))
            {
                BindingEvaluator eval = new BindingEvaluator(CategoryValueMemberPath);
                foreach (object dataItem in this.DataSource)
                {
                    _categoryValues.Add(eval.Eval(dataItem).ToString());
                }
            }
        }

        private void AddSingleStepValues(IEnumerable<string> paths, Dictionary<string, BindingEvaluator> bindingEvaluators, object dataItem)
        {
            foreach (string path in paths)
            {
                _values[path].Add(Convert.ToDouble(bindingEvaluators[path].Eval(dataItem)));
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
                var minimumValues = from vs in _values.Values
                                    where vs.Count > 0
                                    select vs.Min();
                _minimumValue = minimumValues.Count() > 0 ? minimumValues.Min() : 0;
                var maximumValues = from vs in _values.Values
                                    where vs.Count > 0
                                    select vs.Max();
                _maximumValue = maximumValues.Count() > 0 ? maximumValues.Max() : 9;

                AdjustMinMax(_desiredValueGridCount);

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
            int gridCount = GetCategoryGridCount(_categoryGridCount);
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
            _valueGridCount = (int)((max - min) / step);

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
            int index = -1;

            if (_values.Count > 0)
            {
                int count = _values[_values.Keys.First()].Count;
                double step = _plotAreaInnerSize.Width / count;

                index = (int)Math.Round((x - step / 2) / step);
                index = Math.Max(0, index);
                index = Math.Min(count - 1, index);
            }

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
                for (double d = _adjustedMinimumValue + _valueGridStep; d <= _adjustedMaximumValue; d += _valueGridStep)
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

        /// <summary>
        /// Gets a collection of preset brushes used for graphs when their Brush property isn't set explicitly.
        /// </summary>
        public List<Brush> PresetBrushes
        {
            get { return _presetBrushes; }
        }


        private int _valueGridCount = 5;
        private int _desiredValueGridCount = 5;
        private int _categoryGridCount = 5;

        /// <summary>
        /// Identifies <see cref="MinimumValueGridStep"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumValueGridStepProperty = DependencyProperty.Register(
            "MinimumValueGridStep", typeof(double), typeof(SerialChart),
            new PropertyMetadata(30.0)
            );

        /// <summary>
        /// Gets or sets minimum size of a single step in value grid/value axis values.
        /// This is a dependency property.
        /// The default is 30.
        /// </summary>
        /// <remarks>
        /// When chart is resized and distance between grid lines becomes lower than value of MinimumValueGridStep
        /// chart decreases number of grid lines.
        /// </remarks>
        public double MinimumValueGridStep
        {
            get { return (double)GetValue(SerialChart.MinimumValueGridStepProperty); }
            set { SetValue(SerialChart.MinimumValueGridStepProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="MinimumCategoryGridStep"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumCategoryGridStepProperty = DependencyProperty.Register(
            "MinimumCategoryGridStep", typeof(double), typeof(SerialChart),
            new PropertyMetadata(70.0)
            );

        /// <summary>
        /// Gets or sets minimum distance between 2 value tick on category axis.
        /// This is a dependency property.
        /// The default is 70.
        /// </summary>
        /// <remarks>
        /// When chart is resized and distance between grid lines becomes lower than value of MinimumCategoryGridStep
        /// chart decreases number of grid lines.
        /// </remarks>
        public double MinimumCategoryGridStep
        {
            get { return (double)GetValue(SerialChart.MinimumCategoryGridStepProperty); }
            set { SetValue(SerialChart.MinimumCategoryGridStepProperty, value); }
        }


        //// PLOT AREA

        /// <summary>
        /// Identifies <see cref="PlotAreaBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PlotAreaBackgroundProperty = DependencyProperty.Register(
            "PlotAreaBackground", typeof(Brush), typeof(SerialChart),
            new PropertyMetadata(new SolidColorBrush(Colors.White))
            );

        /// <summary>
        /// Gets or sets a brush used as a background for plot area (the area inside of axes).
        /// This is a dependency property.
        /// The default is White.
        /// </summary>
        public Brush PlotAreaBackground
        {
            get { return (Brush)GetValue(SerialChart.PlotAreaBackgroundProperty); }
            set { SetValue(SerialChart.PlotAreaBackgroundProperty, value); }
        }

        //// LEGEND

        /// <summary>
        /// Identifies <see cref="LegendVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendVisibilityProperty = DependencyProperty.Register(
            "LegendVisibility", typeof(Visibility), typeof(SerialChart),
            new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets visibility of the chart legend.
        /// This is a dependency property.
        /// The default is Visible.
        /// </summary>
        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(SerialChart.LegendVisibilityProperty); }
            set { SetValue(SerialChart.LegendVisibilityProperty, value); }
        }


        /// <summary>
        /// Identifies <see cref="AxisForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AxisForegroundProperty = DependencyProperty.Register(
            "AxisForeground", typeof(Brush), typeof(SerialChart),
            new PropertyMetadata(new SolidColorBrush(Colors.Black))
            );

        /// <summary>
        /// Gets or sets foreground color of the axes.
        /// This is a dependency property.
        /// The default is Black.
        /// </summary>
        public Brush AxisForeground
        {
            get { return (Brush)GetValue(SerialChart.AxisForegroundProperty); }
            set { SetValue(SerialChart.AxisForegroundProperty, value); }
        }


        /// <summary>
        /// Identifies <see cref="GridStroke"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GridStrokeProperty = DependencyProperty.Register(
            "GridStroke", typeof(Brush), typeof(SerialChart),
            new PropertyMetadata(new SolidColorBrush(Colors.LightGray))
            );

        /// <summary>
        /// Gets or sets stroke brush for the value grid lines.
        /// This is a dependency property.
        /// The default is LightGray.
        /// </summary>
        public Brush GridStroke
        {
            get { return (Brush)GetValue(SerialChart.GridStrokeProperty); }
            set { SetValue(SerialChart.GridStrokeProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="ValueFormatString"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueFormatStringProperty = DependencyProperty.Register(
            "ValueFormatString", typeof(string), typeof(SerialChart),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the format string used to format values on axes and in tooltips.
        /// This is a depenency property.
        /// </summary>
        public string ValueFormatString
        {
            get { return (string)GetValue(SerialChart.ValueFormatStringProperty); }
            set { SetValue(SerialChart.ValueFormatStringProperty, value); }
        }


    }
}
