using System;
using System.Collections.Generic;
using System.Diagnostics;
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

using static GTBench.Helpers;

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

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Busy = true;
            status.Text = string.Empty;
            warning.Visibility = Visibility.Collapsed;

            var settings = Properties.Settings.Default;
            var sb = new StringBuilder();

            try
            {
                languages.ItemsSource = null;

                sb.AppendLine();
                sb.AppendLine($"Project: {settings.ProjectID}");
                sb.AppendLine($"Location: {settings.LocationID}");
                status.Text = sb.ToString();

                var client = await GetTranslationServiceClientAsync();

                var request = new GetSupportedLanguagesRequest
                {
                    DisplayLanguageCode = "en",
                    Model = GetModelName(),
                    ParentAsLocationName = GetLocationName(),
                };

                var response = await client.GetSupportedLanguagesAsync(request);
                languages_label.Text = $"Available languages ({response.Languages.Count})";
                languages.ItemsSource = response.Languages.Select(lang => lang.LanguageCode + "\t" + lang.DisplayName);

                sb.AppendLine();
                sb.AppendLine("Google Translate service via Cloud Translation (Advanced) API is working.");
                status.Text = sb.ToString();
            }
            catch (Exception exception)
            {
                warning.Visibility = Visibility.Visible;
                // Let the warning panel show its contents.
                await Task.Yield();
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }

            MainWindow.Current.Busy = false;
        }

        private void configuration_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.NavigateTo("settings");
        }

        private void WebUrl_Click(object sender, RoutedEventArgs e)
        {
            Process.Start((sender as Hyperlink)?.Tag as string);
            e.Handled = true;
        }
    }
}
