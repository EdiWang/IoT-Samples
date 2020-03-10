using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.UI.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace AzureFireAlarm.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region GPIO Settings

        public GpioController GpioController { get; }

        public GpioPin RedLEDPin { get; }

        public int RedLEDPinNumber => 5;

        public GpioPin GreenLEDPin { get; }

        public int GreenLEDPinNumber => 6;

        public GpioPin FirePin { get; }

        public int FirePinNumber => 19;

        #endregion

        #region Display Fields

        private string _fireDetectionLog;

        public string FireDetectionLog
        {
            get { return _fireDetectionLog; }
            set { _fireDetectionLog = value; RaisePropertyChanged(); }
        }

        #endregion

        public MainViewModel()
        {
            GpioController = GpioController.GetDefault();
            if (null != GpioController)
            {
                // fire light
                RedLEDPin = GpioController.OpenPin(RedLEDPinNumber);
                RedLEDPin?.SetDriveMode(GpioPinDriveMode.Output);
                RedLEDPin?.Write(GpioPinValue.Low);

                // secure light
                GreenLEDPin = GpioController.OpenPin(GreenLEDPinNumber);
                GreenLEDPin?.SetDriveMode(GpioPinDriveMode.Output);
                GreenLEDPin?.Write(GpioPinValue.High);

                // fire detector
                FirePin = GpioController.OpenPin(FirePinNumber);
                if (null != FirePin)
                {
                    FirePin.SetDriveMode(GpioPinDriveMode.Input);
                    FirePin.ValueChanged += FirePinOnValueChanged;
                }
            }
        }

        private async void FirePinOnValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (sender.Read() == GpioPinValue.Low)
            {
                await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    FireDetectionLog += "\nFire detected! Sending alarm to Azure.";
                });
                
                // alarm buzz
                RedLEDPin.Write(GpioPinValue.High);
                GreenLEDPin?.Write(GpioPinValue.Low);
                await SendToastNotificationAsync(true);
            }
            else
            {
                await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    FireDetectionLog += "\nFire is killed! Sending alarm to Azure.";
                });

                // shut down buzz
                RedLEDPin.Write(GpioPinValue.Low);
                GreenLEDPin?.Write(GpioPinValue.High);
                await SendToastNotificationAsync(false);
            }
        }

        public async Task SendToastNotificationAsync(bool isFire)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "http://edi-rpi-firealarm.azurewebsites.net/FireAlarm/SendAlarm?isFire=" + isFire;
                await client.GetAsync(url);
                Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, isFire);
            }
        }
    }
}
