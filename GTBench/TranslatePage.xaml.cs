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

namespace GTBench
{
    /// <summary>
    /// Interaction logic for TranslatePage.xaml
    /// </summary>
    public partial class TranslatePage : Page
    {
        public TranslatePage()
        {
            InitializeComponent();
        }

        private TranslationServiceClient Client;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Properties.Settings.Default;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private async void translateButton_Click(object sender, RoutedEventArgs e)
        {
            var s = Properties.Settings.Default;
            MainWindow.Current.Busy = true;
            targetText.Text = string.Empty;

            try
            {
                if (Client == null)
                {
                    Client = string.IsNullOrWhiteSpace(s.CredentialsPath)
                        ? await TranslationServiceClient.CreateAsync()
                        : await new TranslationServiceClientBuilder { CredentialsPath = s.CredentialsPath }.BuildAsync();
                }
                var request = new TranslateTextRequest
                {
                    Contents = { sourceText.Text },
                    TargetLanguageCode = s.TargetLanguage,
                    ParentAsLocationName = new LocationName(s.ProjectID, s.LocationID),
                };
                var response = await Client.TranslateTextAsync(request);
                targetText.Text = response.Translations[0].TranslatedText;
            }
            catch (Exception exception)
            {
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }

            MainWindow.Current.Busy = false;
        }
    }
}
