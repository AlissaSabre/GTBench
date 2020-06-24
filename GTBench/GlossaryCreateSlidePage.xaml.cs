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
    /// <summary>
    /// Interaction logic for CreateGlossaryPanel.xaml
    /// </summary>
    public partial class GlossaryCreateSlidePage : SlidePage
    {
        public GlossaryCreateSlidePage()
        {
            InitializeComponent();
        }

        public GlossaryManager.CreateInfo CreateInfo
        {
            get { return (GlossaryManager.CreateInfo)DataContext; }
            set
            {
                DataContext = value;
                languages.Text = string.Join(", ", value.Languages);
            }
        }

        private static readonly char[] Separators = { ' ', ',', ';' };

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            if (CreateInfo.EquivalentTermSet)
            {
                CreateInfo.Languages = languages.Text.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            }
            Close(true);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
