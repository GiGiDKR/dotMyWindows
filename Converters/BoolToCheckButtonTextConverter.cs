using Microsoft.UI.Xaml.Data;

namespace OhMyWindows.Converters;

public class BoolToCheckButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? "Stopper" : "Vérifier";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 