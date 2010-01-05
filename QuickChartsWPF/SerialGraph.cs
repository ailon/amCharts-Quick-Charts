using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AmCharts.Windows.QuickCharts
{
    public abstract class SerialGraph : Control
    {

        public event EventHandler<DataPathEventArgs> ValueMemberPathChanged;

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
            "ValueMemberPath", typeof(string), typeof(SerialGraph), 
            new PropertyMetadata(null, new PropertyChangedCallback(SerialGraph.ValueMemberPath_PropertyChanged))
            );

        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        private static void ValueMemberPath_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SerialGraph graph = d as SerialGraph;
            if (graph.ValueMemberPathChanged != null)
            {
                graph.ValueMemberPathChanged(graph, new DataPathEventArgs(e.NewValue as string));
            }
        }
    }
}
