using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;

namespace AmCharts.Windows.QuickCharts
{
    public class Indicator : Control
    {
        public Indicator()
        {
            this.DefaultStyleKey = typeof(Indicator);
            Visibility = Visibility.Collapsed;
        }

        public void SetPosition(Point position)
        {
            SetValue(Canvas.LeftProperty, position.X - ActualWidth / 2);
            SetValue(Canvas.TopProperty, position.Y - ActualHeight / 2);
        }
    }
}
