# Contexte Technique

## Technologies Utilisées
- .NET 6.0
- WinUI 3
- Windows App SDK
- JSON.NET pour la sérialisation
- PowerShell pour l'exécution des commandes
- Git pour le contrôle de version

## Configuration du Développement
### Prérequis
- Visual Studio 2022
- Windows App SDK
- Windows 10 SDK
- .NET 6.0 SDK
- Git

### Structure du Projet
- OhMyWindows/ : Projet principal WinUI 3
- OhMyWindows.Core/ : Bibliothèque de base
- Docs/ : Documentation
- Projet/ : Plans et tâches

## Contraintes Techniques
1. Compatible Windows 10/11
2. Nécessite des droits administrateur pour l'installation
3. Dépend de Winget et/ou Chocolatey
4. Connexion Internet requise pour la synchronisation
5. Gestion de la concurrence pour les installations parallèles 