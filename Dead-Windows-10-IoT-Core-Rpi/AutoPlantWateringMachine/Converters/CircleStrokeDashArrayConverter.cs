using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace AutoPlantWateringMachine.Converters
{
    public class CircleStrokeDashArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var a1 = 197 * Math.PI * (double)value * 0.01 / 5;
            var a2 = 1000;
            return new DoubleCollection()
            {
                a1, 
                a2
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
