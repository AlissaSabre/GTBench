﻿using System;
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
                sb.AppendLine();
                sb.AppendLine($"Project: {settings.ProjectID}");
                sb.AppendLine($"Location: {settings.LocationID}");
                sb.AppendLine($"Model: {settings.ModelID}");
                sb.AppendLine($"Glossary: {settings.GlossaryID}");
                status.Text = sb.ToString();

                var client = await GetTranslationServiceClientAsync();

                var request = new GetSupportedLanguagesRequest
                {
                    DisplayLanguageCode = "en",
                    Model = GetModelName(),
                    ParentAsLocationName = GetLocationName(),
                };

                var response = await client.GetSupportedLanguagesAsync(request);

                sb.AppendLine();
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
    }
}
