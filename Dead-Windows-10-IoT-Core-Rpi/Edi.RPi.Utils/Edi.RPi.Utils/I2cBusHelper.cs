using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Edi.RPi.Utils
{
    /// <summary>
    /// Helper methods for I2C bus enumeration
    /// </summary>
    public static class I2CBusHelper
    {
        public static async Task<DeviceInformation> GetI2CBusDeviceInformation()
        {
            // Get a selector string for bus:  I2C5 on MBM, I2C1 on RPi2
            string aqs1 = I2cDevice.GetDeviceSelector("I2C1");
            string aqs5 = I2cDevice.GetDeviceSelector("I2C5");

            // Find the I2C bus controller with our selector strings
            var queries = new[]
            {
                DeviceInformation.FindAllAsync(aqs5).AsTask(),
                DeviceInformation.FindAllAsync(aqs1).AsTask()
            };
            var dis = (await Task.WhenAll(queries)).FirstOrDefault(d => d.Any());

            if (dis == null || dis.Count == 0)
            {
                return null;
            }

            return dis[0];
        }
    }
}