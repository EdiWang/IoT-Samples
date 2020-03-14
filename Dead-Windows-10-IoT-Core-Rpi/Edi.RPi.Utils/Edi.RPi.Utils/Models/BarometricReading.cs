using System;

namespace Edi.RPi.Utils.Models
{
    public class BarometricReading
    {
        public readonly double Pressure;
        public readonly double Temperature;
        public BarometricReading(double pressure, double temperature)
        {
            Pressure = pressure;
            Temperature = temperature;
        }
        public double ToSealevelPressure(double altitude)
        {
            return Pressure / Math.Pow(1-altitude/44330.0, 5.255);
        }

        public double ToAltitude(double seaLevelPressure)
        {
            return 44330 * (1 - Math.Pow(Pressure / seaLevelPressure, 1.0/5.255));
        }

        public override string ToString()
        {
            return $"BarometricReading({Pressure:f}, {Temperature:f})";
        }
    }
}