using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace GTBench
{
    [ValueConversion(typeof(bool), typeof(string), ParameterType=typeof(string))]
    public class BooleanToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool)) return DependencyProperty.UnsetValue;
            var strings = ParseParameter(parameter);
            if (strings is null) return DependencyProperty.UnsetValue;
            return strings[(bool)value ? 0 : 1];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var input = value as string;
            if (input is null) return DependencyProperty.UnsetValue;
            var strings = ParseParameter(parameter);
            if (strings is null) return DependencyProperty.UnsetValue;
            switch (Array.IndexOf(strings, input))
            {
                case 0: return true;
                case 1: return false;
                default: return DependencyProperty.UnsetValue;
            }
        }

        private string[] ParseParameter(object parameter)
        {
            var s = parameter as string;
            if (s is null) return null;
            var strings = s.Split(';');
            if (strings.Length != 2) return null;
            return strings;
        }

        public static readonly BooleanToStringConverter Instance = new BooleanToStringConverter();

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
