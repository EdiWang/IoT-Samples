using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace Edi.RPi.RemoteRover
{
    public class TwoMotorsDriver
    {
        private readonly Motor _leftMotor;
        private readonly Motor _rightMotor;

        public GpioController GpioController { get; set; }

        public TwoMotorsDriver(GpioController controller, int leftMotorPinA, int leftMotorPinB, int rightMotorPinA, int rightMotorPinB)
        {
            _leftMotor = new Motor(controller, leftMotorPinA, leftMotorPinB);
            _rightMotor = new Motor(controller, rightMotorPinA, rightMotorPinB);
        }

        public void Stop()
        {
            _leftMotor.Stop();
            _rightMotor.Stop();
        }

        public void MoveForward()
        {
            _leftMotor.MoveForward();
            _rightMotor.MoveForward();
        }

        public void MoveBackward()
        {
            _leftMotor.MoveBackward();
            _rightMotor.MoveBackward();
        }

        public async Task TurnRightAsync()
        {
            _leftMotor.MoveForward();
            _rightMotor.MoveBackward();

            await Task.Delay(TimeSpan.FromMilliseconds(400));

            _leftMotor.Stop();
            _rightMotor.Stop();
        }

        public async Task TurnLeftAsync()
        {
            _leftMotor.MoveBackward();
            _rightMotor.MoveForward();

            await Task.Delay(TimeSpan.FromMilliseconds(400));

            _leftMotor.Stop();
            _rightMotor.Stop();
        }
    }
}