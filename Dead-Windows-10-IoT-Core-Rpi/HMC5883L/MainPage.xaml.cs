using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Edi.RPi.Utils.Sensors;

namespace HMC5883L_Sample
{
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _timer;

        public Hmc5883L Magnetometer { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                Magnetometer = new Hmc5883L(22); // GPIO22 connected to Hmc5883L DRDY-pin
            }
            catch
            {
                Debug.WriteLine("Sensor initialization failed.");
            }

            await Magnetometer.ConnectAsync();

            // Set magnetometer gain an averaging
            if (Magnetometer.Connected)
            {
                Magnetometer.WriteRegister(Hmc5883L.Register.ConfA, (byte)Hmc5883L.ConfigA.Average8);
                Magnetometer.WriteRegister(Hmc5883L.Register.ConfB, (byte)Hmc5883L.ConfigB.Gain1370);
            }

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            _timer.Tick += async (sender, o) =>
            {
                if (Magnetometer.Connected)
                {
                    var m = await Magnetometer.GetReadingAsync();
                    Debug.WriteLine("Magnetometer: " + m);
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        TxtX.Text = $"{m.X:f2} uT";
                        TxtY.Text = $"{m.Y:f2} uT";
                        TxtZ.Text = $"{m.Z:f2} uT";
                    });
                }
            };
            _timer.Start();
        }
    }
}
