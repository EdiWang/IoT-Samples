using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Edi.RPi.BMP180WebServer.Core;

namespace Edi.RPi.BMP180WebServer
{
    public sealed partial class MainPage : Page
    {
        public Bmp180Reader Bmp180Reader { get; set; }

        private Timer _periodicTimer;

        public MainPage()
        {
            this.InitializeComponent();

            // Register for the unloaded event so we can clean up upon exit
            Unloaded += MainPage_Unloaded;

            // Initialize the Sensors
            InitializeReader();

            object httpServer = null;
            if (CoreApplication.Properties.TryGetValue("httpserver", out httpServer))
            {
                var server = httpServer as WebServer.HttpServer;
                if (server != null)
                {
                    server.OnRequestReceived += request =>
                    {
                        Debug.WriteLine(request.URL);
                    };
                }
            }
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            Bmp180Reader.Dispose();
        }

        private async void InitializeReader()
        {
            string calibrationData;

            // Initialize the BMP180 Sensor
            try
            {
                await Bmp180Reader.Instance.InitializeSensorsAsync();
                calibrationData = Bmp180Reader.Instance.CalibrationData;
            }
            catch (Exception ex)
            {
                calibrationData = "Device Error! " + ex.Message;
            }

            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                calibrationDataTextBlock.Text = calibrationData;
            });
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (_periodicTimer == null)
            {
                _periodicTimer = new Timer(this.TimerCallback, null, 0, 1000);
                var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    button.Content = "Stop Reading Sensor";
                });
            }
            else
            {
                _periodicTimer.Dispose();
                var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    button.Content = "Get Sensor Readings";
                });
                _periodicTimer = null;
            }
        }

        private async void TimerCallback(object state)
        {
            string temperatureText = String.Empty;
            string pressureText = String.Empty;

            // Read and format Sensor data
            try
            {
                await Bmp180Reader.Instance.RefreshReadingsAsync();
                temperatureText += Bmp180Reader.Instance.Temperature;
                pressureText += Bmp180Reader.Instance.Pressure;
            }
            catch (Exception ex)
            {
                temperatureText = "Sensor Error: " + ex.Message;
                pressureText = "Sensor Error: " + ex.Message;
            }

            // UI updates must be invoked on the UI thread
            var task = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                temperatureTextBlock.Text = temperatureText;
                pressureTextBlock.Text = pressureText;
            });
        }
    }
}
