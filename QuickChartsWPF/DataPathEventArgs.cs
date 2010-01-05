using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AmCharts.Windows.QuickCharts
{
    public class DataPathEventArgs : EventArgs
    {
        public DataPathEventArgs(string newPath)
        {
            NewPath = newPath;
        }

        public string NewPath { get; set; }
    }
}
