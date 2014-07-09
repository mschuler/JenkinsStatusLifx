using System.Windows.Threading;

namespace JenkinsStatusLifx.ViewModels
{
    public static class DispatcherProvider
    {
        public static Dispatcher Dispatcher { get; set; }
    }
}