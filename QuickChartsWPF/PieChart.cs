using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    public class PieChart : Control
    {
        public PieChart()
        {
            this.DefaultStyleKey = typeof(PieChart);
            this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
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
            SetAngles();
        }

        private void SetAngles()
        {
            double runningTotal = 0;
            for (int i = 0; i < _slices.Count; i++)
            {
                ((RotateTransform)_slices[i].RenderTransform).Angle = runningTotal / _total * 360;
                runningTotal += _values[i];
            }
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

        private List<Slice> _slices = new List<Slice>();

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
                double radius = Math.Min(_sliceCanvasDecorator.ActualWidth, _sliceCanvasDecorator.ActualHeight) / 2 * 0.8;
                for (int i = 0; i < _slices.Count; i++)
                {
                    _slices[i].SetDimensions(radius, _values[i] / _total);
                    _slices[i].SetValue(Canvas.LeftProperty, center.X);
                    _slices[i].SetValue(Canvas.TopProperty, center.Y);
                }
            }
        }
    }
}
