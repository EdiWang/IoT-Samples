using Windows.Devices.Gpio;
using Windows.UI.Xaml.Controls;

namespace Edi.IoT.LazerTripWire
{
    public sealed partial class MainPage : Page
    {
        public GpioPin TripPin { get; set; }

        public GpioPin BuzzPin { get; set; }

        public GpioController Controller { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            Controller = GpioController.GetDefault();

            BuzzPin = Controller.OpenPin(6);
            BuzzPin.SetDriveMode(GpioPinDriveMode.Output);
            BuzzPin.Write(GpioPinValue.High);

            TripPin = Controller.OpenPin(5);
            TripPin.SetDriveMode(GpioPinDriveMode.Input);
            TripPin.ValueChanged += (sender, args) =>
            {
                BuzzPin.Write(TripPin.Read() == GpioPinValue.Low ? 
                    GpioPinValue.Low : 
                    GpioPinValue.High);
            };
        }
    }
}
