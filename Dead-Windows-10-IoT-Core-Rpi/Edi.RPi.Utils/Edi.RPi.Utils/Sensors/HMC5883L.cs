using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Edi.RPi.Utils.Interfaces;
using Edi.RPi.Utils.Models;

namespace Edi.RPi.Utils.Sensors
{
    /// <summary>
    /// Honeywell HMC5883L 3 axis digital compass
    /// </summary>
    /// <remarks>Continuous reading not implemented</remarks>
    public class Hmc5883L : IDisposable, IReadableSensor<MagneticField>
    {
        public class SelfTestException : Exception
        {
            public SelfTestException()
            {
            }

            public SelfTestException(string message) : base(message)
            {
            }

            public SelfTestException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }


        private const int Address = 0x1E;
        private I2cDevice _device;
        private TaskCompletionSource<MagneticField> _tcs;
        private GpioPin _drdy;
        private int? _drdyPin;

        public enum Register
        {
            ConfA = 0,
            ConfB = 1,
            Mode,
            DataXMsb,
            DataXLsb,
            DataYMsb,
            DataYLsb,
            DataZMsb,
            DataZLsb,
            Status,
            IdA, IdB, IdC

        }

        [Flags]
        public enum ConfigA
        {
            MA1 = 1 << 6,
            MA0 = 1 << 5,
            DO2 = 1 << 4,
            DO1 = 1 << 3,
            DO0 = 1 << 2,
            MS1 = 1 << 1,
            MS0 = 1 << 0,
            Average1 = 0,
            Average2 = MA0,
            Average4 = MA1,
            Average8 = MA0|MA1,
            ModeNoBias = 0,
            ModePosBias = MS0,
            ModeNegBias = MS1,
            DataRate_0_75Hz = 0,
            DataRate_1_5Hz = DO0,
            DataRate_3Hz = DO1,
            DataRate_7_5Hz = DO1 | DO0,
            DataRate_15Hz = DO2,
            DataRate_30Hz = DO2 | DO0,
            DataRate_75Hz = DO2 | DO1
        }

        /// <summary>
        /// Gain values a milliGauss per count
        /// </summary>
        [Flags]
        public enum ConfigB
        {
            GN0 = 1 << 5,
            GN1 = 1 << 6,
            GN2 = 1 << 7,
            Gain1370 = 0,
            Gain1090 = GN0,
            Gain820 = GN1,
            Gain660 = GN1 | GN0,
            Gain440 = GN2,
            Gain390 = GN2 | GN0,
            Gain330 = GN2 | GN1,
            Gain230 = GN2 | GN1 | GN0,
            GainMask = GN0|GN1|GN2
        }
        public enum ModeReg
        {
            HighSpeed = 1 << 7,
            MD1 = 1 << 1,
            MD0 = 1 << 0,
            Continuous = 0,
            SingleShot = MD0,
            Idle = MD1
        }

        public enum StatusReg
        {
            LOCK = 1 << 1,
            RDY = 1 << 0,
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="drdyPin">(optional) GpioPin number for DRDY signal for fast result reading</param>
        public Hmc5883L(int? drdyPin = null)
        {
            var hasGpio = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.Gpio.GpioController");
            var hasI2C = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice");
            if (!hasGpio || !hasI2C)
                throw new NotSupportedException();
            _drdyPin = drdyPin;
        }

        /// <summary>
        /// Open connection to chip
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectAsync()
        {
            try
            {
                var i2CBus = await I2CBusHelper.GetI2CBusDeviceInformation();
                if (i2CBus == null) return false; // bus not found

                // ADDR HI 0x5C, ADDR LO 0x23
                var settings = new I2cConnectionSettings(Address) {BusSpeed = I2cBusSpeed.FastMode};

                // Create an I2cDevice with our selected bus controller and I2C settings
                _device = await I2cDevice.FromIdAsync(i2CBus.Id, settings);

                // Self-test before attaching to the DRDY pin
                await SelfTest().ConfigureAwait(true);

                if (_drdyPin.HasValue)
                {
                    var gpc = GpioController.GetDefault();
                    if (gpc == null)
                        return false;
                    _drdy = gpc.OpenPin(_drdyPin.Value);
                    _drdy.Write(GpioPinValue.High);
                    _drdy.SetDriveMode(GpioPinDriveMode.Input);
                    _drdy.ValueChanged += DataReady;
                }

                Connected = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Initialization failed: " + ex);
            }
            return Connected;
        }

        private async Task SelfTest()
        {
            var selftestGains = new[] {ConfigB.Gain390, ConfigB.Gain330, ConfigB.Gain230};
            var gain = 0;
            // enter self-test mode
            _device.Write(new[] {(byte) Register.ConfA, (byte)(ConfigA.DataRate_15Hz|ConfigA.Average8|ConfigA.MS0)});
            _device.Write(new[] {(byte) Register.ConfB, (byte) (selftestGains[gain])});
            _device.Write(new[] {(byte)Register.Mode, (byte)0x00});
            await Task.Delay(6);
            try
            {
                while (true)
                {
                    var rbuf = new byte[6];
                    _device.WriteRead(new[] { (byte)Register.DataXMsb }, rbuf); // Read and discard one sample
                    // wait for measurement to be available
                    await Task.Delay(80);
                    _device.WriteRead(new[] {(byte) Register.DataXMsb}, rbuf);
                    var x = rbuf.BeToInt16();
                    var y = rbuf.BeToInt16(2);
                    var z = rbuf.BeToInt16(4);
                    
                    // test for bounds
                    var lowLimit = 243;
                    var highLimit = 575;
                    if (gain == 1)
                    {
                        lowLimit = 243*330/390;
                        highLimit = 575*330/390;
                    }else if (gain == 2)
                    {
                        lowLimit = 243 * 230 / 390;
                        highLimit = 575 * 230 / 390;
                    }

                    // check values to be within bounds
                    if (new[] { x,y,z}.All(d=> lowLimit<d && d<highLimit))
                        break;

                    // increase gain if local fields take measurements over range
                    if (gain < 2)
                        gain++;
                    else
                        throw new SelfTestException();

                    _device.Write(new[] { (byte)Register.ConfB, (byte)(selftestGains[gain]) });
                    await Task.Delay(80); // wait for one measurement cycle using old gain
                }
            }
            finally
            {
                // exit self-test mode
                _device.Write(new[] {(byte) Register.ConfA, (byte) (ConfigA.DataRate_15Hz | ConfigA.Average8)});
            }
        }

        public bool Connected { get; private set; }


        private void DataReady(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                if (_tcs == null) return;
                try
                {
                    var r = ReadRegister(Register.ConfB, 8);
                    MagneticField field = RegistersToMagneticField(r);

                    _tcs.SetResult(field);
                }
                catch (Exception ex)
                {
                    _tcs.SetException(ex);
                }
            }
        }

        private static MagneticField RegistersToMagneticField(byte[] r)
        {
            int x = r.BeToInt16(2);
            int y = r.BeToInt16(4);
            int z = r.BeToInt16(6);

            double gain = 1090;
            switch ((ConfigB)r[0] & ConfigB.GainMask)
            {
                case ConfigB.Gain1370:
                    gain = 1370;
                    break;
                case ConfigB.Gain1090:
                    gain = 1090;
                    break;
                case ConfigB.Gain820:
                    gain = 820;
                    break;
                case ConfigB.Gain660:
                    gain = 660;
                    break;
                case ConfigB.Gain440:
                    gain = 440;
                    break;
                case ConfigB.Gain390:
                    gain = 390;
                    break;
                case ConfigB.Gain330:
                    gain = 330;
                    break;
                case ConfigB.Gain230:
                    gain = 230;
                    break;
            }
            var field = new MagneticField(x / gain * 100.0, y / gain * 100.0, z / gain * 100.0);
            return field;
        }



        /// <summary>
        /// Read registers
        /// </summary>
        /// <param name="reg">Register to read</param>
        /// <param name="count">Number of bytes to read</param>
        /// <returns>raw byte values</returns>
        public byte[] ReadRegister(Register reg, int count = 1)
        {
            EnsureValidState();
            if (count < 1 || (byte)reg + count > 13)
                throw new ArgumentOutOfRangeException(nameof(count));

            var txBuf = new[] { (byte)reg };
            var rxBuf = new byte[count];
            _device.WriteRead(txBuf, rxBuf);
            return rxBuf;
        }

        /// <summary>
        /// Write to a single register
        /// </summary>
        /// <param name="reg">Register to write</param>
        /// <param name="param">Register value</param>
        /// <returns>raw byte value</returns>
        public void WriteRegister(Register reg, byte param)
        {
            EnsureValidState();

            var txBuf = new[] { (byte)reg, param };
            _device.Write(txBuf);
        }

        /// <summary>
        /// Reads all data registers to determine current field
        /// </summary>
        /// <returns>MagneticField</returns>
        public async Task<MagneticField> GetReadingAsync()
        {
            _tcs = new TaskCompletionSource<MagneticField>();
            WriteRegister(Register.Mode, (byte)ModeReg.SingleShot);

            if (_drdy != null)
            {
                await Task.WhenAny(_tcs.Task, Task.Delay(200)).ConfigureAwait(true);
                if (_tcs.Task.IsCompleted)
                {
                    try { return _tcs.Task.Result; }
                    finally { _tcs = null; }
                }
                else
                    throw new TimeoutException();
            }


            await Task.Delay(100); // Ensure data is available.

            var r = ReadRegister(Register.ConfB, 8);
            MagneticField field = RegistersToMagneticField(r);
            return field;
            
        }


        private void EnsureValidState()
        {
            if (_device == null)
                throw new InvalidOperationException("Device not open.");
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

                    _drdy?.Dispose();
                    _drdy = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~HMC5883L() {
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
