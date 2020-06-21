using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GTBench
{
    public interface ISlidePageController
    {
        void Show(SlidePage slide_page);

        void Close(SlidePage slide_page);
    }
}
