using Microsoft.UI.Xaml.Data;
using OhMyWindows.ViewModels;

namespace OhMyWindows.Converters;

public class BoolToCheckCommandConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is InstallationViewModel viewModel)
        {
            return (bool)value ? viewModel.StopCheckingCommand : viewModel.CheckInstalledCommand;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
} 