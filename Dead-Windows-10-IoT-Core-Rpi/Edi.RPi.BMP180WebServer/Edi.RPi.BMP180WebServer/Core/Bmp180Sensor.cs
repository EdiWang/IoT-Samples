using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Edi.RPi.BMP180WebServer.Core
{
    public class Bmp180Sensor: IDisposable
    {
        #region Register Addresses
        /// <summary>
        /// I2C base address of the BMP180
        /// </summary>
        private const byte BMP180_ADDR = 0x77;

        /// <summary>
        /// 8-bit I2C read address of the BMP180. For testing communications only. Returns 0x55.
        /// </summary>
        private const byte BMP180_REG_CHIPID = 0xD0;

        /// <summary>
        /// 8-bit I2C command address of the BMP180
        /// </summary>
        private const byte BMP180_REG_CONTROL = 0xF4;

        /// <summary>
        /// 8-bit I2C read address of the BMP180
        /// </summary>
        private const byte BMP180_REG_RESULT = 0xF6;

        /// <summary>
        /// 8-bit I2C command for the temperature reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_TEMPERATURE = 0x2E;

        /// <summary>
        /// 8-bit I2C command for the Ultra Low Power Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE0 = 0x34;

        /// <summary>
        /// 8-bit I2C command for the Standard Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE1 = 0x74;

        /// <summary>
        /// 8-bit I2C command for the High Resolution Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE2 = 0xB4;

        /// <summary>
        /// 8-bit I2C command for the Ultra High Resolution Mode Pressure reading of the BMP180
        /// </summary>
        private const byte BMP180_COM_PRESSURE3 = 0xF4;

        /// <summary>
        /// 8-bit I2C command for software reset the BMP180
        /// </summary>
        private const byte BMP180_COM_SOFTRESET = 0xE0;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC1 = 0xAA;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC2 = 0xAC;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC3 = 0xAE;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns> 
        private const byte BMP180_CAL_AC4 = 0xB0;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC5 = 0xB2;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_AC6 = 0xB4;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_B1 = 0xB6;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_B2 = 0xB8;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MB = 0xBA;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MC = 0xBC;

        /// <summary>
        /// 8-bit I2C calibration AC1 register for the BMP180
        /// </summary>
        /// <returns>Calibration data (16 bits)</returns>
        private const byte BMP180_CAL_MD = 0xBE;
        #endregion

        /// <summary>
        /// I2C BMP180 Barometer
        /// </summary>
        private I2cDevice _i2CBarometer;

        private Bmp180CalibrationData _calibrationData;

        private double x0, x1, x2, y0, y1, y2, p0, p1, p2;

        public bool IsInitialized { get; private set; }

        public Bmp180CalibrationData CalibrationData => _calibrationData;

        public Bmp180Sensor()
        {
            _calibrationData = new Bmp180CalibrationData();
        }

        public async Task InitializeAsync()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("The I2C BMP180 sensor is already initialized.");
            }

            // Get a selector string that will return all I2C controllers on the system
            var advancedQuerySyntaxString = I2cDevice.GetDeviceSelector();

            // Find the I2C bus controller devices with our selector string
            var controllerDeviceIds = await DeviceInformation.FindAllAsync(advancedQuerySyntaxString);

            // Ensure we have an I2C controler
            if (controllerDeviceIds == null || controllerDeviceIds.Count == 0)
            {
                throw new I2CDeviceNotFoundException();
            }
            var i2CControllerDeviceId = controllerDeviceIds[0].Id;


            // Setup the settings to address BMP180_ADDR with a 400KHz bus speed
            var i2CSettings = new I2cConnectionSettings(BMP180_ADDR);
            i2CSettings.BusSpeed = I2cBusSpeed.StandardMode;

            // Create an I2cDevice with our selected bus controller ID and I2C settings
            _i2CBarometer = await I2cDevice.FromIdAsync(i2CControllerDeviceId, i2CSettings);

            // Ensure that the 
            if (_i2CBarometer == null)
            {
                throw new I2CAddressException(i2CSettings.SlaveAddress, i2CControllerDeviceId);
            }

            ReadCalibrationData();


            IsInitialized = true;
        }

        /// <summary>
        /// Retrieve calibration data from device
        /// </summary>
        private void ReadCalibrationData()
        {
            var data = WriteRead(BMP180_CAL_AC1, 2);
            Array.Reverse(data);
            _calibrationData.AC1 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC2, 2);
            Array.Reverse(data);
            _calibrationData.AC2 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC3, 2);
            Array.Reverse(data);
            _calibrationData.AC3 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC4, 2);
            Array.Reverse(data);
            _calibrationData.AC4 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC5, 2);
            Array.Reverse(data);
            _calibrationData.AC5 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_AC6, 2);
            Array.Reverse(data);
            _calibrationData.AC6 = BitConverter.ToUInt16(data, 0);

            data = WriteRead(BMP180_CAL_B1, 2);
            Array.Reverse(data);
            _calibrationData.B1 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_B2, 2);
            Array.Reverse(data);
            _calibrationData.B2 = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MB, 2);
            Array.Reverse(data);
            _calibrationData.MB = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MC, 2);
            Array.Reverse(data);
            _calibrationData.MC = BitConverter.ToInt16(data, 0);

            data = WriteRead(BMP180_CAL_MD, 2);
            Array.Reverse(data);
            _calibrationData.MD = BitConverter.ToInt16(data, 0);


            // Compute floating-point polynominals
            var c3 = 160.0 * Math.Pow(2, -15) * _calibrationData.AC3;
            var c4 = Math.Pow(10, -3) * Math.Pow(2, -15) * _calibrationData.AC4;
            var b1 = Math.Pow(160, 2) * Math.Pow(2, -30) * _calibrationData.B1;
            var c5 = (Math.Pow(2, -15) / 160) * _calibrationData.AC5;
            var c6 = _calibrationData.AC6;
            var mc = (Math.Pow(2, 11) / Math.Pow(160, 2)) * _calibrationData.MC;
            var md = _calibrationData.MD / 160.0;
            x0 = _calibrationData.AC1;
            x1 = 160.0 * Math.Pow(2, -13) * _calibrationData.AC2;
            x2 = Math.Pow(160, 2) * Math.Pow(2, -25) * _calibrationData.B2;
            y0 = c4 * Math.Pow(2, 15);
            y1 = c4 * c3;
            y2 = c4 * b1;
            p0 = (3791.0 - 8.0) / 1600.0;
            p1 = 1.0 - 7357.0 * Math.Pow(2, -20);
            p2 = 3038.0 * 100.0 * Math.Pow(2, -36);
        }

        public async Task<byte[]> ReadUncompestatedTemperature()
        {
            var command = new[] { BMP180_REG_CONTROL, BMP180_COM_TEMPERATURE };
            _i2CBarometer.Write(command);
            await Task.Delay(5);
            return WriteRead(BMP180_REG_RESULT, 2); ;
        }

        public async Task<byte[]> ReadUncompestatedPressure(Bmp180AccuracyMode ossMode)
        {
            byte presssureCommand = 0;
            var delay = 5;

            switch (ossMode)
            {
                case Bmp180AccuracyMode.UltraLowPower:
                    presssureCommand = BMP180_COM_PRESSURE0;
                    delay = 5;
                    break;
                case Bmp180AccuracyMode.Standard:
                    presssureCommand = BMP180_COM_PRESSURE1;
                    delay = 8;
                    break;
                case Bmp180AccuracyMode.HighResolution:
                    presssureCommand = BMP180_COM_PRESSURE2;
                    delay = 14;
                    break;
                case Bmp180AccuracyMode.UltraHighResolution:
                    presssureCommand = BMP180_COM_PRESSURE3;
                    delay = 26;
                    break;
            }

            var command = new[] { BMP180_REG_CONTROL, presssureCommand };
            _i2CBarometer.Write(command);

            await Task.Delay(delay);

            return WriteRead(BMP180_REG_RESULT, 3); ;
        }

        private byte[] WriteRead(byte reg, int readLength)
        {
            var readBuffer = new byte[readLength];

            _i2CBarometer.WriteRead(new[] {reg}, readBuffer);

            return readBuffer;
        }

        private int calculateB5(int ut)
        {
            var X1 = (ut - _calibrationData.AC6) * (_calibrationData.AC5) >> 15;
            var X2 = (_calibrationData.MC << 11) / (X1 + _calibrationData.MD);
            return X1 + X2;
        }

        public async Task<Bmp180SensorData> GetSensorDataAsync(Bmp180AccuracyMode oss)
        {
            // Create the return object.
            var sensorData = new Bmp180SensorData();

            // Read the Uncompestated values from the sensor.
            var tData = await ReadUncompestatedTemperature();
            var pData = await ReadUncompestatedPressure(oss);

            // Keep raw data for debug
            sensorData.UncompestatedTemperature = tData;
            sensorData.UncompestatedPressure = pData;

            var ut = (tData[0] << 8) + tData[1];
            var up = (pData[0] * 256.0) + pData[1] + (pData[2] / 256.0);

            // Calculate real values
            var b5 = calculateB5(ut);

            var t = (b5 + 8) >> 4;
            sensorData.Temperature = t/10.0;

            var s = sensorData.Temperature - 25.0;
            var x = (x2 * Math.Pow(s, 2)) + (x1 * s) + x0;
            var y = (y2 * Math.Pow(s, 2)) + (y1 * s) + y0;
            var z = (up - x) / y;

            sensorData.Pressure = (p2 * Math.Pow(z, 2)) + (p1 * z) + p0;

            return sensorData;
        }

        public void Dispose()
        {
            _i2CBarometer.Dispose();
        }
    }

    #region Accuracy Mode
    public enum Bmp180AccuracyMode
    {
        UltraLowPower = 0,
        Standard = 1,
        HighResolution = 2,
        UltraHighResolution = 3
    }
    #endregion

    #region Calibration Data
    public class Bmp180CalibrationData
    {
        public short AC1 { get; set; }
        public short AC2 { get; set; }
        public short AC3 { get; set; }
        public ushort AC4 { get; set; }
        public ushort AC5 { get; set; }
        public ushort AC6 { get; set; }
        public short B1 { get; set; }
        public short B2 { get; set; }
        public short MB { get; set; }
        public short MC { get; set; }
        public short MD { get; set; }

        public override string ToString()
        {
            return "{ AC1: " + AC1.ToString("X") +
                   ", AC2: " + AC2.ToString("X") +
                   ", AC3: " + AC3.ToString("X") +
                   ", AC4: " + AC4.ToString("X") +
                   ", AC5: " + AC5.ToString("X") +
                   ", AC6: " + AC6.ToString("X") +
                   ", VB1: " + B1.ToString("X") +
                   ", VB2: " + B2.ToString("X") +
                   ", MB: " + MB.ToString("X") +
                   ", MC: " + MC.ToString("X") +
                   ", MD: " + MD.ToString("X") + " }";
        }
    }
    #endregion

    #region Sensor Data
    public class Bmp180SensorData
    {
        public double Temperature { get; set; }
        public double Pressure { get; set; }
        public byte[] UncompestatedTemperature { get; set; }
        public byte[] UncompestatedPressure { get; set; }
    }
    #endregion

    #region Exceptions
    internal class I2CDeviceNotFoundException : Exception
    {
        public I2CDeviceNotFoundException() :
            base("Could not find the I2C controller!")
        { }

        public I2CDeviceNotFoundException(string message) : base(message)
        { }

        public I2CDeviceNotFoundException(string message, Exception innerException) : base(message, innerException)
        { }
    }
    internal class I2CAddressException : Exception
    {
        public I2CAddressException() :
                    base("The address on I2C Controller is currently in use.")
        { }

        public I2CAddressException(string message) : base(message)
        { }

        public I2CAddressException(string message, Exception innerException) : base(message, innerException)
        { }

        public I2CAddressException(object slaveAddress, object i2CControllerDeviceId) :
            base(string.Format("The address {0} on I2C Controller {1} is currently in use.", slaveAddress, i2CControllerDeviceId))
        { }
    }
    #endregion
}
