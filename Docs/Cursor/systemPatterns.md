# Patterns Système

## Architecture
- Architecture MVVM (Model-View-ViewModel)
- Services pour la logique métier
- Injection de dépendances
- Gestion asynchrone des opérations

## Décisions Techniques Clés
1. Utilisation de WinUI 3 pour l'interface moderne
2. Exécution parallèle limitée pour les installations
3. Stockage local des états dans JSON
4. Synchronisation avec un dépôt distant pour les packages
5. Séparation claire entre UI et logique métier

## Patterns d'Architecture
### Services
- InstallationService : Gestion des installations
- InstalledPackagesService : Suivi des packages installés
- LocalSettingsService : Configuration de l'application

### ViewModels
- PackageViewModel : Représentation d'un package
- InstallationViewModel : Gestion de l'installation
- CategoryViewModel : Gestion des catégories

### Modèles
- Package : Représentation des données de package
- Installation parallèle avec SemaphoreSlim
- Gestion des états avec INotifyPropertyChanged 