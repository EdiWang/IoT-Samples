using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Edi.RPi.RemoteClientRover
{
    public sealed partial class MainPage : Page
    {
        public GpioController Controller { get; set; }
        public TwoMotorsDriver Driver { get; set; }

        public GpioPin GreenLEDPin { get; set; }

        public GpioPin BlueLEDPin { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Controller = GpioController.GetDefault();
            if (null != Controller)
            {
                Driver = new TwoMotorsDriver(Controller, 5, 6, 27, 22);

                GreenLEDPin = Controller.OpenPin(23);
                GreenLEDPin.SetDriveMode(GpioPinDriveMode.Output);
                GreenLEDPin.Write(GpioPinValue.High);
                BlueLEDPin = Controller.OpenPin(24);
                BlueLEDPin.SetDriveMode(GpioPinDriveMode.Output);
                BlueLEDPin.Write(GpioPinValue.Low);
            }
        }

        private async void BtnForward_Click(object sender, RoutedEventArgs e)
        {
            await LogMessageAsync("Moving Forward");
            Driver.Stop();
            Driver.MoveForward();
            GreenLEDPin.Write(GpioPinValue.Low);
            BlueLEDPin.Write(GpioPinValue.High);
        }

        private async void BtnLeft_Click(object sender, RoutedEventArgs e)
        {
            await LogMessageAsync("Turning Left");
            Driver.Stop();
            await Task.Delay(150);
            await Driver.TurnLeftAsync();
            GreenLEDPin.Write(GpioPinValue.Low);
            BlueLEDPin.Write(GpioPinValue.High);
        }
       
        private async void BtnRight_Click(object sender, RoutedEventArgs e)
        {
            await LogMessageAsync("Turning Right");
            Driver.Stop();
            await Task.Delay(150);
            await Driver.TurnRightAsync();
            GreenLEDPin.Write(GpioPinValue.Low);
            BlueLEDPin.Write(GpioPinValue.High);
        }

        private async void BtnBackward_Click(object sender, RoutedEventArgs e)
        {
            await LogMessageAsync("Moving Backward");
            Driver.Stop();
            Driver.MoveBackward();
            GreenLEDPin.Write(GpioPinValue.Low);
            BlueLEDPin.Write(GpioPinValue.High);
        }

        private async void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            await LogMessageAsync("Stopping");
            Driver.Stop();
            GreenLEDPin.Write(GpioPinValue.High);
            BlueLEDPin.Write(GpioPinValue.Low);
        }

        private async Task LogMessageAsync(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtMessage.Text = $"\n{message}";
            });
        }
    }
}
