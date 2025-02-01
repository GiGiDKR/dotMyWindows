# Plan d'implémentation de la gestion des packages dans OhMyWindows - Taches pour LLM Assistant

**Directives :**

*   Exécuter les modifications un module à la fois.
*   Après chaque module, demander à l'utilisateur de tester en lui indiquant précisément ce qu'il doit tester.
*   Attendre la validation de l'utilisateur que le test du module est concluant avant de passer au module suivant.
*   Modifier (en ajoutant, sans rien supprimer) ce fichier `plan/Taches.md` si de nouvelles fonctionnalités sont nécessaires ou si des problèmes dans le plan sont détectés.

---

1.  **Module 1 : Vérification du modèle `Package`**
    *   Objectif : Vérifier et adapter si nécessaire la classe `Package` dans le dossier `Models` pour représenter un package (Nom, ID, Source).
    *   Fichiers à créer/modifier : `OhMyWindows/Models/Package.cs` (déjà existant, vérifier et adapter si nécessaire).
    *   Instructions de test pour l'utilisateur : Vérifier que la classe `Package` contient les propriétés `Name`, `Id` et `Source` avec les types appropriés.

2.  **Module 2 : Création du service `InstalledPackagesService`**
    *   Objectif : Créer le service `InstalledPackagesService` dans le dossier `Services` pour gérer la persistance locale des packages installés.
    *   Fichiers à créer/modifier : `OhMyWindows/Services/InstalledPackagesService.cs` (à créer).
    *   Instructions de test pour l'utilisateur :
        *   Créer une instance de `InstalledPackagesService`.
        *   Utiliser `SetPackageInstalled` pour marquer un package comme installé et vérifier que l'état est correctement sauvegardé dans `installed_packages.json`.
        *   Utiliser `IsPackageInstalled` pour vérifier si un package est marqué comme installé.

3.  **Module 3 : Création du service `InstallationService`**
    *   Objectif : Créer le service `InstallationService` dans le dossier `Services` pour gérer le téléchargement de `packages.json`, la lecture des packages et l'installation via `winget` et `choco`.
    *   Fichiers à créer/modifier : `OhMyWindows/Services/InstallationService.cs` (à créer).
    *   **Informations importantes :**
        *   Le fichier `packages.json` sera téléchargé depuis GitHub à l'URL suivante : `https://raw.githubusercontent.com/GiGiDKR/OhMyWindows/{branch}/files/packages.json`.
        *   Les branches suivantes seront testées dans l'ordre jusqu'à ce que le téléchargement réussisse : `alpha`, `dev`, `main`, `master`.
        *   Une fois téléchargé (ou si le téléchargement échoue, une version par défaut sera utilisée), le fichier `packages.json` sera stocké localement dans le dossier `Data` à l'intérieur du dossier de l'application, au chemin suivant : `[Chemin de base de l'application]/Data/packages.json`.
        *   Ce chemin local sera utilisé par l'application pour lire la liste des packages.
    *   Instructions de test pour l'utilisateur :
        *   Créer une instance de `InstallationService`.
        *   Appeler `InitializeAsync` pour télécharger ou charger `packages.json`. Vérifier que le fichier est correctement chargé et que la liste des packages peut être récupérée avec `GetPackagesAsync`.
        *   (Test d'installation sera fait dans un module ultérieur).

4.  **Module 4 : Création du `PackageViewModel`**
    *   Objectif : Créer le `PackageViewModel` dans le dossier `ViewModels` pour représenter un package dans la vue et gérer son état (sélection, installation).
    *   Fichiers à créer/modifier : `OhMyWindows/ViewModels/PackageViewModel.cs` (à créer).
    *   Instructions de test pour l'utilisateur :
        *   Créer une instance de `PackageViewModel`.
        *   Vérifier que le `PackageViewModel` contient les propriétés `Name`, `Id`, `Source`, `IsInstalled`, `IsSelected` et la logique de notification de changement de sélection.

5.  **Module 5 : Vérification du `InstallationViewModel`**
    *   Objectif : Vérifier et adapter si nécessaire le `InstallationViewModel` dans le dossier `ViewModels` pour gérer la liste des packages, les commandes et l'interaction avec les services. **Implémenter l'exécution parallèle limitée pour la vérification du statut et l'installation.**
    *   Fichiers à créer/modifier : `OhMyWindows/ViewModels/InstallationViewModel.cs` (déjà existant, vérifier et adapter si nécessaire).
    *   **Logique d'exécution parallèle :**
        *   Utiliser `SemaphoreSlim` pour limiter les tâches concurrentes.
        *   Créer une file d'attente de tâches pour la vérification et l'installation.
        *   Acquérir un slot du `SemaphoreSlim` avant d'exécuter chaque tâche et le libérer après.
        *   Utiliser `CancellationTokenSource` pour l'annulation.
        *   Utiliser `IProgress` pour le rapport de progression.
    *   Instructions de test pour l'utilisateur :
        *   Créer une instance de `InstallationViewModel`.
        *   Vérifier que le `InstallationViewModel` contient les commandes `CheckStatusCommand`, `InstallSelectedCommand`, `StopCheckCommand`, `ResetCommand`, `SelectAllCommand` et les propriétés nécessaires pour la vue.
        *   Vérifier que la vérification du statut et l'installation des packages s'effectuent en parallèle, avec une limite par défaut de 3 exécutions simultanées.
        *   (Intégration avec les services et tests des commandes dans les modules suivants).

6.  **Module 6 bis : Implémentation de la catégorisation des packages et UI Expanders**
    *   Objectif : Modifier le modèle `Package`, le `InstallationViewModel` et la `InstallationPage.xaml` pour supporter la catégorisation des packages et l'affichage via des Expanders.
    *   Fichiers à créer/modifier :
        *   `OhMyWindows/Models/Package.cs` : Ajouter une propriété `Category` pour la catégorie du package.
        *   `OhMyWindows/ViewModels/InstallationViewModel.cs` : Modifier la collection `Packages` pour gérer les packages par catégorie. Adapter la logique pour regrouper les packages par catégorie.
        *   `OhMyWindows/Views/InstallationPage.xaml` : Modifier l'interface utilisateur pour utiliser des Expanders pour afficher les catégories et les packages à l'intérieur de chaque Expander.
    *   Instructions de test pour l'utilisateur :
        *   Vérifier que le fichier `packages.json` (ou la version par défaut) contient des informations de catégorie pour les packages.
        *   Vérifier que la `InstallationPage` affiche les catégories sous forme d'Expanders.
        *   Vérifier que chaque Expander contient la liste des packages de sa catégorie avec des checkboxes.
        *   Vérifier que la sélection des checkboxes fonctionne correctement.

7.  **Module 7 : Vérification de la vue `InstallationPage`**
    *   Objectif : Vérifier et adapter si nécessaire la vue `InstallationPage.xaml` et son code-behind `InstallationPage.xaml.cs` dans le dossier `Views` pour afficher la liste des packages et permettre l'interaction utilisateur.
    *   Fichiers à créer/modifier : `OhMyWindows/Views/InstallationPage.xaml`, `OhMyWindows/Views/InstallationPage.xaml.cs` (déjà existants, vérifier et adapter si nécessaire).
    *   Instructions de test pour l'utilisateur :
        *   Naviguer vers la `InstallationPage` (intégration navigation dans module suivant).
        *   Vérifier que la page affiche les catégories sous forme d'expanders contenant une liste de packages (même vide pour le moment).
        *   Vérifier la présence des boutons et des éléments de l'interface utilisateur.

8.  **Module 8 : Intégration de la navigation**
    *   Objectif : Intégrer la `InstallationPage` dans la navigation de l'application pour la rendre accessible à l'utilisateur.
    *   Fichiers à créer/modifier : `OhMyWindows/Views/ShellPage.xaml`, `OhMyWindows/ViewModels/ShellViewModel.cs`.
    *   Instructions de test pour l'utilisateur :
        *   Démarrer l'application.
        *   Vérifier qu'un élément de navigation vers la page "Installation des packages" est présent dans le menu de navigation.
        *   Cliquer sur cet élément et vérifier que la `InstallationPage` s'affiche correctement.

9.  **Module 9 : Test de la vérification du statut des packages**
    *   Objectif : Implémenter et tester la fonctionnalité de vérification du statut des packages installés.
    *   Fichiers à modifier : `OhMyWindows/ViewModels/InstallationViewModel.cs`, `OhMyWindows/Services/InstallationService.cs`, `OhMyWindows/Views/InstallationPage.xaml`.
    *   Instructions de test pour l'utilisateur :
        *   Naviguer vers la `InstallationPage`.
        *   Cliquer sur le bouton "Vérifier".
        *   Vérifier que l'état d'installation des packages est mis à jour dans l'interface utilisateur (les packages installés doivent être décochés et marqués comme installés).
        *   Vérifier la barre de progression et le texte de statut pendant la vérification.
        *   Vérifier que la vérification du statut s'effectue en parallèle (tester avec plusieurs packages).
        *   Modifier la limite d'exécution parallèle dans le code (temporairement) et vérifier que la limite est bien prise en compte.

10. **Module 10 : Test de l'installation des packages**
    *   Objectif : Implémenter et tester la fonctionnalité d'installation des packages sélectionnés.
    *   Fichiers à modifier : `OhMyWindows/ViewModels/InstallationViewModel.cs`, `OhMyWindows/Services/InstallationService.cs`, `OhMyWindows/Views/InstallationPage.xaml`.
    *   Instructions de test pour l'utilisateur :
        *   Naviguer vers la `InstallationPage`.
        *   Sélectionner quelques packages à installer.
        *   Cliquer sur le bouton "Installer".
        *   Vérifier que l'installation des packages sélectionnés démarre.
        *   Vérifier la barre de progression et le texte de statut pendant l'installation.
        *   Après l'installation, vérifier que les packages installés sont marqués comme installés et décochés.
        *   (Optionnel) Vérifier que les packages sont réellement installés sur le système.
        *   Vérifier que l'installation des packages s'effectue en parallèle (tester avec plusieurs packages).
        *   Modifier la limite d'exécution parallèle dans le code (temporairement) et vérifier que la limite est bien prise en compte.

11. **Module 11 : Création de l'option de configuration dans `SettingsPage`**
    *   Objectif : Ajouter une option dans la page `SettingsPage` pour permettre à l'utilisateur de configurer le nombre maximal d'exécutions parallèles pour la vérification du statut et l'installation des packages.
    *   Fichiers à créer/modifier :
        *   `OhMyWindows/Services/LocalSettingsService.cs` : Modifier `SaveSettingAsync` et `ReadSettingAsync` pour gérer le paramètre `MaxParallelTasks`. Ajouter une constante pour la clé du paramètre (`MaxParallelTasksSettingKey`).
        *   `OhMyWindows/ViewModels/SettingsViewModel.cs` : Ajouter une propriété `MaxParallelTasks` (int) et la logique pour charger et sauvegarder la valeur via `LocalSettingsService`.
        *   `OhMyWindows/Views/SettingsPage.xaml` : Ajouter un `NumberBox` ou `Slider` lié à la propriété `MaxParallelTasks` du `SettingsViewModel`.
    *   Instructions de test pour l'utilisateur :
        *   Naviguer vers la `SettingsPage`.
        *   Vérifier la présence du nouveau contrôle pour configurer la limite d'exécution parallèle.
        *   Modifier la valeur du contrôle et vérifier que la nouvelle valeur est sauvegardée et rechargée correctement après redémarrage de l'application.
        *   Naviguer vers la `InstallationPage` et vérifier que la limite d'exécution parallèle configurée dans les paramètres est bien prise en compte lors de la vérification du statut et de l'installation des packages.

12.  **Module 12 : Améliorations et gestion des erreurs**
    *   Objectif : Implémenter les améliorations suggérées (gestion des erreurs, indication de progression plus précise, etc.) et améliorer la robustesse de la gestion des packages.
    *   Fichiers à modifier : Tous les fichiers concernés par la gestion des packages.
    *   Instructions de test pour l'utilisateur :
        *   Tester les cas d'erreur possibles (erreur de téléchargement de `packages.json`, erreur d'installation d'un package, etc.) et vérifier que des messages d'erreur clairs sont affichés à l'utilisateur.
        *   Vérifier que l'indication de progression est précise et informative.
        *   Tester les autres améliorations implémentées.

**Validation du plan**

Veuillez valider ce plan de tâches afin que je puisse commencer l'implémentation module par module, en commençant par le module 1.