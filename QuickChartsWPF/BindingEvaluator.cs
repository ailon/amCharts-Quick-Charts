using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace AmCharts.Windows.QuickCharts
{
    public class BindingEvaluator : FrameworkElement
    {
        public BindingEvaluator(string propertyPath)
        {
            _propertyPath = propertyPath;
        }

        private string _propertyPath;

        public static readonly DependencyProperty EvaluatorProperty = DependencyProperty.Register(
            "Evaluator", typeof(object), typeof(BindingEvaluator), null);

        public object Eval(object source)
        {
            ClearValue(BindingEvaluator.EvaluatorProperty);
            var binding = new Binding(_propertyPath);
            binding.Mode = BindingMode.OneTime;
            binding.Source = source;
            SetBinding(BindingEvaluator.EvaluatorProperty, binding);
            return GetValue(BindingEvaluator.EvaluatorProperty);
        }
    }
}
