using System;
using System.Diagnostics;
using System.Threading;
using Windows.Devices.Gpio;

namespace Edi.RPi.Utils.Sensors
{
    public class Hc04UltrasonicDistanceSensor
    {
        private GpioPin _pinTrig, _pinEcho;
        public double? Distance { get; }

        /// <summary>
        /// Available Gpio Pins. Refer: https://ms-iot.github.io/content/en-US/win10/samples/PinMappingsRPi2.htm
        /// </summary>
        public enum AvailableGpioPin : int
        {
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 29
            /// </summary>
            GpioPin_5 = 5,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 31
            /// </summary>
            GpioPin_6 = 6,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 32
            /// </summary>
            GpioPin_12 = 12,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 33
            /// </summary>
            GpioPin_13 = 13,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 36
            /// </summary>
            GpioPin_16 = 16,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 12
            /// </summary>
            GpioPin_18 = 18,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 15
            /// </summary>
            GpioPin_22 = 22,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 16
            /// </summary>
            GpioPin_23 = 23,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 18
            /// </summary>
            GpioPin_24 = 24,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 22
            /// </summary>
            GpioPin_25 = 25,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 37
            /// </summary>
            GpioPin_26 = 26,
            /// <summary>
            /// Raspberry Pi 2 - Header Pin Number : 13
            /// </summary>
            GpioPin_27 = 27
        }

        public Hc04UltrasonicDistanceSensor(AvailableGpioPin trigPin, AvailableGpioPin echoPin)
        {
            var gpio = GpioController.GetDefault();

            _pinTrig = gpio.OpenPin((int)trigPin);
            _pinTrig.SetDriveMode(GpioPinDriveMode.Output);
            _pinTrig.Write(GpioPinValue.Low);

            _pinEcho = gpio.OpenPin((int)echoPin);
            _pinEcho.SetDriveMode(GpioPinDriveMode.Input);
        }

        public double GetDistance()
        {
            var mre = new ManualResetEventSlim(false);

            //Send a 10µs pulse to start the measurement
            _pinTrig.Write(GpioPinValue.High);
            mre.Wait(TimeSpan.FromMilliseconds(0.01));
            _pinTrig.Write(GpioPinValue.Low);

            var time = PulseIn(_pinEcho, GpioPinValue.High, 500);

            // multiply by speed of sound in milliseconds (34000) divided by 2 (cause pulse make rountrip)
            var distance = time * 17000;
            return distance;
        }

        private double PulseIn(GpioPin pin, GpioPinValue value, ushort timeout)
        {
            var sw = new Stopwatch();
            var swTimeout = new Stopwatch();

            swTimeout.Start();

            // Wait for pulse
            while (pin.Read() != value)
            {
                if (swTimeout.ElapsedMilliseconds > timeout)
                {
                    return 3.5;
                }
            }
            sw.Start();

            // Wait for pulse end
            while (pin.Read() == value)
            {
                if (swTimeout.ElapsedMilliseconds > timeout)
                {
                    return 3.4;
                }
            }
            sw.Stop();

            return sw.Elapsed.TotalSeconds;
        }
    }
}
