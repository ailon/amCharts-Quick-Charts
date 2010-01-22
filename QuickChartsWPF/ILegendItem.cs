using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    public interface ILegendItem
    {
        string Title { get; set; }
        Brush Brush { get; set; }
    }
}
