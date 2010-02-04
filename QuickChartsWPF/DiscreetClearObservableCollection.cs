using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AmCharts.Windows.QuickCharts
{
    /// <summary>
    /// A version of <see cref="ObservableCollection{T}"/> which on call to Clear() removes items one-by-one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DiscreetClearObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Removes items from the collection one-by-one.
        /// </summary>
        protected override void ClearItems()
        {
            for (int i = this.Count - 1; i >= 1; i--)
            {
                RemoveAt(i);
            }
        }
    }
}
