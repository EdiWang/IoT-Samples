using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LCDTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Lcd _lcd;

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await InitLcd();
            base.OnNavigatedTo(e);
        }

        public string CurrentComputerName()
        {
            var hostNames = NetworkInformation.GetHostNames();
            var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
            return localName.DisplayName.Replace(".local", "");
        }

        public string CurrentIpAddress()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();

            if (icp != null && icp.NetworkAdapter != null)
            {
                var hostname =
                    NetworkInformation.GetHostNames()
                        .SingleOrDefault(
                            hn =>
                            hn.IPInformation != null && hn.IPInformation.NetworkAdapter != null
                            && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                            == icp.NetworkAdapter.NetworkAdapterId);

                if (hostname != null)
                {
                    // the ip address
                    return hostname.CanonicalName;
                }
            }

            return string.Empty;
        }

        private async Task InitLcd()
        {
            var controller = GpioController.GetDefault();

            _lcd = new Lcd(16, 2);
            await _lcd.InitAsync(controller, 18, 23, 19, 13, 6, 5);
            await _lcd.ClearAsync();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            _lcd.WriteLine(textBox.Text);
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            await _lcd.ClearAsync();
            _lcd.Write("Windows 10 IoT Core ");
            _lcd.SetCursor(0, 1);
            _lcd.Write("IP:" + CurrentIpAddress());
            //lcd.setCursor(0, 2);
            //lcd.write("Name:" + CurrentComputerName());
            //while (true)
            //{
            //    lcd.setCursor(0, 3);
            //    lcd.write("Time :" + DateTime.Now.ToString("HH:mm:ss.ffff"));
            //}
        }
    }
}
