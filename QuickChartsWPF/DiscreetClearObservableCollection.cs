using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AmCharts.Windows.QuickCharts
{
    public class DiscreetClearObservableCollection<T> : ObservableCollection<T>
    {
        protected override void ClearItems()
        {
            for (int i = this.Count - 1; i >= 1; i--)
            {
                RemoveAt(i);
            }
        }
    }
}
