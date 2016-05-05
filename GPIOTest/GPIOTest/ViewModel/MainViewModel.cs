using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation.Diagnostics;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GPIOTest.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private GpioController controller = null;

        private string _message;
        private bool _isOutputHighChecked;

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }

        #region GPIO Checkbox

        private bool _gpio04;
        private bool _gpio05;
        private bool _gpio06;
        private bool _gpio12;
        private bool _gpio13;
        private bool _gpio16;
        private bool _gpio19;
        private bool _gpio20;
        private bool _gpio21;
        private bool _gpio26;

        public bool GPIO_04
        {
            get { return _gpio04; }
            set
            {
                _gpio04 = value;
                WriteGPIOPin(4, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_05
        {
            get { return _gpio05; }
            set
            {
                _gpio05 = value;
                WriteGPIOPin(5, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_06
        {
            get { return _gpio06; }
            set
            {
                _gpio06 = value;
                WriteGPIOPin(6, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_12
        {
            get { return _gpio12; }
            set
            {
                _gpio12 = value;
                WriteGPIOPin(12, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_13
        {
            get { return _gpio13; }
            set
            {
                _gpio13 = value;
                WriteGPIOPin(13, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_16
        {
            get { return _gpio16; }
            set
            {
                _gpio16 = value;
                WriteGPIOPin(16, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_19
        {
            get { return _gpio19; }
            set
            {
                _gpio19 = value;
                WriteGPIOPin(19, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_20
        {
            get { return _gpio20; }
            set
            {
                _gpio20 = value;
                WriteGPIOPin(20, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_21
        {
            get { return _gpio21; }
            set
            {
                _gpio21 = value;
                WriteGPIOPin(21, value);
                RaisePropertyChanged();
            }
        }

        public bool GPIO_26
        {
            get { return _gpio26; }
            set
            {
                _gpio26 = value;
                WriteGPIOPin(26, value);
                RaisePropertyChanged();
            }
        }

        #endregion

        public bool IsOutputHighChecked
        {
            get { return _isOutputHighChecked; }
            set { _isOutputHighChecked = value; RaisePropertyChanged(); }
        }

        public RelayCommand CommandPlayLEDRoll { get; set; }

        public int[] GpioPinIds => new int[] { 21, 20, 16, 12, 4, 5, 6, 13, 19, 26 };
        public List<KeyValuePair<GpioPin, int>> GpioPins { get; set; }

        public MainViewModel()
        {
            IsOutputHighChecked = false;
            CommandPlayLEDRoll = new RelayCommand(async ()=> await PlayLedRoll());

            Message = "Initializing the GPIO...";

            controller = GpioController.GetDefault();
            if (null == controller)
            {
                Message = "There is no GPIO controller.";
            }
            else
            {
                Message = "GPIO is initialized!";

                GpioPins = new List<KeyValuePair<GpioPin, int>>();
                foreach (var pinId in GpioPinIds)
                {
                    var pin = controller.OpenPin(pinId);
                    pin.SetDriveMode(GpioPinDriveMode.Output);
                    GpioPins.Add(new KeyValuePair<GpioPin, int>(pin, pinId));
                }
            }
        }

        private async Task PlayLedRoll()
        {
            IsOutputHighChecked = false;

            foreach (int t in GpioPinIds)
            {
                await BlinkGpioPin(t);
            }
            for (int i = GpioPinIds.Length - 1; i >= 0; i--)
            {
                await BlinkGpioPin(GpioPinIds[i]);
            }
        }

        private async Task BlinkGpioPin(int gpioId)
        {
            WriteGPIOPin(gpioId, true);
            await Task.Delay(200);
            WriteGPIOPin(gpioId, false);
        }

        private void WriteGPIOPin(int gpioPinId, bool isHigh)
        {
            bool val = isHigh;
            if (!IsOutputHighChecked)
            {
                val = !val;
            }

            var pin = GpioPins.FirstOrDefault(p => p.Value == gpioPinId);
            pin.Key.Write(val ? GpioPinValue.High : GpioPinValue.Low);

            Message = $"Write [{(val ? GpioPinValue.High : GpioPinValue.Low)}] to GPIO [{gpioPinId}]";
        }
    }
}
