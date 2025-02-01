using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;

namespace OhMyWindows.ViewModels
{
    public class CategoryViewModel : INotifyPropertyChanged
    {
        private bool _isExpanded;

        public string Name { get; }
        public ObservableCollection<PackageViewModel> Packages { get; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        public CategoryViewModel(string name)
        {
            Name = name;
            Packages = new ObservableCollection<PackageViewModel>();
        }

        public CategoryViewModel(string name, List<PackageViewModel> packages)
        {
            Name = name;
            Packages = new ObservableCollection<PackageViewModel>(packages ?? new List<PackageViewModel>());
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 