using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;


namespace GTBench
{
    using System.Globalization;
    using System.Windows.Markup;
    using static Helpers;

    /// <summary>
    /// Interaction logic for TranslatePage.xaml
    /// </summary>
    public partial class TranslatePage : Page
    {
        public TranslatePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Properties.Settings.Default;
        }

        private async void translateButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;
            MainWindow.Current.Busy = true;
            targetText.Text = string.Empty;

            try
            {
                var client = await GetTranslationServiceClientAsync();
                var request = new TranslateTextRequest
                {
                    Contents = { sourceText.Text },
                    GlossaryConfig = GetGlossaryConfig(),
                    MimeType = settings.MimeType,
                    Model = GetModelName(),
                    ParentAsLocationName = GetLocationName(),
                    SourceLanguageCode = settings.SourceLanguage,
                    TargetLanguageCode = settings.TargetLanguage,
                };
                request.Labels.AddLabels();
                var response = await client.TranslateTextAsync(request);
                targetText.Text = request.GlossaryConfig == null
                    ? response.Translations[0].TranslatedText
                    : response.GlossaryTranslations[0].TranslatedText;
            }
            catch (Exception exception)
            {
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }

            MainWindow.Current.Busy = false;
        }
    }

    [MarkupExtensionReturnType(typeof(HiddenIfNullOrWhite))]
    [ValueConversion(typeof(string), typeof(Visibility))]
    public class HiddenIfNullOrWhite : MarkupExtension, IValueConverter
    {
        public static readonly HiddenIfNullOrWhite Instance = new HiddenIfNullOrWhite();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            return string.IsNullOrWhiteSpace(text) ? Visibility.Hidden : Visibility.Visible;
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
