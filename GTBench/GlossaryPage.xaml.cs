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
using Grpc.Core.Utils;
using Google.Api.Gax;

namespace GTBench
{
    using System.Collections.ObjectModel;
    using System.Windows.Media.Animation;
    using static GTBench.Helpers;

    /// <summary>
    /// Interaction logic for GlossaryPage.xaml
    /// </summary>
    public partial class GlossaryPage : Page, ISlidePageController
    {
        public GlossaryPage()
        {
            InitializeComponent();
        }

        private ObservableCollection<GlossaryInfo> Glossaries;

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = Properties.Settings.Default;
            glossaries.ItemsSource = Glossaries = new ObservableCollection<GlossaryInfo>();
            MainWindow.Current.Busy = true;
            try
            {
                var client = await GetTranslationServiceClientAsync();
                var response = client.ListGlossariesAsync(GetLocationName());
                await response.ForEachAsync(glossary =>
                {
                    Glossaries.Add(new GlossaryInfo(glossary));
                });
            }
            catch (Exception exception)
            {
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }
            MainWindow.Current.Busy = false;
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = Properties.Settings.Default;
            var create_page = new GlossaryCreateSlidePage { Controller = this, DataContext = DataContext };
            var ok = await create_page.ShowDialogAsync();
            if (!ok) return;

            MainWindow.Current.Busy = true;
            var sb = new StringBuilder();
            try
            {
                var client = await GetTranslationServiceClientAsync();
                var glossary = new Glossary
                {
                    InputConfig = new GlossaryInputConfig
                    {
                        GcsSource = new GcsSource { InputUri = create_page.InputUri },
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

        #region ISlidePageController implementation

        private SlidePage SlidePage;

        void ISlidePageController.Show(SlidePage slide_page)
        {
            SlidePage = slide_page;
            slidePageFrame.Navigate(slide_page);
            (slidePageFrame.FindResource("slideIn") as Storyboard).Begin();
        }

        void ISlidePageController.Close(SlidePage slide_page)
        {
            (slidePageFrame.FindResource("slideOut") as Storyboard).Begin();
            SlidePage = null;
        }

        private void SlidePageShield_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SlidePage?.Close(false);
        }

        #endregion
    }
}
