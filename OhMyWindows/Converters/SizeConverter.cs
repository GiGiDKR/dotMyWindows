using Microsoft.UI.Xaml.Data;

namespace OhMyWindows.Converters;

public class SizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is long sizeInKb)
        {
            if (sizeInKb >= 1024 * 1024) // Plus de 1 Go
            {
                return $"{sizeInKb / (1024.0 * 1024.0):F2} Go";
            }
            else if (sizeInKb >= 1024) // Plus de 1 Mo
            {
                return $"{sizeInKb / 1024.0:F2} Mo";
            }
            return $"{sizeInKb} Ko";
        }
        return "0 Ko";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
