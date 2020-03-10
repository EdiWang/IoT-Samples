using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;

namespace Edi.RPi.RemoteRover
{
    public sealed partial class MainPage : Page
    {
        public GpioController Controller { get; set; }

        public GpioPin PinA { get; set; }

        public GpioPin PinB { get; set; }

        public GpioPin PinC { get; set; }

        public GpioPin PinD { get; set; }

        public TwoMotorsDriver Driver { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            Controller = GpioController.GetDefault();
            if (null != Controller)
            {
                Driver = new TwoMotorsDriver(Controller, 22, 27, 6, 5);

                // D1
                PinA = Controller.OpenPin(13);
                PinA.SetDriveMode(GpioPinDriveMode.Input);
                PinA.ValueChanged += async (sender, args) =>
                {
                    if (PinA.Read() == GpioPinValue.High)
                    {
                        await LogMessageAsync("Singal =========> A, Moving Forward");
                        Driver.MoveForward();
                        await LogMessageAsync("Operation Complete =========> A");
                    }
                };

                // D2
                PinB = Controller.OpenPin(19);
                PinB.SetDriveMode(GpioPinDriveMode.Input);
                PinB.ValueChanged += async (sender, args) =>
                {
                    if (PinB.Read() == GpioPinValue.High)
                    {
                        await LogMessageAsync("Singal =========> B, Stopping");
                        Driver.Stop();
                        await LogMessageAsync("Operation Complete =========> B");
                    }
                };

                // D4
                PinC = Controller.OpenPin(16);
                PinC.SetDriveMode(GpioPinDriveMode.Input);
                PinC.ValueChanged += async (sender, args) =>
                {
                    if (PinC.Read() == GpioPinValue.High)
                    {
                        await LogMessageAsync("Singal =========> C, Turning Right");
                        Driver.Stop();
                        await Task.Delay(150);
                        await Driver.TurnRightAsync();
                        await LogMessageAsync("Operation Complete =========> C");
                    }
                };

                // D3
                PinD = Controller.OpenPin(26);
                PinD.SetDriveMode(GpioPinDriveMode.Input);
                PinD.ValueChanged += async (sender, args) =>
                {
                    if (PinD.Read() == GpioPinValue.High)
                    {
                        await LogMessageAsync("Singal =========> D, Turning Left");
                        Driver.Stop();
                        await Task.Delay(150);
                        await Driver.TurnLeftAsync();
                        await LogMessageAsync("Operation Complete =========> D");
                    }
                };
            }
        }

        private async Task LogMessageAsync(string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                TxtMessage.Text += $"\n{message}";
            });
        }
    }
}
