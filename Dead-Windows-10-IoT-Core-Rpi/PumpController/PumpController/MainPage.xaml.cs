using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace PumpController
{
    public sealed partial class MainPage : Page
    {
        public GpioController GpioController { get; set; }

        public GpioPin RelayPin { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            GpioController = GpioController.GetDefault();
            RelayPin = GpioController.OpenPin(5);
            RelayPin.SetDriveMode(GpioPinDriveMode.Output);
            RelayPin.Write(GpioPinValue.High);
        }

        private void Pump_Toggled(object sender, RoutedEventArgs e)
        {
            RelayPin.Write(PumpToggle.IsOn ? GpioPinValue.Low : GpioPinValue.High);
        }
    }
}
