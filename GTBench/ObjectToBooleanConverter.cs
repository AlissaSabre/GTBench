﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace GTBench
{
    [MarkupExtensionReturnType(typeof(ObjectToBooleanConverter))]
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectToBooleanConverter : MarkupExtension, IValueConverter
    {
        public static readonly ObjectToBooleanConverter Instance = new ObjectToBooleanConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Instance;
        }
    }
}
