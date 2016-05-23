using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Networking.Connectivity;
using Edi.RPi.TestHub.Presenters;
using Edi.RPi.TestHub.Utils;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IoTCoreDefaultApp;

namespace Edi.RPi.TestHub.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private GpioController controller = null;

        public string BoardName
        {
            get { return _boardName; }
            set { _boardName = value; RaisePropertyChanged(); }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; RaisePropertyChanged(); }
        }

        public string OsVersion
        {
            get { return _osVersion; }
            set { _osVersion = value; RaisePropertyChanged(); }
        }

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
        private string _boardName;
        private string _deviceName;
        private string _lanIp;
        private string _osVersion;
        private string _currentTime;
        private string _ipAddress1;
        private string _networkName1;
        private ObservableCollection<NetworkPresenter.NetworkInfo> _networkInfos;

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

        public string CurrentTime
        {
            get { return _currentTime; }
            set { _currentTime = value; RaisePropertyChanged(); }
        }

        public string IpAddress1
        {
            get { return _ipAddress1; }
            set { _ipAddress1 = value; RaisePropertyChanged(); }
        }

        public string NetworkName1
        {
            get { return _networkName1; }
            set { _networkName1 = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<NetworkPresenter.NetworkInfo> NetworkInfos
        {
            get { return _networkInfos; }
            set { _networkInfos = value; RaisePropertyChanged(); }
        }

        public RelayCommand CommandPlayLEDRoll { get; set; }

        public RelayCommand CommandTurnLeft { get; set; }

        public RelayCommand CommandTurnRight { get; set; }

        public RelayCommand CommandStopMotor { get; set; }

        public int[] GpioPinIds => new int[] { 21, 20, 16, 12, 4, 5, 6, 13, 19, 26 };
        public List<KeyValuePair<GpioPin, int>> GpioPins { get; set; }

        public MainViewModel()
        {
            NetworkInfos = new ObservableCollection<NetworkPresenter.NetworkInfo>();
            UpdateBoardInfo();
            UpdateDateTime();

            IsOutputHighChecked = false;
            CommandPlayLEDRoll = new RelayCommand(async () => await PlayLedRoll());
            CommandTurnLeft = new RelayCommand(async () => await TurnMotor(90, TurnDirection.Left));
            CommandTurnRight = new RelayCommand(async () => await TurnMotor(90, TurnDirection.Right));
            CommandStopMotor = new RelayCommand(async () => await TurnMotor(0, TurnDirection.Right, true));

            Message = "Initializing the GPIO...";

            controller = GpioController.GetDefault();

            if (null == controller)
            {
                Message = "There is no GPIO controller.";
            }
            else
            {
                Message = "GPIO is initialized!";
                InitAllGPIOPin();
            }
        }

        private void UpdateBoardInfo()
        {
            BoardName = DeviceInfoPresenter.GetBoardName();

            ulong version = 0;
            if (!ulong.TryParse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion, out version))
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                OsVersion = loader.GetString("OSVersionNotAvailable");
            }
            else
            {
                OsVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}",
                    (version & 0xFFFF000000000000) >> 48,
                    (version & 0x0000FFFF00000000) >> 32,
                    (version & 0x00000000FFFF0000) >> 16,
                    version & 0x000000000000FFFF);
            }
        }

        public async Task UpdateNetworkInfo()
        {
            this.DeviceName = DeviceInfoPresenter.GetDeviceName();
            this.IpAddress1 = NetworkPresenter.GetCurrentIpv4Address();
            this.NetworkName1 = NetworkPresenter.GetCurrentNetworkName();

            var networkInfos = await NetworkPresenter.GetNetworkInformation();
            this.NetworkInfos = new ObservableCollection<NetworkPresenter.NetworkInfo>(networkInfos);
        }

        public void UpdateDateTime()
        {
            // Using DateTime.Now is simpler, but the time zone is cached. So, we use a native method insead.
            SYSTEMTIME localTime;
            NativeTimeMethods.GetLocalTime(out localTime);

            DateTime t = localTime.ToDateTime();
            CurrentTime = t.ToString("t", CultureInfo.CurrentCulture) + Environment.NewLine + t.ToString("d", CultureInfo.CurrentCulture);
        }

        private void InitAllGPIOPin()
        {
            GpioPins = new List<KeyValuePair<GpioPin, int>>();
            foreach (var pinId in GpioPinIds)
            {
                var pin = controller.OpenPin(pinId);
                pin.SetDriveMode(GpioPinDriveMode.Output);
                GpioPins.Add(new KeyValuePair<GpioPin, int>(pin, pinId));
            }
        }

        private async Task TurnMotor(int degree, TurnDirection direction, bool isStop = false)
        {
            foreach (var pin in GpioPins)
            {
                pin.Key.Dispose();
            }

            using (var uln2003Driver = new Uln2003Driver(controller, 26, 13, 6, 5))
            {
                // bug
                if (isStop)
                {
                    uln2003Driver.Stop();
                }
                else
                {
                    await uln2003Driver.TurnAsync(degree, direction, CancellationToken.None);
                }
            }

            InitAllGPIOPin();
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
