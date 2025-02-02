using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace OhMyWindows.Converters;

public class BooleanToRotationConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isDescending)
        {
            var rotation = isDescending ? 180 : 0;
            return new RotateTransform { Angle = rotation, CenterX = 0.5, CenterY = 0.5 };
        }
        return new RotateTransform();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
