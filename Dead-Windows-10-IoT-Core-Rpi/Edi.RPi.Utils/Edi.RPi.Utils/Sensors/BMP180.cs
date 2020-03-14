using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Edi.RPi.Utils.Interfaces;
using Edi.RPi.Utils.Models;

namespace Edi.RPi.Utils.Sensors
{
    /// <summary>
    /// Bosch Sensortec BMP180 pressure sensor
    /// </summary>
    public class Bmp180: IDisposable, IReadableSensor<BarometricReading>
    {
        private bool _calibrated;
        private int _address = 0x77;
        private I2cDevice _device;

        private int AC1;
        private int AC2;
        private int AC3;
        private uint AC4;
        private uint AC5;
        private uint AC6;
        private int B1;
        private int B2;
        private int MB;
        private int MC;
        private int MD;

        public enum Registers
        {
            AC1 = 0xAA,
            AC2 = 0xAC,
            AC3 = 0xAE,
            AC4 = 0xB0,
            AC5 = 0xB2,
            AC6 = 0xB4,
            B1 = 0xB6,
            B2 = 0xB8,
            MB = 0xBa,
            MC = 0xBC,
            MD = 0xBE,
            ID = 0xD0, // Always 0x55
            soft_reset = 0xE0,
            ctrl_meas = 0xF4,
            out_msb = 0xF6,
            out_lsb = 0xF7,
            out_xlsb = 0xF8
        }

        public Bmp180()
        {
            var hasI2C = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice");
            if (!hasI2C)
                throw new NotSupportedException();
        }

        /// <summary>
        /// Reads calibration coefficients
        /// </summary>
        public void ReadCalibration()
        {
            EnsureConnected();
            if (_calibrated) return;

            var calData = new byte[22];
            _device.WriteRead(new[] { (byte)Registers.AC1 }, calData);
            AC1 = calData.BeToInt16();
            AC2 = calData.BeToInt16(2);
            AC3 = calData.BeToInt16(4);
            AC4 = calData.BeToUInt16(6);
            AC5 = calData.BeToUInt16(8);
            AC6 = calData.BeToUInt16(10);
            B1 =  calData.BeToInt16(12);
            B2 =  calData.BeToInt16(14);
            MB =  calData.BeToInt16(16);
            MC =  calData.BeToInt16(18);
            MD =  calData.BeToInt16(20);
            _calibrated = true;
        }

        /// <summary>
        /// Connects to the device
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                var i2CBus = await I2CBusHelper.GetI2CBusDeviceInformation();
                if (i2CBus == null) return false; // bus not found

                // ADDR HI 0x5C, ADDR LO 0x23
                var settings = new I2cConnectionSettings(_address) {BusSpeed = I2cBusSpeed.FastMode};

                // Create an I2cDevice with our selected bus controller and I2C settings
                _device = await I2cDevice.FromIdAsync(i2CBus.Id, settings);

                var id = new byte[1];
                _device.WriteRead(new byte[] {(byte) Registers.ID}, id);
                if (id[0] != 0x55)
                    throw new InvalidOperationException("I2C device communication failure");
                Connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initialization failed: " + ex);
            }
            return Connected;
        }

        public bool Connected { get; private set; }

        public async Task<BarometricReading> GetReadingAsync()
        {
            ReadCalibration();


            // Get Uncalibrated Temperature
            _device.Write(new[] { (byte)Registers.ctrl_meas, (byte)0x2E });
            await Task.Delay(5);
            while (GetStatusFlag())
            {
                await Task.Delay(5);
            }

            var rawT = new byte[2];
            _device.WriteRead(new[] { (byte)Registers.out_msb },rawT);
            long UT = rawT.BeToInt16();

            // Get Uncalibrated Pressure
            byte measure = (byte)(0x34 | (OverSampling << 6));
            var rawP = new byte[3];
            bool hasPressure = false;
            do
            {
                _device.Write(new[] { (byte)Registers.ctrl_meas, measure });
                await Task.Delay(5 * (1 << OverSampling));
                while (GetStatusFlag())
                {
                    await Task.Delay(10);
                }
                _device.WriteRead(new[] { (byte)Registers.out_msb }, rawP);
                hasPressure = rawP[0] != rawT[0] || rawP[1] != rawT[1];
            } while (!hasPressure);
            long UP = (rawP.BeToUInt16() << 8 + rawP[2]) >> (8 - OverSampling);

            // Calculate true temperature
            long x1 = (UT - AC6) * AC5 >> 15;
            long x2 = (MC << 11) / (x1 + MD);
            long b5 = x1 + x2;
            var T = ((b5 + 8) >> 4 )/10.0;

            // Calculate true pressure
            var b6 = b5 - 4000;
            x1 = (B2 * (b6 * b6 >> 12)) >> 11;
            x2 = AC2 * b6 >> 11;
            long x3 = x1 + x2;
            long b3 = (((AC1 * 4 + x3) << OverSampling) + 2) >> 2;
            x1 = AC3 * b6 >> 13;
            x2 = (B1 * (b6 * b6 >> 12)) >> 16;
            x3 = (x1 + x2 + 2) >> 2;
            ulong b4 = AC4 * (ulong)(x3 + 32768) >> 15;
            ulong b7 = (ulong)(UP - b3) * (ulong)(50000 >> OverSampling);
            long P;
            if (b7 < 0x80000000)
                P = (long)((b7 * 2 ) / b4);
            else
                P = (long)((b7 / b4) * 2);
            x1 = (P >> 8) * (P >> 8);
            x1 = (x1 * 3038) >> 16;
            x2 = (-7357 * P) >> 16;
            P = P + ((x1 + x2 + 3791) >> 4);

            return new BarometricReading(P/100.0, T);
        }

        /// <summary>
        /// Returns true if measuring flag is set
        /// </summary>
        /// <remarks>seems to always be zero, unlike datasheet claims</remarks>
        /// <returns></returns>
        private bool GetStatusFlag()
        {
            bool measuring;
            var status = new byte[1];
            _device.WriteRead(new[] { (byte)Registers.ctrl_meas }, status);
            measuring = (status[0] & 1 << 5) == 1;
            return measuring;
        }

        private void EnsureConnected()
        {
            if (_device == null) throw new InvalidOperationException("I2C device not connected");
        }

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        private int _overSampling;


        /// <summary>
        /// OverSampling mode 0..3
        /// </summary>
        public int OverSampling
        {
            get { return _overSampling; }
            set
            {
                if (value < 0 || value > 3) throw new ArgumentOutOfRangeException(nameof(value), "0<=value<=3");
                _overSampling = value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _device?.Dispose();
                    _device = null;
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BMP180() {
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
