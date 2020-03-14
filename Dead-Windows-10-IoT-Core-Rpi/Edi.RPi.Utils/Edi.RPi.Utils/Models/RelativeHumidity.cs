using System;

namespace Edi.RPi.Utils.Models
{
    /// <summary>
    /// Relative humidity data model
    /// </summary>
    public class RelativeHumidity
    {
        public readonly double Temperature;
        public readonly double Humidity;

        public double DewPoint
        {
            get
            {
                double b, c, d;
                d = 234.5;
                if (Temperature < 0)
                {
                    b = 17.368;
                    c = 238.88;

                }
                else
                {
                    b = 17.966;
                    c = 247.15;
                }
                var gamma = Math.Log(Humidity/100 * Math.Exp((b-Temperature/d)*(Temperature/(c+Temperature))));
                return c*gamma/(b-gamma);
            }
        }

        public RelativeHumidity(double temperature, double humidity)
        {
            Temperature = temperature;
            Humidity = humidity;
        }

        public override string ToString()
        {
            return $"RelativeHumidity({Temperature:f}, {Humidity:f})";
        }
    }
}