using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private PointCollection _locations;
        
        protected PointCollection Locations { get { return _locations; } }

        private double _groundLevel;

        protected double GroundLevel { get { return _groundLevel; } }
        
        public void SetPointLocations(PointCollection locations, double groundLevel)
        {
            _locations = locations;
            _groundLevel = groundLevel;
        }

        public abstract void Render();

        protected double XStep
        {
            get
            {
                if (Locations.Count > 1)
                {
                    return Locations[1].X - Locations[0].X;
                }
                else if (Locations.Count == 1)
                {
                    return Locations[0].X * 2;
                }
                else
                {
                    return 0;
                }
            }
        }

    }
}
