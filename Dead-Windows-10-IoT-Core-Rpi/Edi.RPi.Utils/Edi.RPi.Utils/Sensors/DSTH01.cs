using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Edi.RPi.Utils.Interfaces;
using Edi.RPi.Utils.Models;

namespace Edi.RPi.Utils.Sensors
{
    public class Dsth01 : IDisposable, IReadableSensor<RelativeHumidity>
    {
        private int _csPinNumber;
        private GpioPin _chipSelect;

        private readonly int _address = 0x40;
        private I2cDevice _device;
        private bool _heater;

        public enum Registers
        {
            Status = 0x00,
            DataH = 0x01,
            DataL = 0x02,
            Config = 0x03,
            Id = 0x04
        }

        [Flags]
        public enum Config
        {
            Fast = 1<<5,
            Temp = 1<<4,
            Heat = 1<<1,
            Start = 1<<0,
        }

        /// <summary>
        /// Chip select pin
        /// </summary>
        /// <param name="chipSelectPin"></param>
        public Dsth01(int chipSelectPin)
        {
            var hasGpio = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController");
            var hasI2C = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice");
            if (!hasGpio || !hasI2C)
                throw new NotSupportedException();

            _csPinNumber = chipSelectPin;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                DeviceInformation i2CBus = await I2CBusHelper.GetI2CBusDeviceInformation();
                if (i2CBus == null) return false; // bus not found


                // ADDR HI 0x5C, ADDR LO 0x23
                var settings = new I2cConnectionSettings(_address) {BusSpeed = I2cBusSpeed.FastMode};

                // Create an I2cDevice with our selected bus controller and I2C settings
                _device = await I2cDevice.FromIdAsync(i2CBus.Id, settings);

                var gpc = GpioController.GetDefault();
                if (gpc == null)
                    return false;
                _chipSelect = gpc.OpenPin(_csPinNumber);
                _chipSelect.Write(GpioPinValue.Low);
                _chipSelect.SetDriveMode(GpioPinDriveMode.Output);
                await Task.Delay(15);
                var sbuf = new byte[1];
                _device.WriteRead(new [] {(byte)Registers.Status},sbuf);
                _chipSelect.Write(GpioPinValue.High);
                Connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initialization failed: " + ex);
                _chipSelect?.Dispose();
                _chipSelect = null;
            }
            return Connected;
        }

        public bool Connected { get; private set; }

        /// <summary>
        /// Gets a temperature and humidity measurements from chip in standard mode
        /// </summary>
        /// <returns></returns>
        public async Task<RelativeHumidity> GetReadingAsync()
        {
            try
            {
                _chipSelect.Write(GpioPinValue.Low);
                await Task.Delay(15); // wake-up time

                var buf = new byte[2];
                _device.Write(new[] {(byte) Registers.Config, (byte) (Config.Temp | Config.Start | (_heater ? Config.Heat : 0))});
                await WaitConversionReady();


                _device.WriteRead(new[] {(byte) Registers.DataH}, buf);
                var t = (buf.BeToUInt16() >> 2)/32.0 - 50;

                _device.Write(new byte[] { (byte)Registers.Config, (byte)(Config.Start | (_heater ? Config.Heat : 0)) });
                await WaitConversionReady();
                _device.WriteRead(new[] { (byte)Registers.DataH }, buf);
                var rh = (buf.BeToUInt16() >> 4)/16.0 - 24;

                return new RelativeHumidity(t,rh);
            }
            finally
            {
                _chipSelect.Write(GpioPinValue.High);
            }
        }

        private async Task WaitConversionReady()
        {
            while (true)
            {
                await Task.Delay(10); // Fast conversion typical delay
                var sbuf = new byte[1];
                _device.WriteRead(new[] {(byte) Registers.Status}, sbuf);
                if ((sbuf[0] & 1) == 0) return;
            }
        }

        /// <summary>
        /// Heater element power flag used with next measurement reading
        /// </summary>
        public bool Heater
        {
            get {return _heater;}
            set { _heater = value; }
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _device?.Dispose();
                    _device = null;
                    _chipSelect?.Dispose();
                    _chipSelect = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DSTH01() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
