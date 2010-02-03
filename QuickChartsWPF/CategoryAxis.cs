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
    /// <summary>
    /// Represents category axis in serial chart (usually x-axis)
    /// </summary>
    public class CategoryAxis : Control
    {
        /// <summary>
        /// Instantiates CategoryAxis object
        /// </summary>
        public CategoryAxis()
        {
            this.DefaultStyleKey = typeof(CategoryAxis);

            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        private Canvas _valuesPanel;
        private Canvas _tickPanel;
        private List<string> _values = new List<string>();
        private List<double> _locations = new List<double>();
        private List<TextBlock> _valueBoxes = new List<TextBlock>();
        private List<Line> _valueTicks = new List<Line>();

        /// <summary>
        /// Applies control template
        /// </summary>
        public override void OnApplyTemplate()
        {
            _valuesPanel = (Canvas)TreeHelper.TemplateFindName("PART_ValuesPanel", this);
            _tickPanel = (Canvas)TreeHelper.TemplateFindName("PART_TickPanel", this);
        }

        /// <summary>
        /// Measures desired size for the axis.
        /// </summary>
        /// <param name="constraint">Constraint</param>
        /// <returns>Desired size</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = new Size(0, 0);
            if (!double.IsInfinity(constraint.Width))
            {
                desiredSize.Width = constraint.Width;
            }
            double maxBoxHeight = 0;
            foreach (TextBlock valueBox in _valueBoxes)
            {
                valueBox.Measure(new Size(constraint.Width, constraint.Height - 8));
                maxBoxHeight = Math.Max(maxBoxHeight, valueBox.GetDesiredSize().Height);
            }
            desiredSize.Height = maxBoxHeight + 20;

            return desiredSize;
        }

        /// <summary>
        /// Arranges axis labels
        /// </summary>
        /// <param name="arrangeBounds">Arrange bounds</param>
        /// <returns>Arranged bounds</returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            foreach (TextBlock valueBox in _valueBoxes)
            {
                Size tbSize = valueBox.GetDesiredSize();
                double newLeft = (double)valueBox.GetValue(Canvas.LeftProperty) - tbSize.Width / 2;
                double newTop = 3;
                valueBox.SetValue(Canvas.TopProperty, newTop);
                valueBox.SetValue(Canvas.LeftProperty, newLeft);
            }

            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// Sets values to be displayed as axis labels
        /// </summary>
        /// <param name="values">Values</param>
        public void SetValues(IEnumerable<string> values)
        {
            _values = new List<string>(values);
            CreateValueObjects();
        }

        /// <summary>
        /// Set's locations of value labels (ticks) on the axis.
        /// </summary>
        /// <param name="locations">Value locations (coordinates)</param>
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
                        Stroke = Foreground,
                        StrokeThickness = 1,
                        Y1 = 0, Y2 = 5
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
            _valueBoxes[index].Text = _values[index];
        }

        private void SetObjectLocations()
        {
            for (int i = 0; i < _valueBoxes.Count; i++)
            {
                _valueBoxes[i].SetValue(Canvas.LeftProperty, _locations[i]);
                _valueTicks[i].SetValue(Canvas.LeftProperty, _locations[i]);
            }
        }

    }

}
