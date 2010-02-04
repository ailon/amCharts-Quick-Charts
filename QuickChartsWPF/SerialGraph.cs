using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Base class for graphs in serial chart.
    /// </summary>
    public abstract class SerialGraph : Control, ILegendItem
    {
        /// <summary>
        /// Event is raised when ValueMemberPath changes.
        /// </summary>
        public event EventHandler<DataPathEventArgs> ValueMemberPathChanged;

        /// <summary>
        /// Identifies <see cref="ValueMemberPath"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
            "ValueMemberPath", typeof(string), typeof(SerialGraph), 
            new PropertyMetadata(null, new PropertyChangedCallback(SerialGraph.OnValueMemberPathPropertyChanged))
            );

        /// <summary>
        /// Gets or sets path to the member in the datasource holding values for this graph.
        /// This is a dependency property.
        /// </summary>
        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        private static void OnValueMemberPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SerialGraph graph = d as SerialGraph;
            if (graph.ValueMemberPathChanged != null)
            {
                graph.ValueMemberPathChanged(graph, new DataPathEventArgs(e.NewValue as string));
            }
        }

        private PointCollection _locations;
        
        /// <summary>
        /// Gets locations (coordinates) of data points for the graph.
        /// </summary>
        protected PointCollection Locations { get { return _locations; } }

        private double _groundLevel;

        /// <summary>
        /// Gets Y-coordinate of 0 or a value closest to 0.
        /// </summary>
        protected double GroundLevel { get { return _groundLevel; } }
        
        /// <summary>
        /// Sets point coordinates and ground level.
        /// </summary>
        /// <param name="locations">Locations (coordinates) of data points for the graph.</param>
        /// <param name="groundLevel">Y-coordinate of 0 value or value closest to 0.</param>
        public void SetPointLocations(PointCollection locations, double groundLevel)
        {
            _locations = locations;
            _groundLevel = groundLevel;
        }

        /// <summary>
        /// When implemented in inheriting classes, renders the graphs visual.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Gets single x-axis step size (the distance between 2 neighbour data points).
        /// </summary>
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

        /// <summary>
        /// Identifies <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(SerialGraph),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the title of the graph.
        /// This is a dependency property.
        /// </summary>
        public string Title
        {
            get { return (string)GetValue(SerialGraph.TitleProperty); }
            set { SetValue(SerialGraph.TitleProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="Brush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(SerialGraph),
            new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets brush for the graph.
        /// This is a dependency property.
        /// </summary>
        public Brush Brush
        {
            get { return (Brush)GetValue(SerialGraph.BrushProperty); }
            set { SetValue(SerialGraph.BrushProperty, value); }
        }
    }
}
