using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GTBench
{
    public class ExceptionDialog
    {
        public Exception Exception { get; set; }

        public void ShowDialog()
        {
            MessageBox.Show(Exception.ToString(), "Exception - GTBench");
        }
    }
}
