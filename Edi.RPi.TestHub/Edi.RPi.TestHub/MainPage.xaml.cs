using System;
using Windows.Networking.Connectivity;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Edi.RPi.TestHub.ViewModel;
using IoTCoreDefaultApp;

namespace Edi.RPi.TestHub
{
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        private CoreDispatcher MainPageDispatcher;
        private DispatcherTimer timer;
        private ConnectedDevicePresenter connectedDevicePresenter;

        private MainViewModel vm;

        public CoreDispatcher UIThreadDispatcher
        {
            get
            {
                return MainPageDispatcher;
            }

            set
            {
                MainPageDispatcher = value;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            vm = this.DataContext as MainViewModel;
            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            MainPageDispatcher = Window.Current.Dispatcher;

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            this.Loaded += async (sender, e) =>
            {
                UpdateConnectedDevices();
                await vm.UpdateNetworkInfo();

                timer = new DispatcherTimer();
                timer.Tick += timer_Tick;
                timer.Interval = TimeSpan.FromSeconds(10);
                timer.Start();
            };

            this.Unloaded += (sender, e) =>
            {
                timer.Stop();
                timer = null;
            };
        }

        private void timer_Tick(object sender, object e)
        {
            vm.UpdateDateTime();
        }

        private void UpdateConnectedDevices()
        {
            connectedDevicePresenter = new ConnectedDevicePresenter(MainPageDispatcher);
            this.ConnectedDevices.ItemsSource = connectedDevicePresenter.GetConnectedDevices();
        }

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            await MainPageDispatcher.RunAsync(CoreDispatcherPriority.Low, async () =>
            {
                var updateNetworkInfo = vm?.UpdateNetworkInfo();
                if (updateNetworkInfo != null) await updateNetworkInfo;
            });
        }

        private void ShutdownHelper(ShutdownKind kind)
        {
            ShutdownManager.BeginShutdown(kind, TimeSpan.FromSeconds(0.5));
        }

        private void BtnRestart_OnClick(object sender, RoutedEventArgs e)
        {
            ShutdownHelper(ShutdownKind.Restart);
        }

        private void BtnShutdown_OnClick(object sender, RoutedEventArgs e)
        {
            ShutdownHelper(ShutdownKind.Shutdown);
        }
    }
}
