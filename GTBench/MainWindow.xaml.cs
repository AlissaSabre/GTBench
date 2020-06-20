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
using System.Windows.Threading;

namespace GTBench
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Current = this;
        }

        public static MainWindow Current { get; private set; }

        private bool _Busy;

        public bool Busy
        {
            get => _Busy;
            set
            {
                if (value != _Busy)
                {
                    Cursor = value ? Cursors.Wait : null;
                    busyPanel.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                    _Busy = value;
                }
            }
        }

        private readonly Dictionary<string, Page> Pages = new Dictionary<string, Page>
        {
            { "home", new HomePage() },
            { "translate", new TranslatePage() },
            { "glossary", new GlossaryPage() },
            { "settings", new SettingsPage() },
        };

        public void NavigateTo(string page_name)
        {
            if (Pages.TryGetValue(page_name, out var page))
            {
                frame.Navigate(page);
                Title = string.Format("{0} - GTBench", page.Title);
            }
            else
            {
                throw new ArgumentException($"unknown page name: {page_name}", nameof(page_name));
            }
        }

        private void menuItem_Selected(object sender, RoutedEventArgs e)
        {
            menu.IsOpen = false;
            NavigateTo((sender as FrameworkElement)?.Name);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }
}
