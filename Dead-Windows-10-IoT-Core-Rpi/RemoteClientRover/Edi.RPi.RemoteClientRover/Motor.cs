using Windows.Devices.Gpio;

namespace Edi.RPi.RemoteClientRover
{
    public class Motor
    {
        private readonly GpioPin _motorGpioPinA;
        private readonly GpioPin _motorGpioPinB;

        public Motor(GpioController controller, int gpioPinIn1, int gpioPinIn2)
        {
            _motorGpioPinA = controller.OpenPin(gpioPinIn1);
            _motorGpioPinB = controller.OpenPin(gpioPinIn2);
            _motorGpioPinA.Write(GpioPinValue.Low);
            _motorGpioPinB.Write(GpioPinValue.Low);
            _motorGpioPinA.SetDriveMode(GpioPinDriveMode.Output);
            _motorGpioPinB.SetDriveMode(GpioPinDriveMode.Output);
        }

        public void MoveForward()
        {
            _motorGpioPinA.Write(GpioPinValue.Low);
            _motorGpioPinB.Write(GpioPinValue.High);
        }

        public void MoveBackward()
        {
            _motorGpioPinA.Write(GpioPinValue.High);
            _motorGpioPinB.Write(GpioPinValue.Low);
        }

        public void Stop()
        {
            _motorGpioPinA.Write(GpioPinValue.Low);
            _motorGpioPinB.Write(GpioPinValue.Low);
        }
    }
}
