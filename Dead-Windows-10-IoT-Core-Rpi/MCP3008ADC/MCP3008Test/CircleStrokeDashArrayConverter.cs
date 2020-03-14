using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace MCP3008Test
{
    public class CircleStrokeDashArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // 197π × 30% ÷ 3 = 61.889

            var a1 = 197 * Math.PI * (double)value * 0.01 / 3;
            var a2 = 1000;
            return new DoubleCollection()
            {
                a1, 
                a2
            };

            //Shape shape = (Shape)value;
            //double segNum = double.Parse(parameter.ToString());

            //double offset = shape.StrokeDashOffset;
            //double width = shape.Width;
            //double thickness = shape.StrokeThickness;

            //double visibleLen = offset * 2;
            //double length = (width - thickness) * Math.PI / segNum / thickness;

            ////return new DoubleCollection(new[] { visibleLen, (length - visibleLen) });

            //return new DoubleCollection()
            //{
            //   visibleLen,
            //   length - visibleLen
            //};
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
