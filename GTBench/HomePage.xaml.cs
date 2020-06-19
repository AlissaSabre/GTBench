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
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private TranslationServiceClient Client;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Busy = true;
            status.Text = string.Empty;
            warning.Visibility = Visibility.Collapsed;

            try
            {
                var s = Properties.Settings.Default;
                Client = string.IsNullOrWhiteSpace(s.CredentialsPath)
                    ? await TranslationServiceClient.CreateAsync()
                    : await new TranslationServiceClientBuilder { CredentialsPath = s.CredentialsPath }.BuildAsync();
                var request = new GetSupportedLanguagesRequest
                {
                    ParentAsLocationName = new LocationName(s.ProjectID, s.LocationID),
                    DisplayLanguageCode = "en",
                };
                if (!string.IsNullOrWhiteSpace(s.ModelID)) request.Model = $"projects/{s.ProjectID}/locations/{s.LocationID}/models/{s.ModelID}";
                var response = await Client.GetSupportedLanguagesAsync(request);
                var sb = new StringBuilder();
                sb.AppendLine("Google Cloud Translation Service is working.");
                sb.AppendLine();
                sb.Append("Supported source languages: ");
                sb.AppendLine(string.Join(", ", response.Languages
                    .Where(lang => lang.SupportSource)
                    .Select(lang => $"{lang.LanguageCode} ({lang.DisplayName})")));
                status.Text = sb.ToString();
            }
            catch (Exception exception)
            {
                warning.Visibility = Visibility.Visible;
                await Task.Yield();
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }

            MainWindow.Current.Busy = false;
        }

        private void configuration_Click(object sender, RoutedEventArgs e)
        {
           MainWindow.Current.NavigateTo("settings");
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Client = null;
        }
    }
}
