using System;
using System.Diagnostics;
using Windows.Devices.Gpio;

namespace Edi.RPi.RemoteBoat.RemoteController
{
    public class PT2272M4Module
    {
        public GpioController Controller { get; set; }

        public GpioPin PinA { get; set; }

        public GpioPin PinB { get; set; }

        public GpioPin PinC { get; set; }

        public GpioPin PinD { get; set; }

        public event PinATriggeredEventHandler PinATriggered;

        public delegate void PinATriggeredEventHandler(object sender, EventArgs e);

        public event PinBTriggeredEventHandler PinBTriggered;

        public delegate void PinBTriggeredEventHandler(object sender, EventArgs e);

        public event PinCTriggeredEventHandler PinCTriggered;

        public delegate void PinCTriggeredEventHandler(object sender, EventArgs e);

        public event PinDTriggeredEventHandler PinDTriggered;

        public delegate void PinDTriggeredEventHandler(object sender, EventArgs e);

        public PT2272M4Module(GpioController controller, int pinA, int pinB, int pinC, int pinD)
        {
            Controller = controller;
            PinA = Controller.OpenPin(pinA);
            PinA.SetDriveMode(GpioPinDriveMode.Input);
            PinB = Controller.OpenPin(pinB);
            PinB.SetDriveMode(GpioPinDriveMode.Input);
            PinC = Controller.OpenPin(pinC);
            PinC.SetDriveMode(GpioPinDriveMode.Input);
            PinD = Controller.OpenPin(pinD);
            PinD.SetDriveMode(GpioPinDriveMode.Input);

            Debug.WriteLine($"PT2272M4Module initiated on pin {pinA}, {pinB}, {pinC}, {pinD}");

            PinA.ValueChanged += (sender, args) =>
            {
                if (PinA.Read() == GpioPinValue.High)
                {
                    PinATriggered?.Invoke(sender, EventArgs.Empty);
                }
            };

            PinB.ValueChanged += (sender, args) =>
            {
                if (PinB.Read() == GpioPinValue.High)
                {
                    PinBTriggered?.Invoke(sender, EventArgs.Empty);
                }
            };

            PinC.ValueChanged += (sender, args) =>
            {
                if (PinC.Read() == GpioPinValue.High)
                {
                    PinCTriggered?.Invoke(sender, EventArgs.Empty);
                }
            };

            PinD.ValueChanged += (sender, args) =>
            {
                if (PinD.Read() == GpioPinValue.High)
                {
                    PinDTriggered?.Invoke(sender, EventArgs.Empty);
                }
            };
        }
    }
}
