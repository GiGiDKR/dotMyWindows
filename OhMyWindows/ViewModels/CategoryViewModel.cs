using System.Collections.ObjectModel;

namespace OhMyWindows.ViewModels
{
    public class CategoryViewModel
    {
        public string Name { get; }
        public ObservableCollection<PackageViewModel> Packages { get; }

        public CategoryViewModel(string name)
        {
            Name = name;
            Packages = new ObservableCollection<PackageViewModel>();
        }
    }
} 