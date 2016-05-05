using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;

namespace WindowsIoTReedLight
{
    public sealed partial class MainPage : Page
    {
        public GpioController GpioController { get; set; }

        public GpioPin ReedPin { get; set; }

        public GpioPin LedPin { get; set; }

        public bool IsLightOn { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            GpioController = GpioController.GetDefault();
            if (null != GpioController)
            {
                ReedPin = GpioController.OpenPin(5);
                ReedPin.SetDriveMode(GpioPinDriveMode.Input);
                ReedPin.ValueChanged += (sender, args) =>
                {
                    if (ReedPin.Read() == GpioPinValue.Low)
                    {
                        IsLightOn = !IsLightOn;

                        LedPin.Write(IsLightOn ? GpioPinValue.Low : GpioPinValue.High);
                    }
                };

                LedPin = GpioController.OpenPin(6);
                LedPin.SetDriveMode(GpioPinDriveMode.Output);
                LedPin.Write(GpioPinValue.High);
            }
        }
    }
}
