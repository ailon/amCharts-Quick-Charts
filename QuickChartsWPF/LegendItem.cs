using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    public class LegendItem : ILegendItem
    {
        public string Title
        {
            get;
            set;
        }

        public Brush Brush
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
