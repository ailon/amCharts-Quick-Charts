using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    public class ValueAxis : Control
    {
        public ValueAxis()
        {
            this.DefaultStyleKey = typeof(ValueAxis);

            VerticalAlignment = VerticalAlignment.Stretch;
        }

        private Canvas _valuesPanel;
        private Canvas _tickPanel;
        private List<double> _values = new List<double>();
        private List<double> _locations = new List<double>();
        private List<TextBlock> _valueBoxes = new List<TextBlock>();
        private List<Line> _valueTicks = new List<Line>();

        public override void OnApplyTemplate()
        {
            _valuesPanel = (Canvas)TreeHelper.TemplateFindName("PART_ValuesPanel", this);
            _tickPanel = (Canvas)TreeHelper.TemplateFindName("PART_TickPanel", this);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = new Size(0, 0);
            if (!double.IsInfinity(constraint.Height))
            {
                desiredSize.Height = constraint.Height;
            }
            double maxBoxWidth = 0;
            foreach (TextBlock valueBox in _valueBoxes)
            {
                valueBox.Measure(new Size(constraint.Width - 8, constraint.Height));
                maxBoxWidth = Math.Max(maxBoxWidth, valueBox.GetDesiredSize().Width);
            }
            desiredSize.Width = maxBoxWidth + 20;

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            foreach (TextBlock valueBox in _valueBoxes)
            {
                Size tbSize = valueBox.GetDesiredSize();
                double newTop = (double)valueBox.GetValue(Canvas.TopProperty) - tbSize.Height / 2;
                double newLeft = _valuesPanel.ActualWidth - tbSize.Width - 3;
                valueBox.SetValue(Canvas.TopProperty, newTop);
                valueBox.SetValue(Canvas.LeftProperty, newLeft);
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        public void SetValues(IEnumerable<double> values)
        {
            _values = new List<double>(values);
            CreateValueObjects();
        }

        public void SetLocations(IEnumerable<double> locations)
        {
            _locations = new List<double>(locations);
            SetObjectLocations();
            InvalidateArrange();
        }

        private void CreateValueObjects()
        {
            int count = Math.Min(_values.Count, _valueBoxes.Count);
            for (int i = 0; i < count; i++)
            {
                SetValueBoxText(i);
            }

            if (_values.Count != _valueBoxes.Count)
            {
                AddRemoveValueObjects(count);
            }
        }

        private void AddRemoveValueObjects(int count)
        {
            if (_values.Count > _valueBoxes.Count)
            {
                AddValueObjects(count);
            }
            else if (_values.Count < _valueBoxes.Count)
            {
                RemoveValueObjects(count);
            }
        }

        private void AddValueObjects(int count)
        {
            for (int i = count; i < _values.Count; i++)
            {
                _valueBoxes.Add(new TextBlock());
                SetValueBoxText(i);
                _valueTicks.Add(
                    new Line() 
                    { 
                        Stroke = new SolidColorBrush(Colors.Black), // TODO: real values
                        StrokeThickness = 1,
                        X1 = 0, X2 = 5
                    });

                _valuesPanel.Children.Add(_valueBoxes[i]);
                _tickPanel.Children.Add(_valueTicks[i]);
            }
            this.InvalidateMeasure();
        }

        private void RemoveValueObjects(int count)
        {
            for (int i = _valueBoxes.Count -1; i >= count; i--)
            {
                _valuesPanel.Children.Remove(_valueBoxes[i]);
                _tickPanel.Children.Remove(_valueTicks[i]);
                _valueBoxes.RemoveAt(i);
                _valueTicks.RemoveAt(i);
            }
        }

        private void SetValueBoxText(int index)
        {
            _valueBoxes[index].Text = _values[index].ToString(); // TODO: formatting
        }

        private void SetObjectLocations()
        {
            for (int i = 0; i < _valueBoxes.Count; i++)
            {
                _valueBoxes[i].SetValue(Canvas.TopProperty, _locations[i]);
                _valueTicks[i].SetValue(Canvas.TopProperty, _locations[i]);
            }
        }

    }

}
