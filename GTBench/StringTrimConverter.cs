using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace GTBench
{
    [MarkupExtensionReturnType(typeof(StringTrimConverter))]
    [ValueConversion(typeof(string), typeof(string))]
    public class StringTrimConverter : MarkupExtension, IValueConverter
    {
        public static readonly StringTrimConverter Instance = new StringTrimConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString().Trim();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
