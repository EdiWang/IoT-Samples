using System.Threading.Tasks;

namespace Edi.RPi.Utils.Interfaces
{
    /// <summary>
    /// Base interface for readable sensors
    /// </summary>
    public interface IReadableSensor
    {
        /// <summary>
        /// Establishes a connection to the sensor device
        /// </summary>
        /// <returns>True if connection to sensor was successful</returns>
        Task<bool> ConnectAsync();

        /// <summary>
        /// True if the device has been successfully connected
        /// </summary>
        bool Connected { get; }
    }

    /// <summary>
    /// Generic typed interface for sensor readings
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadableSensor<T> : IReadableSensor
    {
        /// <summary>
        /// Retrieves a single measurement reading from the sensor device
        /// </summary>
        /// <returns></returns>
        Task<T> GetReadingAsync();
    }
}