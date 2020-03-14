using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Edi.RPi.RemoteBoat.EngineDriver
{
    public class Engine
    {
        public GpioController Controller { get; private set; }

        public TwoMotorsDriver Driver { get; }

        public Engine(GpioController controller, int in1, int in2, int in3, int in4)
        {
            if (null != controller)
            {
                Controller = controller;
                Driver = new TwoMotorsDriver(controller, in1, in2, in3, in4);
                Debug.WriteLine($"Initiated Boat Engine on PIN {in1}, {in2}, {in3}, {in4}");
            }
        }

        public void Forward()
        {
            Debug.WriteLine("Moving Forward");
            Driver.Stop();
            Driver.MoveForward();
        }

        public void Backward()
        {
            Driver.Stop();
            Driver.MoveBackward();
        }

        public async Task Left(int delay = 150)
        {
            Driver.Stop();
            await Task.Delay(delay);
            await Driver.TurnLeftAsync();
        }

        public async Task Right(int delay = 150)
        {
            Driver.Stop();
            await Task.Delay(delay);
            await Driver.TurnRightAsync();
        }

        public void Stop()
        {
            Driver.Stop();
        }
    }
}
