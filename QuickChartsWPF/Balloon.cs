using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Represents a value balloon (tooltip).
    /// </summary>
    public class Balloon : Control
    {
        /// <summary>
        /// Instantiates Balloon.
        /// </summary>
        public Balloon()
        {
            this.DefaultStyleKey = typeof(Balloon);
            this.IsHitTestVisible = false;
        }

        /// <summary>
        /// Identifies <see cref="Text"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(Balloon),
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
