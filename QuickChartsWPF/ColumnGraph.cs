using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AmCharts.Windows.QuickCharts
{
    public class ColumnGraph : SerialGraph
    {
        public ColumnGraph()
        {
            this.DefaultStyleKey = typeof(ColumnGraph);
        }

        private Canvas _graphCanvas;

        public override void OnApplyTemplate()
        {
            _graphCanvas = (Canvas)TreeHelper.TemplateFindName("PART_GraphCanvas", this);
        }

        public override void Render()
        {
        }
    }
}
