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
using System.ComponentModel;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Displays pie charts.
    /// </summary>
    public class PieChart : Control
    {
        /// <summary>
        /// Instantiates PieChart.
        /// </summary>
        public PieChart()
        {
            this.DefaultStyleKey = typeof(PieChart);
            this._slices.CollectionChanged += new NotifyCollectionChangedEventHandler(OnSlicesCollectionChanged);

            Padding = new Thickness(10);
        }


        private Balloon _balloon;

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
                    if (dataItem is INotifyPropertyChanged)
                    {
                        (dataItem as INotifyPropertyChanged).PropertyChanged -= OnDataSourceItemPropertyChanged;
                        (dataItem as INotifyPropertyChanged).PropertyChanged += new PropertyChangedEventHandler(OnDataSourceItemPropertyChanged);
                    }
                }
                _total = _values.Sum();
            }
        }

        void OnDataSourceItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TitleMemberPath || e.PropertyName == ValueMemberPath)
            {
                ProcessData();
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
                ((RotateTransform)_slices[i].RenderTransform).Angle = (_total != 0 ? runningTotal / _total * 360 : 360.0 / _slices.Count * i);
                runningTotal += _values[i];

                // tooltip
                string tooltipContent = _slices[i].Title + " : " + _values[i].ToString() + " (" + (_total != 0 ? _values[i] / _total : 1.0 / _slices.Count).ToString("0.#%") + ")";
                //ToolTipService.SetToolTip(_slices[i], tooltipContent);
                _slices[i].ToolTipText = tooltipContent;
            }
            UpdateLegend();
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
#if WINDOWS_PHONE
                slice.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnSliceManipulationStarted);
#else
                slice.MouseEnter += new MouseEventHandler(OnSliceMouseEnter);
                slice.MouseLeave += new MouseEventHandler(OnSliceMouseLeave);
                slice.MouseMove += new MouseEventHandler(OnSliceMouseMove);
#endif
                AddSliceToCanvas(slice);
            }
        }

#if WINDOWS_PHONE
        private bool isSliceEvent = false;
        void OnSliceManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            GeneralTransform gt = (sender as Slice).TransformToVisual(_sliceCanvasDecorator);
            DisplayBalloon(sender as Slice, gt.Transform(e.ManipulationOrigin));
            //e.Handled = true;
            isSliceEvent = true;
        }

        /// <summary>
        /// Reacts to touch manipulations.
        /// </summary>
        /// <param name="e">Manipulation event arguments</param>
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
        {
            if (!isSliceEvent)
            {
                HideBaloon();
                SwitchLegend();
            }
            else
            {
                isSliceEvent = false;
            }
        }

        private void SwitchLegend()
        {
            if (_legend.Visibility == Visibility.Visible)
            {
                _legend.Visibility = Visibility.Collapsed;
            }
            else
            {
                _legend.Visibility = Visibility.Visible;
            }
        }
#else
        void OnSliceMouseEnter(object sender, MouseEventArgs e)
        {
            DisplayBalloon(sender as Slice, e.GetPosition(_sliceCanvasDecorator));
        }

        void OnSliceMouseMove(object sender, MouseEventArgs e)
        {
            DisplayBalloon(sender as Slice, e.GetPosition(_sliceCanvasDecorator));
        }

        void OnSliceMouseLeave(object sender, MouseEventArgs e)
        {
            HideBaloon();
        }
#endif


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

        /// <summary>
        /// Applies control template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _sliceCanvasDecorator = (Border)TreeHelper.TemplateFindName("PART_SliceCanvasDecorator", this);
            _sliceCanvasDecorator.SizeChanged += new SizeChangedEventHandler(OnGraphCanvasDecoratorSizeChanged);
            _sliceCanvas = (Canvas)TreeHelper.TemplateFindName("PART_SliceCanvas", this);

            _balloon = (Balloon)TreeHelper.TemplateFindName("PART_Balloon", this);

            AddSlicesToCanvas();

            _legend = (Legend)TreeHelper.TemplateFindName("PART_Legend", this);

#if WINDOWS_PHONE
            _legend.ManipulationStarted += new EventHandler<ManipulationStartedEventArgs>(OnLegendManipulationStarted);
#endif

            UpdateLegend();
        }

#if WINDOWS_PHONE
        void OnLegendManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            SwitchLegend();
            e.Handled = true;
        }
#endif

        private void UpdateLegend()
        {
            if (_legend != null)
            {
                _legend.LegendItemsSource = _slices.Cast<ILegendItem>();
            }
        }

        void OnSlicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateLegend();
        }

        private void OnGraphCanvasDecoratorSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderSlices();
        }

        private ObservableCollection<Slice> _slices = new ObservableCollection<Slice>();

        private void RenderSlices()
        {
            if (_values.Count != _slices.Count)
                ReallocateSlices();

            ArrangeSlices();
            HideBaloon();
        }

        private void ArrangeSlices()
        {
            if (_sliceCanvasDecorator != null)
            {
                Point center = new Point(_sliceCanvasDecorator.ActualWidth / 2, _sliceCanvasDecorator.ActualHeight / 2);
                double radius = Math.Min(_sliceCanvasDecorator.ActualWidth, _sliceCanvasDecorator.ActualHeight) / 2;
                for (int i = 0; i < _slices.Count; i++)
                {
                    _slices[i].SetDimensions(radius, (_total != 0 ? _values[i] / _total : 1.0 / _slices.Count));
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

        private void DisplayBalloon(Slice slice, Point position)
        {
            _balloon.Text = slice.ToolTipText;
            _balloon.Visibility = Visibility.Visible;
            _balloon.Measure(new Size(_sliceCanvasDecorator.ActualWidth, _sliceCanvasDecorator.ActualHeight));
            double balloonLeft = position.X - _balloon.DesiredSize.Width / 2;
            if (balloonLeft < 0)
            {
                balloonLeft = position.X;
            }
            else if (balloonLeft + _balloon.DesiredSize.Width > _sliceCanvasDecorator.ActualWidth)
            {
                balloonLeft = position.X - _balloon.DesiredSize.Width;
            }
            double balloonTop = position.Y - _balloon.DesiredSize.Height - 5;
            if (balloonTop < 0)
            {
                balloonTop = position.Y + 17;
            }
            _balloon.SetValue(Canvas.LeftProperty, balloonLeft);
            _balloon.SetValue(Canvas.TopProperty, balloonTop);
        }

        private void HideBaloon()
        {
            if (_balloon != null)
            {
                _balloon.Visibility = Visibility.Collapsed;
            }
        }
    }
}
