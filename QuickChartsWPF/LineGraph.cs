using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Data;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// Facilitates rendering of line graphs.
    /// </summary>
    public class LineGraph : SerialGraph
    {
        /// <summary>
        /// Instantiates LineGraph.
        /// </summary>
        public LineGraph()
        {
            this.DefaultStyleKey = typeof(LineGraph);
            _lineGraph = new Polyline();

            BindBrush();
            BindStrokeThickness();
        }

        private void BindBrush()
        {
            Binding brushBinding = new Binding("Brush");
            brushBinding.Source = this;
            _lineGraph.SetBinding(Polyline.StrokeProperty, brushBinding);
        }

        private void BindStrokeThickness()
        {
            Binding thicknessBinding = new Binding("StrokeThickness");
            thicknessBinding.Source = this;
            _lineGraph.SetBinding(Polyline.StrokeThicknessProperty, thicknessBinding);
        }

        private Canvas _graphCanvas;
        private Polyline _lineGraph;

        /// <summary>
        /// Applies control template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
            _graphCanvas.Children.Add(_lineGraph);
        }

        /// <summary>
        /// Renders line graph.
        /// </summary>
        public override void Render()
        {
            _lineGraph.Points = Locations;
        }


        /// <summary>
        /// Identifies <see cref="StrokeThickness"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(LineGraph),
            new PropertyMetadata(2.0)
            );

        /// <summary>
        /// Gets or sets stroke thickness for a line graph line.
        /// This is a dependency property.
        /// The default is 2.
        /// </summary>
        public double StrokeThickness
        {
            get { return (double)GetValue(LineGraph.StrokeThicknessProperty); }
            set { SetValue(LineGraph.StrokeThicknessProperty, value); }
        }


    }
}
