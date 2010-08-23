using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Represents and indicator of current data point in serial chart.
    /// </summary>
    public class Indicator : Control
    {
        /// <summary>
        /// Creates Indicator instance.
        /// </summary>
        public Indicator()
        {
            this.DefaultStyleKey = typeof(Indicator);
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Positions indicator.
        /// </summary>
        /// <param name="position">Data point coordinates.</param>
        public void SetPosition(Point position)
        {
            SetValue(Canvas.LeftProperty, position.X);
            SetValue(Canvas.TopProperty, position.Y);
        }

        /// <summary>
        /// Identifies <see cref="Fill"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(Indicator),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets filling Brush for the indicator.
        /// This is a dependency property.
        /// </summary>
        public Brush Fill
        {
            get { return (Brush)GetValue(Indicator.FillProperty); }
            set { SetValue(Indicator.FillProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="Stroke"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            "Stroke", typeof(Brush), typeof(Indicator),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets stroke (outline) Brush for the indicator.
        /// This is a dependency property.
        /// </summary>
        public Brush Stroke
        {
            get { return (Brush)GetValue(Indicator.StrokeProperty); }
            set { SetValue(Indicator.StrokeProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Indicator),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets balloon text.
        /// This is a dependency property.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

    }
}
