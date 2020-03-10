using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Edi.RPi.S3003Servo
{
    public sealed partial class MainPage : Page
    {
        // https://www.sitepoint.com/running-windows-10-iot-core-on-a-raspberry-pi/
        private const int SERVO_PIN_A = 5;
        private const int SERVO_PIN_B = 6;
        public GpioPin ServoPinA { get; private set; }
        public GpioPin ServoPinB { get; private set; }
        public double BeatPace { get; } = 1000;
        public int CounterClockwiseDanceMove { get; } = 1;
        public int ClockwiseDanceMove { get; } = 2;
        public int CurrentDirection { get; private set; }
        public double PulseFrequency { get; } = 20;
        public Stopwatch Stopwatch1 { get; private set; }

        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();
            this.InitDancing();
        }

        private void InitDancing()
        {
            var gpioController = GpioController.GetDefault();
            ServoPinA = gpioController.OpenPin(SERVO_PIN_A);
            ServoPinA.SetDriveMode(GpioPinDriveMode.Output);
            ServoPinB = gpioController.OpenPin(SERVO_PIN_B);
            ServoPinB.SetDriveMode(GpioPinDriveMode.Output);

            Stopwatch1 = Stopwatch.StartNew();

            CurrentDirection = 0; // Initially we aren't dancing at all.

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(BeatPace)
            };
            timer.Tick += Beat;

            if (ServoPinA != null && ServoPinB != null)
            {
                timer.Start();
                Windows.System.Threading.ThreadPool.RunAsync(this.MotorThread, Windows.System.Threading.WorkItemPriority.High);
            }

            if (GpioStatus != null)
            {
                GpioStatus.Text = "GPIO pin ready";
            }
        }

        private void Beat(object sender, object e)
        {
            CurrentDirection =
                CurrentDirection != ClockwiseDanceMove ?
                ClockwiseDanceMove :
                CounterClockwiseDanceMove;
        }

        private void MotorThread(IAsyncAction action)
        {
            while (true)
            {
                if (CurrentDirection != 0)
                {
                    ServoPinA.Write(GpioPinValue.High);
                    ServoPinB.Write(GpioPinValue.High);
                }

                Wait(CurrentDirection);

                ServoPinA.Write(GpioPinValue.Low);
                ServoPinB.Write(GpioPinValue.Low);
                Wait(PulseFrequency - CurrentDirection);
            }
        }

        private void Wait(double milliseconds)
        {
            long initialTick = Stopwatch1.ElapsedTicks;
            long initialElapsed = Stopwatch1.ElapsedMilliseconds;
            double desiredTicks = milliseconds / 1000.0 * Stopwatch.Frequency;
            double finalTick = initialTick + desiredTicks;
            while (Stopwatch1.ElapsedTicks < finalTick)
            {

            }
        }
    }
}
