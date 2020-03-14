using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;

namespace MCP3008Test.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private float _currentSoilMoisture;
        private double _moisturePercentage;
        private string _message;

        public float CurrentSoilMoisture
        {
            get { return _currentSoilMoisture; }
            set { _currentSoilMoisture = value; RaisePropertyChanged(); }
        }

        public double MoisturePercentage
        {
            get { return _moisturePercentage; }
            set { _moisturePercentage = value; RaisePropertyChanged(); }
        }

        public string Message
        {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(); }
        }

        public MainViewModel()
        {
            Message = "Ready.";
        }

        public async Task Start()
        {
            var sensorProvider = new MCP3008DataProvider();
            await sensorProvider.mcp3008.Initialize();
            sensorProvider.DataReceived += async (o, eventArgs) =>
            {
                await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CurrentSoilMoisture = eventArgs.SensorValue;

                    MoisturePercentage = 100 - ((5 / 51.2) * CurrentSoilMoisture);

                    Message += $"\nSensorValue: {eventArgs.SensorValue}";
                });
            };

            sensorProvider.StartTimer();
        }
    }
}
