using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Edi.RPi.Utils.Interfaces;

namespace Edi.RPi.Utils.Sensors
{

    /// <summary>
    /// Rohm BH1750FVI ambient light sensor
    /// </summary>
    public class Bh1750Fvi : IDisposable, INotifyingSensor<double>
    {
        /// <summary>
        /// Direct OpCodes of the BH1750FVI chip
        /// </summary>
        public enum Commands
        {
            PowerDown = 0,
            PowerOn = 1,
            Reset = 0x7,
            ContReadH = 0x10,
            ContReadH2 = 0x11,
            ContReadL = 0x13,
            ReadH = 0x20,
            ReadH2 = 0x21,
            ReadL = 0x23,
            /// <summary>
            /// Low 3 bits are bits 765 of MT register
            /// </summary>
            WriteMtReg765 = 0x40,
            /// <summary>
            /// Low 5 bits are bits 43210 of MT register
            /// </summary>
            WriteMtRegLow = 0x60,
        }

        /// <summary>
        /// Invoked when the <see cref="LastReading"/> property changes, either by continuous measurements or <see cref="GetReadingAsync"/> method call.
        /// </summary>
        public event EventHandler<double> ReadingChanged;
        private void OnLuxChanged(double lux)
        {
            ReadingChanged?.Invoke(this, lux);
        }

        /// <summary>
        /// Supported measurement Modes
        /// </summary>
        public enum Resolution { Low, High, VeryHigh }

        private readonly int _address;
        private I2cDevice _device;
        private bool _powerState;
        private bool _continuous;
        private int _mTime = 69;
        private Timer _timer;
        private Resolution _mode;
        private double _lastReading;
        private int _period = 1000;

        public Bh1750Fvi(bool addrLow = true)
        {
            var hasI2C = Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.I2c.I2cDevice");
            if (!hasI2C)
                throw new NotSupportedException();

            _address = addrLow ? 0x23 : 0x5C;
        }

        /// <summary>
        /// Latest measured value in lux
        /// </summary>
        public double LastReading
        {
            get
            {
                return _lastReading;
            }
            private set {
                if (Math.Abs(value - _lastReading) < double.Epsilon) return;
                _lastReading = value;
                OnLuxChanged(_lastReading);
            }
        }

        /// <summary>
        /// Triggers a single measurement
        /// </summary>
        /// <remarks>
        /// If not <see cref="Powered"/>, turns power automatically on. Chip powers down after a one-shot measurement.
        /// Returns latest value if continuous measurement is on without triggering a new one-shot measurement.
        /// </remarks>
        /// <returns>Illumination in lux</returns>
        public async Task<double> GetReadingAsync()
        {
            if (_device == null)
                await ConnectAsync();

            if (_continuous)
                return _lastReading;
            Powered = true;
            switch (Mode)
            {
                case Resolution.Low:
                    SendCommand(Commands.ReadL);
                    break;
                case Resolution.High:
                    SendCommand(Commands.ReadH);
                    break;
                case Resolution.VeryHigh:
                    SendCommand(Commands.ReadH2);
                    break;
            }
            await Task.Delay(Mode == Resolution.Low? 24:180); // L mode 16ms typ, 24ms max; H/H2 mode 160ms typ, 180ms pax

            var count = ReadUInt();
            Debug.WriteLine("Read counts: " + count);

            _powerState = false; // Chip powers off automatically after one-shot measurement

            LastReading = CountToLux(count);

            return _lastReading;

        }

        /// <summary>
        /// Controls power state of the chip
        /// </summary>
        public bool Powered {
            get { return _powerState; }
            set
            {
                if (value == _powerState) return;
                
                _powerState = value;
                SendCommand(_powerState ? Commands.PowerOn : Commands.PowerDown);
            }
        }


        /// <summary>
        /// Measurement mode
        /// </summary>
        /// <remarks>
        /// Low has 4lx resolution at upto 65k lx, measurement time is under 30ms.
        /// High and VeryHigh have variable resolution (H2 = H/2), measurement time 100-200ms.
        /// <seealso cref="ContinuousPeriod"/>
        /// </remarks>
        public Resolution Mode
        {
            get { return _mode; }
            set
            {
                if (value == _mode) return;
                _mode = value;
                if (_continuous)
                {
                    ContinuousMeasurement = false;
                    ContinuousMeasurement = true;
                }
            }
        }

        /// <summary>
        /// Start running continuous measurements, poll every <see cref="ContinuousPeriod"/> ms, 
        /// using <see cref="Mode"/> resolution. Triggers <see cref="ReadingChanged"/> when measured value changes.
        /// </summary>
        public bool ContinuousMeasurement
        {
            get { return _continuous; }
            set {
                if (value == _continuous) return;

                _continuous = value;
                if (_continuous)
                {
                    StartContinousMeasurement();
                    _timer = new Timer(a => 
                    {
                        try {
                            var count = ReadUInt();
                            LastReading = CountToLux(count);
                        }
                        catch { Debug.WriteLine("Reading a luminance value failed."); }
                    }, null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(_period));
                }
                else
                {
                    _timer.Dispose();
                    _timer = null;
                    Powered = false;
                }
            }
        }

        /// <summary>
        /// Milliseconds between measurement readings, value >= 10
        /// </summary>
        public int ContinuousPeriod {
            get { return _period; }
            set
            {
                if (value < 10) throw new ArgumentOutOfRangeException(nameof(value), "value=>10");
                if (value == _period) return;
                _period = value;
                if (_continuous)
                {
                    ContinuousMeasurement = false;
                    ContinuousMeasurement = true;
                }
            }
        }

        private void StartContinousMeasurement()
        {
            Powered = true;
            switch (Mode)
            {
                case Resolution.High:
                    SendCommand(Commands.ContReadH);
                    break;
                case Resolution.VeryHigh:
                    SendCommand(Commands.ContReadH2);
                    break;
                case Resolution.Low:
                    SendCommand(Commands.ContReadL);
                    break;
            }
        }

        /// <summary>
        /// Measurement Time register for H and VH modes, [31..254]
        /// </summary>
        public int MeasurementTime {
            get { return _mTime; }
            set {
                if (value == _mTime) return;
                if (value < 31 || value > 254) throw new ArgumentOutOfRangeException(nameof(value), "31<=value<=254");

                _mTime = value;
                WriteMtReg((byte)_mTime);
            } 
        }


        private double CountToLux(uint counts)
        {
            return counts/1.2*(69.0/_mTime)/(Mode==Resolution.VeryHigh?2.0:1.0);
        }

        #region Low level I2C controls

        /// <summary>
        /// Connects to the devices
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

                Powered = true;
                WriteMtReg((byte) _mTime);
                Powered = false;
                Connected = true;
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Initialization failed: "+ex);
            }
            return Connected;
        }

        public bool Connected { get; private set; }

        /// <summary>
        /// Sends an op code to the chip
        /// </summary>
        /// <param name="cmd"><see cref="Commands"/></param>
        /// <exception cref="FileNotFoundException">If device is not responding</exception>
        private void SendCommand(Commands cmd)
        {
            Debug.WriteLine("SendCommand " + cmd);
            if (_device == null) throw new InvalidOperationException("Device not yet initialized");

            byte[] writeBuf = { (byte)cmd };
            _device.Write(writeBuf);
        }

        private void WriteMtReg(byte value)
        {
            Debug.WriteLine("WriteMTReg " + value);
            if (_device == null) throw new InvalidOperationException("Device not yet initialized");

            byte[] writeBuf = { (byte)((byte)Commands.WriteMtReg765 | value>>5)};
            _device.Write(writeBuf);

            writeBuf[0] = (byte)((byte)Commands.WriteMtRegLow | (value&0x1f));
            _device.Write(writeBuf);

        }

        /// <summary>
        /// Reads an integer from the chip
        /// </summary>
        /// <returns>result</returns>
        /// <exception cref="FileNotFoundException">If device is not responding</exception>
        private UInt16 ReadUInt()
        {
            if (_device == null) throw new InvalidOperationException("Device not yet initialized");

            byte[] readBuf = { 0, 0 };
            _device.Read(readBuf);
            Debug.WriteLine($"Read bytes {readBuf[0]:x02} {readBuf[1]:x02}");
            return (UInt16)readBuf.BeToUInt16();
        }
        #endregion

        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls
        

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_device != null)
                    {
                        ContinuousMeasurement = false;
                        Powered = false;
                        _device.Dispose();
                        _device = null;
                    }
                }
                
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~BH1750FVI() {
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
