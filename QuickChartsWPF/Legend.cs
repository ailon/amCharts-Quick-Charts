using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace AmCharts.Windows.QuickCharts
{
    public class Legend : ItemsControl
    {
        public Legend()
        {
            this.DefaultStyleKey = typeof(Legend);

            this.ItemsSource = _itemsSource;
        }

        private ObservableCollection<LegendItem> _itemsSource = new ObservableCollection<LegendItem>();

        private IEnumerable<ILegendItem> _legendItemsSource;
        public IEnumerable<ILegendItem> LegendItemsSource
        {
            get { return _legendItemsSource; }
            set
            {
                if (value is INotifyCollectionChanged)
                {
                    (value as INotifyCollectionChanged).CollectionChanged += new NotifyCollectionChangedEventHandler(LegendItemsSource_CollectionChanged);
                }
                _legendItemsSource = value;
                _itemsSource.Clear();
                AddLegendItems(value.ToList());
            }
        }

        void LegendItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _itemsSource.Clear();
            }
            else
            {
                if (e.OldItems != null)
                {
                    foreach (ILegendItem item in e.NewItems)
                    {
                        _itemsSource.Remove(_itemsSource.First(p => p.OriginalItem == item));
                    }
                }

                AddLegendItems(e.NewItems);
            }
        }

        private void AddLegendItems(IList items)
        {
            if (items != null)
            {
                foreach (ILegendItem item in items)
                {
                    _itemsSource.Add(new LegendItem() { 
                        Title = item.Title, 
                        Brush = item.Brush, 
                        OriginalItem = item 
                    });
                }
            }
        }
    }
}
