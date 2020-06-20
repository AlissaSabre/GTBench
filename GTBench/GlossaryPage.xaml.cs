using Google.Cloud.Translate.V3;
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

namespace GTBench
{
    using static GTBench.Helpers;

    /// <summary>
    /// Interaction logic for GlossaryPage.xaml
    /// </summary>
    public partial class GlossaryPage : Page
    {
        public GlossaryPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Properties.Settings.Default;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;
            MainWindow.Current.Busy = true;
            var sb = new StringBuilder();
            try
            {
                var client = await GetTranslationServiceClientAsync();
                var glossary = new Glossary
                {
                    InputConfig = new GlossaryInputConfig
                    {
                        GcsSource = new GcsSource { InputUri = dataUri.Text.Trim() },
                    },
                    LanguagePair = new Glossary.Types.LanguageCodePair
                    {
                        SourceLanguageCode = settings.SourceLanguage,
                        TargetLanguageCode = settings.TargetLanguage,
                    },
                    Name = GetGlossaryName(),
                };

                var request_made = DateTime.UtcNow;
                sb.AppendLine($"Glossary creation started on {request_made:yyyyMMddTHHmmssZ}.");
                creationStatus.Text = sb.ToString();

                var create_operation = await client.CreateGlossaryAsync(GetLocationName(), glossary);
                var operation_name = create_operation.Name;
                sb.AppendLine($"Operation name = \"{operation_name}\"");
                creationStatus.Text = sb.ToString();
                await Task.Yield();
                while (!create_operation.IsCompleted)
                {
                    sb.AppendLine($"Glossary creation \"{create_operation.Metadata.State}\", waiting to complete for {(DateTime.UtcNow - request_made)}.");
                    creationStatus.Text = sb.ToString();

                    await Task.Delay(TimeSpan.FromSeconds(10));
                    create_operation = await client.PollOnceCreateGlossaryAsync(operation_name);
                }

                if (create_operation.IsFaulted)
                {
                    sb.AppendLine("Glossary creation failed.");
                    new ExceptionDialog { Exception = create_operation.Exception }.ShowDialog();
                }
                else
                {
                    var g = create_operation.Result;
                    sb.AppendLine($"Glossary {g.GlossaryName.GlossaryId} has been created.");
                    sb.AppendLine($"Creation started on {g.SubmitTime:yyyyMMddTHHmmssZ} and finished on {g.EndTime:yyyyMMddTHHmmssZ}.");
                    sb.AppendLine($"It includes {g.EntryCount} entries.");
                    creationStatus.Text = sb.ToString();
                }
            }
            catch (Exception exception)
            {
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }
            MainWindow.Current.Busy = false;
        }
    }
}
