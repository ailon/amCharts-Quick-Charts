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
            
            // TODO: REMOVE TEMP SETTINGS BELOW
            _lineGraph.StrokeThickness = 3;
        }

        private void BindBrush()
        {
            Binding brushBinding = new Binding("Brush");
            brushBinding.Source = this;
            _lineGraph.SetBinding(Polyline.StrokeProperty, brushBinding);
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
    }
}
