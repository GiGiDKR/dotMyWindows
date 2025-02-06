using System;
using System.ComponentModel;
using OhMyWindows.Models;

namespace OhMyWindows.ViewModels;
    /// <summary>
    /// ViewModel pour représenter un package dans la vue.
    /// Gère l'état (installation, sélection) et notifie les changements.
    /// </summary>
    public class PackageViewModel : INotifyPropertyChanged
    {
        private bool isSelected;
        private bool isInstalled;
        private readonly Package _package;

        public PackageViewModel(Package package, bool isInstalled = false)
        {
            _package = package;
            Id = package.Id;
            Name = package.Name;
            Source = package.Source;
            Category = package.Category;
            this.isInstalled = isInstalled;
        }
        /// Package modèle sous-jacent.
        /// </summary>
        public Package Package => _package;

        /// <summary>
        /// Identifiant du package.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Nom du package.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Source ou provenance du package.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Catégorie du package.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Indique si le package est installé.
        /// </summary>
        public bool IsInstalled
        {
            get => isInstalled;
            set
            {
                if (isInstalled != value)
                {
                    isInstalled = value;
                    OnPropertyChanged(nameof(IsInstalled));
                }
            }
        }

        /// <summary>
        /// Indique si le package est sélectionné dans la vue.
        /// Notifie le changement lorsqu'il est modifié.
        /// </summary>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (isSelected != value)
                {
                    isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Méthode pour notifier que la propriété a changé.
        /// </summary>
        /// <param name="propertyName">Nom de la propriété modifiée.</param>
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
