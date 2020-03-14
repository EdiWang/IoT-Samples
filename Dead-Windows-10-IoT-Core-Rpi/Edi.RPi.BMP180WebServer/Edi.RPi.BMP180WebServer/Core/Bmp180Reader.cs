using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Edi.RPi.BMP180WebServer.Core
{
    public class Bmp180Reader : IDisposable
    {
        private static volatile Bmp180Reader _instance = null;
        private static object objLock = new Object();

        private Bmp180Reader()
        {

        }

        public static Bmp180Reader Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (objLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new Bmp180Reader();
                        }
                    }
                }
                return _instance;
            }
        }

        private Bmp180Sensor _bmp180;

        public string CalibrationData { get; set; }

        public string Temperature { get; set; }

        public string Pressure { get; set; }

        public async Task InitializeSensorsAsync()
        {
            string calibrationData;

            // Initialize the BMP180 Sensor
            try
            {
                _bmp180 = new Bmp180Sensor();
                await _bmp180.InitializeAsync();
                calibrationData = _bmp180.CalibrationData.ToString();
            }
            catch (Exception ex)
            {
                calibrationData = "Device Error! " + ex.Message;
            }

            CalibrationData = calibrationData;
        }

        public async Task RefreshReadingsAsync()
        {
            string temperatureText, pressureText;

            // Read and format Sensor data
            try
            {
                var sensorData = await _bmp180.GetSensorDataAsync(Bmp180AccuracyMode.UltraHighResolution);
                temperatureText = sensorData.Temperature.ToString("F1");
                pressureText = sensorData.Pressure.ToString("F2");
                temperatureText += "C - hex:" + BitConverter.ToString(sensorData.UncompestatedTemperature);
                pressureText += "hPa - hex:" + BitConverter.ToString(sensorData.UncompestatedPressure);
            }
            catch (Exception ex)
            {
                temperatureText = "Sensor Error: " + ex.Message;
                pressureText = "Sensor Error: " + ex.Message;
            }

            Temperature = temperatureText;
            Pressure = pressureText;
        }

        public void Dispose()
        {
            _bmp180.Dispose();
        }
    }
}
