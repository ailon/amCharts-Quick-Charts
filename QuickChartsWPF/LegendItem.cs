using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AmCharts.Windows.QuickCharts
{
    // Note: This class is used to facilitate a workaround with Silverlight issue of displaying objects 
    // inherited from DependencyObject in ItemsControl

    /// <summary>
    /// Represents a single chart legend item.
    /// </summary>
    public class LegendItem : ILegendItem
    {
        /// <summary>
        /// Gets or sets legend item title.
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets legend item key brush.
        /// </summary>
        public Brush Brush
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets original source object for this item.
        /// </summary>
        public ILegendItem OriginalItem
        {
            get;
            set;
        }
    }
}
