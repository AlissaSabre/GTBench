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
    using System.Collections.Specialized;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using static GTBench.Helpers;

    /// <summary>
    /// Interaction logic for GlossaryPage.xaml
    /// </summary>
    public partial class GlossaryPage : Page, ISlidePageController
    {
        private GlossaryManager GlossaryManager = new GlossaryManager();

        public GlossaryPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Current.Busy = true;
            var settings = Properties.Settings.Default;
            DataContext = settings;
            glossaries.ItemsSource = GlossaryManager.DataSource;
            try
            {
                await GlossaryManager.RefreshAsync();
            }
            catch (Exception exception)
            {
                new ExceptionDialog { Exception = exception }.ShowDialog();
            }
            MainWindow.Current.Busy = false;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            GlossaryManager.StopPolling();
        }

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            var create_page = new GlossaryCreateSlidePage
            {
                Controller = this,
                CreateInfo = GlossaryManager.CreateDefaultCreateInfo(),
            };
            if (await create_page.ShowDialogAsync())
            {
                MainWindow.Current.Busy = true;
                try
                {
                    await GlossaryManager.CreateAsync(create_page.CreateInfo);
                }
                catch (Exception exception)
                {
                    new ExceptionDialog { Exception = exception }.ShowDialog();
                }
                MainWindow.Current.Busy = false;
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var glossary_info = glossaries.SelectedItem as GlossaryInfo;
            if (glossary_info == null) return;

            var delete_page = new GlossaryDeleteSlidePage { Controller = this, DataContext = glossary_info };
            if (await delete_page.ShowDialogAsync())
            {
                MainWindow.Current.Busy = true;
                try
                {
                    await GlossaryManager.DeleteAsync(glossary_info.GlossaryID);
                }
                catch (Exception exception)
                {
                    new ExceptionDialog { Exception = exception }.ShowDialog();
                }
                MainWindow.Current.Busy = false;
            }
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
