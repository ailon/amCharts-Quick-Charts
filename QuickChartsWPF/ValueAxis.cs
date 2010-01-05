using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AmCharts.Windows.QuickCharts
{
    public class ValueAxis : Control
    {
        public ValueAxis()
        {
            this.DefaultStyleKey = typeof(ValueAxis);

            VerticalAlignment = VerticalAlignment.Stretch;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size desiredSize = new Size(0, 0);
            if (!double.IsInfinity(constraint.Height))
            {
                desiredSize.Height = constraint.Height;
            }
            desiredSize.Width = 50; // TODO: real calculations

            return desiredSize;
        }
    }
}
