using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace GTBench
{
    [MarkupExtensionReturnType(typeof(BooleanToVisibilityConverter))]
    public class BooleanToVisibilityConverterExtension : MarkupExtension
    {
        private static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();
        public override object ProvideValue(IServiceProvider serviceProvider) => Instance;
    }
}
