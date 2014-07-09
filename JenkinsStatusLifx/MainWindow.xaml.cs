using System.Threading;
using JenkinsStatusLifx.ViewModels;
using LifxLib;

namespace JenkinsStatusLifx
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherProvider.Dispatcher = Dispatcher;
            DataContext = new MainViewModel();

            Loaded += (s, e) => ThreadPool.QueueUserWorkItem(_ => LifxNetwork.Instance.DiscoverNetworkAsync());
        }
    }
}
