using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GTBench
{
    public abstract class SlidePage : Page
    {
        private TaskCompletionSource<bool> TaskCompletionSource;

        public ISlidePageController Controller { get; set; }

        public Task<bool> ShowDialogAsync()
        {
            TaskCompletionSource = new TaskCompletionSource<bool>();
            Controller.Show(this);
            return TaskCompletionSource.Task;
        }

        public void Close(bool ok = false)
        {
            Controller.Close(this);
            TaskCompletionSource?.TrySetResult(ok);
        }
    }
}
