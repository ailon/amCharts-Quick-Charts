using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

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

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(Indicator),
            new PropertyMetadata(null)
            );

        public Brush Fill
        {
            get { return (Brush)GetValue(Indicator.FillProperty); }
            set { SetValue(Indicator.FillProperty, value); }
        }

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(Indicator),
            new PropertyMetadata(null)
            );

        public Brush Stroke
        {
            get { return (Brush)GetValue(Indicator.StrokeProperty); }
            set { SetValue(Indicator.StrokeProperty, value); }
        }
    }
}
