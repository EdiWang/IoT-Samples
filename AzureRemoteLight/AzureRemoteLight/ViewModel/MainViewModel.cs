using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using GalaSoft.MvvmLight;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace AzureRemoteLight.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region GPIO Settings

        public GpioController GpioController { get; }

        public GpioPin LedPin { get; }

        #endregion

        #region Azure IoT Hub Settings

        public DeviceClient DeviceClient { get; }

        public string IotHubUri { get; } = ".azure-devices.net";

        public string DeviceKey { get; } = "";

        public string DeviceId => "";

        #endregion

        #region Display Fields

        private bool _isAzureConnected;
        private string _cloudToDeviceLog;

        public bool IsAzureConnected
        {
            get { return _isAzureConnected; }
            set { _isAzureConnected = value; RaisePropertyChanged(); }
        }

        public string CloudToDeviceLog
        {
            get { return _cloudToDeviceLog; }
            set { _cloudToDeviceLog = value; RaisePropertyChanged(); }
        }

        #endregion

        public MainViewModel()
        {
            DeviceClient = DeviceClient.Create(IotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(DeviceId, DeviceKey));

            GpioController = GpioController.GetDefault();
            if (null != GpioController)
            {
                LedPin = GpioController.OpenPin(4);
                LedPin.SetDriveMode(GpioPinDriveMode.Output);
            }
        }

        public async Task SendDeviceToCloudMessagesAsync()
        {
            try
            {
                var telemetryDataPoint = new
                {
                    deviceId = DeviceId,
                    message = "Hello"
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await DeviceClient.SendEventAsync(message);
                Debug.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);

                IsAzureConnected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public async Task ReceiveCloudToDeviceMessageAsync()
        {
            CloudToDeviceLog = "Receiving events...";
            Debug.WriteLine("\nReceiving cloud to device messages from service");

            while (true)
            {
                Message receivedMessage = await DeviceClient.ReceiveAsync();
                if (receivedMessage == null) continue;

                var msg = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                CloudToDeviceLog += "\nReceived message: " + msg;

                if (msg == "on")
                {
                    LedPin.Write(GpioPinValue.Low);
                }

                if (msg == "off")
                {
                    LedPin.Write(GpioPinValue.High);
                }

                await DeviceClient.CompleteAsync(receivedMessage);
            }
        }
    }
}
