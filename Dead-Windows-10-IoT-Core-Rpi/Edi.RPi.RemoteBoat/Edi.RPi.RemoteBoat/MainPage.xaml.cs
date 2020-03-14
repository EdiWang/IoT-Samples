using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Edi.RPi.RemoteBoat.EngineDriver;
using Edi.RPi.RemoteBoat.RemoteController;

namespace Edi.RPi.RemoteBoat
{
    public sealed partial class MainPage : Page
    {
        public Engine Engine { get; set; }

        public PT2272M4Module Pt2272M4Module { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            var ctl = GpioController.GetDefault();
            Engine = new Engine(ctl, 5, 6, 13, 19);
            Pt2272M4Module = new PT2272M4Module(ctl, 23, 27, 17, 24);

            Pt2272M4Module.PinATriggered += Pt2272M4ModuleOnPinATriggered;
            Pt2272M4Module.PinBTriggered += Pt2272M4ModuleOnPinBTriggered;
            Pt2272M4Module.PinCTriggered += Pt2272M4ModuleOnPinCTriggered;
            Pt2272M4Module.PinDTriggered += Pt2272M4ModuleOnPinDTriggered;
        }

        private void Pt2272M4ModuleOnPinATriggered(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("A");
            Engine.Forward();
        }

        private void Pt2272M4ModuleOnPinBTriggered(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("B");
            Engine.Backward();
        }

        private void Pt2272M4ModuleOnPinCTriggered(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("C");
            Engine.Stop();
        }

        private void Pt2272M4ModuleOnPinDTriggered(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("D");
        }
    }
}
