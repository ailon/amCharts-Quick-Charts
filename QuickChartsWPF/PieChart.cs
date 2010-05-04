using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AmCharts.Windows.QuickCharts
{
    public class PieChart : Control
    {
        public PieChart()
        {
            this.DefaultStyleKey = typeof(PieChart);
            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
            this._slices.CollectionChanged += new NotifyCollectionChangedEventHandler(OnSlicedCollectionChanged);
        }


        //// LEGEND

        private Legend _legend;

        /// <summary>
        /// Identifies <see cref="LegendVisibility"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LegendVisibilityProperty = DependencyProperty.Register(
            "LegendVisibility", typeof(Visibility), typeof(PieChart),
            new PropertyMetadata(Visibility.Visible)
            );

        /// <summary>
        /// Gets or sets visibility of the chart legend.
        /// This is a dependency property.
        /// The default is Visible.
        /// </summary>
        public Visibility LegendVisibility
        {
            get { return (Visibility)GetValue(LegendVisibilityProperty); }
            set { SetValue(LegendVisibilityProperty, value); }
        }




        /// <summary>
        /// Identifies <see cref="DataSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            "DataSource", typeof(IEnumerable), typeof(PieChart),
            new PropertyMetadata(null, new PropertyChangedCallback(PieChart.OnDataSourcePropertyChanged)));

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
            PieChart chart = d as PieChart;
            DetachOldDataSourceCollectionChangedListener(chart, e.OldValue);
            AttachDataSourceCollectionChangedListener(chart, e.NewValue);
            chart.ProcessData();
        }

        private static void DetachOldDataSourceCollectionChangedListener(PieChart chart, object dataSource)
        {
            if (dataSource != null && dataSource is INotifyCollectionChanged)
            {
                (dataSource as INotifyCollectionChanged).CollectionChanged -= chart.OnDataSourceCollectionChanged;
            }
        }

        private static void AttachDataSourceCollectionChangedListener(PieChart chart, object dataSource)
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
        /// Identifies <see cref="TitleMemberPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleMemberPathProperty = DependencyProperty.Register(
            "TitleMemberPath", typeof(string), typeof(PieChart),
            new PropertyMetadata(null, new PropertyChangedCallback(PieChart.OnMemberPathPropertyChanged))
            );

        /// <summary>
        /// Gets or sets path to the property holding slice titles in data source.
        /// This is a dependency property.
        /// </summary>
        public string TitleMemberPath
        {
            get { return (string)GetValue(TitleMemberPathProperty); }
            set { SetValue(TitleMemberPathProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="ValueMemberPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
            "ValueMemberPath", typeof(string), typeof(PieChart),
            new PropertyMetadata(null, new PropertyChangedCallback(PieChart.OnMemberPathPropertyChanged))
            );

        /// <summary>
        /// Gets or sets path to the member in the datasource holding values for the slice.
        /// This is a dependency property.
        /// </summary>
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        private static void OnMemberPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PieChart chart = d as PieChart;
            if (chart != null)
            {
                chart.ProcessData();
            }
        }

        private List<string> _titles = new List<string>();
        private List<double> _values = new List<double>();
        private double _total;

        private void ProcessData()
        {
            if (this.DataSource != null)
            {
                SetData();
                ReallocateSlices();
            }
            else
            {
                _titles.Clear();
                _values.Clear();
                _total = 0;
            }
            //InvalidateArrange();
            RenderSlices();
        }

        private void SetData()
        {
            _titles.Clear();
            _values.Clear();
            if (!string.IsNullOrEmpty(TitleMemberPath) && !string.IsNullOrEmpty(ValueMemberPath))
            {
                BindingEvaluator titleEval = new BindingEvaluator(TitleMemberPath);
                BindingEvaluator valueEval = new BindingEvaluator(ValueMemberPath);
                foreach (object dataItem in this.DataSource)
                {
                    _titles.Add(titleEval.Eval(dataItem).ToString());
                    _values.Add((double)valueEval.Eval(dataItem));
                }
                _total = _values.Sum();
            }
        }

        private void ReallocateSlices()
        {
            if (_values.Count > _slices.Count)
            {
                AddSlices();
            }
            else if (_values.Count < _slices.Count)
            {
                RemoveSlices();
            }
            SetSliceData();
        }

        private void SetSliceData()
        {
            double runningTotal = 0;
            for (int i = 0; i < _slices.Count; i++)
            {
                // title
                _slices[i].Title = _titles[i];
                SetSliceBrush(i);

                // angle
                ((RotateTransform)_slices[i].RenderTransform).Angle = runningTotal / _total * 360;
                runningTotal += _values[i];

                // tooltip
                string tooltipContent = _slices[i].Title + " : " + _values[i].ToString() + " (" + (_values[i] / _total).ToString("0.#%") + ")";
                ToolTipService.SetToolTip(_slices[i], tooltipContent);
            }
        }

        private void SetSliceBrush(int index)
        {
            List<Brush> brushes = _brushes.Count > 0 ? _brushes : _presetBrushes;
            int brushCount = brushes.Count;
            _slices[index].Brush = brushes[index % brushCount];
        }

        private void RemoveSlices()
        {
            for (int i = _slices.Count - 1; i >= _values.Count; i--)
            {
                RemoveSliceFromCanvas(_slices[i]);
                _slices.RemoveAt(i);
            }
        }

        private void RemoveSliceFromCanvas(Slice slice)
        {
            if (_sliceCanvas != null && _sliceCanvas.Children.Contains(slice))
            {
                _sliceCanvas.Children.Remove(slice);
            }
        }

        private void AddSlices()
        {
            for (int i = _slices.Count; i < _values.Count; i++)
            {
                Slice slice = new Slice();
                slice.RenderTransform = new RotateTransform();
                slice.RenderTransformOrigin = new Point(0, 0);
                _slices.Add(slice);
                AddSliceToCanvas(slice);
            }
        }


        private void AddSlicesToCanvas()
        {
            foreach (Slice slice in _slices)
            {
                AddSliceToCanvas(slice);
            }
        }

        private void AddSliceToCanvas(Slice slice)
        {
            if (_sliceCanvas != null && !_sliceCanvas.Children.Contains(slice))
            {
                _sliceCanvas.Children.Add(slice);
            }
        }

        private Canvas _sliceCanvas;
        private Border _sliceCanvasDecorator;

        public override void OnApplyTemplate()
        {
            _sliceCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_SliceCanvasDecorator", this);
            _sliceCanvasDecorator.SizeChanged += new SizeChangedEventHandler(OnGraphCanvasDecoratorSizeChanged);
            _sliceCanvas = (Canvas)TreeHelper.TemplateFindName("PART_SliceCanvas", this);

            AddSlicesToCanvas();

            _legend = (Legend)TreeHelper.TemplateFindName("PART_Legend", this);
            UpdateLegend();
        }

        private void UpdateLegend()
        {
            if (_legend != null)
            {
                _legend.LegendItemsSource = _slices.Cast<ILegendItem>();
            }
        }

        void OnSlicedCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLegend();
        }

        private void OnGraphCanvasDecoratorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderSlices();
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            //RenderSlices();
            return base.ArrangeOverride(arrangeBounds);
        }

        private void OnLayoutUpdated(object sender, EventArgs e)
        {
            //RenderSlices();
        }

        private ObservableCollection<Slice> _slices = new ObservableCollection<Slice>();

        private void RenderSlices()
        {
            if (_values.Count != _slices.Count)
                ReallocateSlices();

            ArrangeSlices();
        }

        private void ArrangeSlices()
        {
            if (_sliceCanvasDecorator != null)
            {
                Point center = new Point(_sliceCanvasDecorator.ActualWidth / 2, _sliceCanvasDecorator.ActualHeight / 2);
                double radius = Math.Min(_sliceCanvasDecorator.ActualWidth, _sliceCanvasDecorator.ActualHeight) / 2;
                for (int i = 0; i < _slices.Count; i++)
                {
                    _slices[i].SetDimensions(radius, _values[i] / _total);
                    _slices[i].SetValue(Canvas.LeftProperty, center.X);
                    _slices[i].SetValue(Canvas.TopProperty, center.Y);
                }
            }
        }

        private List<Brush> _presetBrushes = new List<Brush>()
        {
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x0F, 0x00)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x66, 0x00)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x9E, 0x01)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xFC, 0xD2, 0x02)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xF8, 0xFF, 0x01)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xB0, 0xDE, 0x09)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x04, 0xD2, 0x15)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x0D, 0x8E, 0xCF)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x0D, 0x52, 0xD1)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x3A, 0x0C, 0xD0)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x8A, 0x0C, 0xCF)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xCD, 0x0D, 0x74)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x75, 0x4D, 0xEB)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0x33)),
            new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x00, 0x00))
        };

        private List<Brush> _brushes = new List<Brush>();

        /// <summary>
        /// Gets a collection of preset brushes used for slices.
        /// </summary>
        public List<Brush> Brushes
        {
            get { return _brushes; }
            set { throw new NotSupportedException(); }
        }

    }
}
