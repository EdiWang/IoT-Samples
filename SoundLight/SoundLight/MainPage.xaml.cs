using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SoundLight
{
    public sealed partial class MainPage : Page
    {
        public GpioPin LedPin { get; set; }

        public GpioPin SoundPin { get; set; }

        public bool IsLightOn { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var controller = GpioController.GetDefault();
            if (null != controller)
            {
                LedPin = controller.OpenPin(5);
                LedPin.SetDriveMode(GpioPinDriveMode.Output);
                LedPin.Write(GpioPinValue.Low); // set light to off at start up

                SoundPin = controller.OpenPin(6);
                SoundPin.SetDriveMode(GpioPinDriveMode.Input);

                SoundPin.ValueChanged += (pin, args) =>
                {
                    var pinValue = SoundPin.Read();
                    if (pinValue == GpioPinValue.Low)
                    {
                        Debug.WriteLine("Sound Detected!");
                        LedPin.Write(IsLightOn ? GpioPinValue.Low : GpioPinValue.High);
                        IsLightOn = !IsLightOn;
                    }
                };
            }
        }
    }
}
