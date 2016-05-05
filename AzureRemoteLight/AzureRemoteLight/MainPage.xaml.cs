using Windows.UI.Xaml.Controls;
using AzureRemoteLight.ViewModel;

namespace AzureRemoteLight
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel _vm;

        public MainPage()
        {
            this.InitializeComponent();

            _vm = this.DataContext as MainViewModel;

            Loaded += async (sender, args) =>
            {
                // send device connected message
                await _vm.SendDeviceToCloudMessagesAsync();

                // receive remote light control events
                await _vm.ReceiveCloudToDeviceMessageAsync();
            };
        }
    }
}
