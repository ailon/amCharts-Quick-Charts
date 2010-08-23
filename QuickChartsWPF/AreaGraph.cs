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
    /// Facilitates rendering of area graphs.
    /// </summary>
    public class AreaGraph : SerialGraph
    {
        /// <summary>
        /// Instantiates AreaGraph.
        /// </summary>
        public AreaGraph()
        {
            this.DefaultStyleKey = typeof(AreaGraph);
            _areaGraph = new Polygon();

            BindBrush();
        }

        private void BindBrush()
        {
            Binding brushBinding = new Binding("Brush");
            brushBinding.Source = this;
            _areaGraph.SetBinding(Polygon.FillProperty, brushBinding);
        }

        private Canvas _graphCanvas;
        private Polygon _areaGraph;

        /// <summary>
        /// Applies control template.
        /// </summary>
        public override void OnApplyTemplate()
        {
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
            _graphCanvas.Children.Add(_areaGraph);
        }

        /// <summary>
        /// Renders area graph.
        /// </summary>
        public override void Render()
        {
            PointCollection newPoints = GetAreaPoints();
            if (_areaGraph.Points.Count != newPoints.Count)
            {
                _areaGraph.Points = newPoints;
            }
            else
            {
                for (int i = 0; i < newPoints.Count; i++)
                {
                    if (!_areaGraph.Points[i].Equals(newPoints[i]))
                    {
                        _areaGraph.Points = newPoints;
                        break;
                    }
                }
            }
        }

        private PointCollection GetAreaPoints()
        {

            PointCollection points = new PointCollection();

            if (Locations == null)
                return points;

            CopyLocationsToPoints(points);

            if (points.Count > 0)
            {
                points.Insert(0, new Point(points[0].X, GroundLevel));
                points.Add(new Point(points[points.Count - 1].X, GroundLevel));
            }
            return points;
        }

        private void CopyLocationsToPoints(PointCollection points)
        {
            // copy Points from location cause SL doesn't support PointColleciton() with parameter
            foreach (Point p in Locations)
            {
                points.Add(p);
            }
        }
    }
}
