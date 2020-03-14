using System;

namespace Edi.RPi.Utils.Interfaces
{
    public interface INotifyingSensor<T>: IReadableSensor<T>
    {
        /// <summary>
        /// Invoked when the <see cref="INotifyingSensor{T}.LastReading"/> property changes, either by continuous measurements or <see cref="IReadableSensor{T}.GetReadingAsync"/> method call.
        /// </summary>
        event EventHandler<T> ReadingChanged;

        /// <summary>
        /// Latest measured value
        /// </summary>
        T LastReading { get; }
    }
}