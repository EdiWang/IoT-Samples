using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace LazerTripWire
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public GpioController Controller { get; set; }

        public GpioPin BuzzPin { get; set; }

        public GpioPin LazerPin { get; set; }

        public bool IsBusy { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Controller = GpioController.GetDefault();
            LazerPin = Controller.OpenPin(26);
            LazerPin.SetDriveMode(GpioPinDriveMode.Input);

            BuzzPin = Controller.OpenPin(6);
            BuzzPin.SetDriveMode(GpioPinDriveMode.Output);
            BuzzPin.Write(GpioPinValue.High);

            Debug.WriteLine("-----------------------------------------");

            LazerPin.ValueChanged += (pin, args) =>
            {
                if (!IsBusy)
                {
                    var read = LazerPin.Read();
                    Debug.WriteLine(read);
                    if (read == GpioPinValue.High)
                    {
                        BuzzPin.Write(GpioPinValue.High);
                        //IsBusy = true;
                        //await Beep();
                        //IsBusy = false;
                    }
                    else
                    {
                        BuzzPin.Write(GpioPinValue.Low);
                        //IsBusy = true;
                        //await Alarm(3);
                        //IsBusy = false;
                    }
                }
                else
                {
                    Debug.WriteLine("Busy");
                }
            };
        }

        public async Task Beep()
        {
            BuzzPin.Write(GpioPinValue.Low);
            await Task.Delay(300);
            BuzzPin.Write(GpioPinValue.High);
        }

        public async Task Alarm(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                BuzzPin.Write(GpioPinValue.Low);
                await Task.Delay(1000);
                BuzzPin.Write(GpioPinValue.High);
                await Task.Delay(500);
            }
        }
    }
}
