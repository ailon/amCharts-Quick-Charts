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
    public class LineGraph : SerialGraph
    {
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

        public override void OnApplyTemplate()
        {
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
            _graphCanvas.Children.Add(_lineGraph);
        }

        public override void Render()
        {
            _lineGraph.Points = Locations;
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            "StrokeThickness", typeof(double), typeof(LineGraph),
            new PropertyMetadata(2.0)
            );

        public double StrokeThickness
        {
            get { return (double)GetValue(LineGraph.StrokeThicknessProperty); }
            set { SetValue(LineGraph.StrokeThicknessProperty, value); }
        }


    }
}
