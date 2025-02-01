# Patterns Système

## Architecture Générale
- Architecture MVVM (Model-View-ViewModel)
- Services pour la logique métier
- ViewModels pour la logique de présentation
- Vues XAML pour l'interface utilisateur

## Services Principaux

### PowerShellVersionService
- Vérifie la version de PowerShell la plus récente installée
- Retourne le chemin approprié (pwsh.exe, powershell.exe, ou cmd.exe)
- Utilisé pour garantir la compatibilité des commandes PowerShell

### InstallationService
- Gère le téléchargement et la lecture du fichier packages.json
- S'intègre avec PowerShellVersionService pour l'exécution des commandes
- Utilise les gestionnaires de paquets (winget, choco)
- Journalise toutes les opérations

### Autres Services
- NavigationService : Gestion de la navigation
- ThemeSelectorService : Gestion des thèmes
- LocalSettingsService : Gestion des paramètres locaux

## Modèles de Données
- Package : Représente un paquet installable
- PackageList : Collection de paquets avec métadonnées

## Gestion des Commandes
- Utilisation de ProcessStartInfo pour l'exécution des commandes
- Redirection des sorties standard et d'erreur
- Gestion asynchrone avec CancellationToken

## Logging
- Système de journalisation centralisé dans InstallationService
- Horodatage des événements
- Capture des erreurs et des sorties de commande

## Gestion des Erreurs
- Try-catch systématique
- Logging des exceptions
- Messages d'erreur utilisateur appropriés

## Points d'Extension
- Support prévu pour d'autres gestionnaires de paquets
- Architecture extensible pour les futures fonctionnalités
