using Microsoft.UI.Xaml.Data;

namespace OhMyWindows.Converters;

public class DateConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string dateStr && !string.IsNullOrWhiteSpace(dateStr))
        {
            // Format de date du registre Windows : yyyyMMdd
            if (dateStr.Length == 8 && long.TryParse(dateStr, out long numericDate))
            {
                int year = (int)(numericDate / 10000);
                int month = (int)((numericDate % 10000) / 100);
                int day = (int)(numericDate % 100);

                try
                {
                    var date = new DateTime(year, month, day);
                    return date.ToString("dd/MM/yyyy");
                }
                catch
                {
                    return string.Empty;
                }
            }
            // Essayer le parsing standard si ce n'est pas le format du registre
            else if (DateTime.TryParse(dateStr, out DateTime date))
            {
                return date.ToString("dd/MM/yyyy");
            }
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
