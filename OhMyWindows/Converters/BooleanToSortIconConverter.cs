using Microsoft.UI.Xaml.Data;

namespace OhMyWindows.Converters;

public class BooleanToSortIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (bool)value ? "\uE74A" : "\uE74B"; // E74A = flèche haut (SortUp), E74B = flèche bas (SortDown)
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
