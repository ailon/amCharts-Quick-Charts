using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmCharts.Windows.QuickCharts
{
    public class LegendItem : ILegendItem
    {
        public string Title
        {
            get;
            set;
        }

        public System.Windows.Media.Brush Brush
        {
            get;
            set;
        }

        public ILegendItem OriginalItem
        {
            get;
            set;
        }
    }
}
