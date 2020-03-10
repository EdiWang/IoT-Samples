using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace GY30LightSensor
{
    public class GY30LightSensorEventArgs : EventArgs
    {
        public int? Lux { get; set; }

        public GY30LightSensorEventArgs(int? lux)
        {
            Lux = lux;
        }
    }

    public class GY30LightSensor
    {
        public int Bh1750Address => 0x23;

        public I2cDevice I2CLightSensor { get; private set; }

        private Timer PeriodicTimer { get; set; }

        public int TimerIntervalMs { get; set; }

        public event ReadingEventHandler Reading;

        public delegate void ReadingEventHandler(object sender, GY30LightSensorEventArgs e);

        private void OnReading(int lux)
        {
            Reading?.Invoke(lux, new GY30LightSensorEventArgs(lux));
        }

        public GY30LightSensor(int timerIntervalMs = 100)
        {
            TimerIntervalMs = timerIntervalMs;
        }

        public async Task InitLightSensorAsync()
        {
            string aqs = I2cDevice.GetDeviceSelector();
            /* Get a selector string that will return all I2C controllers on the system */
            var dis = await DeviceInformation.FindAllAsync(aqs);
            /* Find the I2C bus controller device with our selector string           */
            if (dis.Count == 0)
            {
                throw new FileNotFoundException("No I2C controllers were found on the system");
            }

            var settings = new I2cConnectionSettings(Bh1750Address)
            {
                BusSpeed = I2cBusSpeed.FastMode
            };

            I2CLightSensor = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            /* Create an I2cDevice with our selected bus controller and I2C settings */
            if (I2CLightSensor == null)
            {
                throw new UnauthorizedAccessException(string.Format("Slave address {0} on I2C Controller {1} is currently in use by " +
                                 "another application. Please ensure that no other applications are using I2C.", settings.SlaveAddress, dis[0].Id));
            }

            /* Write the register settings */
            try
            {
                I2CLightSensor.Write(new byte[] { 0x10 }); // 1 [lux] aufloesung
            }
            /* If the write fails display the error and stop running */
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to communicate with device: " + ex.Message);
                throw;
            }

            PeriodicTimer = new Timer(this.TimerCallback, null, 0, TimerIntervalMs);
        }

        private void TimerCallback(object state)
        {
            var lux = ReadI2CLux();
            OnReading(lux);
        }

        private int ReadI2CLux()
        {
            byte[] regAddrBuf = new byte[] { 0x23 };
            byte[] readBuf = new byte[2];
            I2CLightSensor.WriteRead(regAddrBuf, readBuf);

            // is this calculation correct?
            var valf = ((readBuf[0] << 8) | readBuf[1]) / 1.2; 

            return (int)valf;
        }
    }
}
