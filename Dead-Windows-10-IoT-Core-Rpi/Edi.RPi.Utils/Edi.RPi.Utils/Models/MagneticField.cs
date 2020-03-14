namespace Edi.RPi.Utils.Models
{
    /// <summary>
    /// Magnetic field in micro Tesla
    /// </summary>
    public sealed class MagneticField
    {
        public MagneticField(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public readonly double X;
        public readonly double Y;
        public readonly double Z;
        public override string ToString()
        {
            return $"MagneticField({X:f},{Y:f},{Z:f})";
        }
    }
}