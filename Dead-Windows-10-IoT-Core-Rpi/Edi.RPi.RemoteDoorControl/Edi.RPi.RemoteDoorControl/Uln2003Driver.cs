using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Edi.RPi.RemoteDoorControl
{
    public class Uln2003Driver : IDisposable
    {
        public int IntervalMs { get; set; }

        private readonly GpioPin[] _gpioPins = new GpioPin[4];

        private readonly GpioPinValue[][] _waveDriveSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High}
        };

        private readonly GpioPinValue[][] _fullStepSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High},
            new[] {GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High }

        };

        private readonly GpioPinValue[][] _halfStepSequence =
        {
            new[] {GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High},
            new[] {GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High, GpioPinValue.Low, GpioPinValue.Low},
            new[] {GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.Low, GpioPinValue.High, GpioPinValue.High, GpioPinValue.High }
        };

        public Uln2003Driver(GpioController gpioController,
            int wireIn1, int wireIn2, int wireIn3, int wireIn4,
            GpioSharingMode sharingMode = GpioSharingMode.Exclusive, int intervalMs = 1)
        {
            var gpio = gpioController ?? GpioController.GetDefault();

            _gpioPins[0] = gpio.OpenPin(wireIn1, sharingMode);
            _gpioPins[1] = gpio.OpenPin(wireIn2, sharingMode);
            _gpioPins[2] = gpio.OpenPin(wireIn3, sharingMode);
            _gpioPins[3] = gpio.OpenPin(wireIn4, sharingMode);

            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
                gpioPin.SetDriveMode(GpioPinDriveMode.Output);
            }

            IntervalMs = intervalMs;
        }

        public async Task TurnAsync(TurnDirection direction, CancellationToken ct,
            DrivingMethod drivingMethod = DrivingMethod.FullStep)
        {
            bool stop = false;
            GpioPinValue[][] methodSequence;
            switch (drivingMethod)
            {
                case DrivingMethod.WaveDrive:
                    methodSequence = _waveDriveSequence;
                    break;
                case DrivingMethod.FullStep:
                    methodSequence = _fullStepSequence;
                    break;
                case DrivingMethod.HalfStep:
                    methodSequence = _halfStepSequence;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drivingMethod), drivingMethod, null);
            }
            while (!stop)
            {
                for (var j = 0; j < methodSequence[0].Length; j++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        _gpioPins[i].Write(methodSequence[direction == TurnDirection.Right ? i : 3 - i][j]);
                    }

                    // don't pass cancellation token, will blow up.
                    await Task.Delay(IntervalMs);

                    if (ct.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancel Requested, stop now.");
                        stop = true;
                        break;
                    }
                }
            }

            Stop();
        }

        public async Task TurnAsync(int degree, TurnDirection direction, CancellationToken ct,
            DrivingMethod drivingMethod = DrivingMethod.FullStep)
        {
            var steps = 0;
            GpioPinValue[][] methodSequence;
            switch (drivingMethod)
            {
                case DrivingMethod.WaveDrive:
                    methodSequence = _waveDriveSequence;
                    steps = (int)Math.Ceiling(degree / 0.1767478397486253);
                    break;
                case DrivingMethod.FullStep:
                    methodSequence = _fullStepSequence;
                    steps = (int)Math.Ceiling(degree / 0.1767478397486253);
                    break;
                case DrivingMethod.HalfStep:
                    methodSequence = _halfStepSequence;
                    steps = (int)Math.Ceiling(degree / 0.0883739198743126);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drivingMethod), drivingMethod, null);
            }
            var counter = 0;
            while (counter < steps)
            {
                for (var j = 0; j < methodSequence[0].Length; j++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        _gpioPins[i].Write(methodSequence[direction == TurnDirection.Right ? i : 3 - i][j]);
                    }

                    // don't pass cancellation token, will blow up.
                    await Task.Delay(IntervalMs);

                    if (ct.IsCancellationRequested)
                    {
                        Debug.WriteLine("Cancel Requested, stop now.");
                        counter = steps;
                    }
                    else
                    {
                        counter++;
                    }

                    if (counter == steps)
                    {
                        break;
                    }
                }
            }

            Stop();
        }

        public void Stop()
        {
            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
            }
        }

        public void Dispose()
        {
            foreach (var gpioPin in _gpioPins)
            {
                gpioPin.Write(GpioPinValue.Low);
                gpioPin.Dispose();
            }
        }
    }

    public enum DrivingMethod
    {
        WaveDrive,
        FullStep,
        HalfStep
    }

    public enum TurnDirection
    {
        Left,
        Right
    }
}