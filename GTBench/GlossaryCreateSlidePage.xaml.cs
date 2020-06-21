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

        public string InputUri
        {
            get { return inputUri.Text.Trim(); }
            set { inputUri.Text = value; }
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
