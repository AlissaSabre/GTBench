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
    /// <summary>
    /// Converts a GlossaryInfo to a detailed string representation.
    /// </summary>
    /// <remarks>
    /// This class is dedicated to <see cref="GlossaryPage"/> UI.
    /// It hard-codes some UI texts inside with no localizeable way. :(
    /// </remarks>
    [MarkupExtensionReturnType(typeof(IValueConverter))]
    [ValueConversion(typeof(GlossaryInfo), typeof(string))]
    public class GlossaryInfoToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var info = value as GlossaryInfo;
            if (info is null) return null;

            var nl = Environment.NewLine;

            var type_string =
                info.Type == "U" ? $"Unidirectional from {LanguageNames.FromCode(info.SourceLanguage)} to {LanguageNames.FromCode(info.TargetLanguage)}" :
                info.Type == "M" ? $"Equivalent term set containing {LanguageNames.FromList(info.SourceLanguage)}" :
                "(unknown)";

            return
                $"Glossary resource name: {Helpers.GetGlossaryName(info.GlossaryID)}" + nl +
                $"Glossary Type: {type_string}" + nl +
                $"Number of entries: {(object)info.Entries ?? "(unknown)"}" + nl +
                $"Input file URI: {info.InputUri}" + nl +
                nl + (
                    info.Status == GlossaryStatus.None ? "This glossary resource is ready for use." :
                    info.Status == GlossaryStatus.Creating ? "This glossary resource is being created on the cloud." :
                    info.Status == GlossaryStatus.Deleting ? "This glossary resource is being deleted on the cloud." :
                    info.Status == GlossaryStatus.Error ? $"An error occured with this glossary resource: {info.AlertMessage}" :
                    info.Status == GlossaryStatus.Faint ? "This is a remnant entry and will soon be removed." : "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new GlossaryInfoToStringConverter();
        }
    }
}
