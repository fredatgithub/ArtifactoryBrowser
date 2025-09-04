# Artifactory Browser

Application WPF en C# pour naviguer et lister les fichiers d'un dépôt Artifactory.

## Fonctionnalités

- **Navigation dans les dépôts** : Parcourez facilement les dépôts Artifactory
- **Support multi-environnements** : Affichez les fichiers des environnements DEV et PROD côte à côte
- **Mémorisation des préférences** : L'application se souvient de vos paramètres entre les sessions :
  - Dernière URL Artifactory utilisée
  - Taille et position de la fenêtre
  - État de la fenêtre (normal, maximisé, etc.)
- **Interface utilisateur intuitive** : Affichage clair des fichiers avec leurs métadonnées

## Prérequis

- .NET Framework 4.8
- Accès à une instance Artifactory
- Droits de lecture sur les dépôts à consulter

## Installation

1. Clonez ce dépôt
2. Ouvrez la solution `ArtifactoryBrowser.sln` dans Visual Studio 2019 ou ultérieur
3. Compilez la solution (F6)
4. Exécutez l'application (F5)

## Utilisation

1. **Connexion à Artifactory** :
   - Entrez l'URL de base de votre instance Artifactory dans la barre d'adresse
   - Appuyez sur Entrée ou cliquez sur le bouton "Rechercher"

2. **Navigation** :
   - Les fichiers des environnements DEV et PROD s'affichent dans des grilles séparées
   - Les colonnes sont triables en cliquant sur les en-têtes
   - Les fichiers sont triés par date de modification (du plus récent au plus ancien)

3. **Options** :
   - Double-cliquez sur un fichier pour l'ouvrir (si l'application associée est installée)
   - Redimensionnez la fenêtre ou maximisez-la selon vos préférences
   - Les paramètres sont automatiquement sauvegardés à la fermeture

## Fonctionnement technique

### Structure du projet

- **MainWindow.xaml** : Interface utilisateur principale
- **MainWindow.xaml.cs** : Logique de l'application
- **Models/ArtifactoryFile.cs** : Modèle de données pour les fichiers Artifactory
- **Properties/Settings.settings** : Configuration de l'application

### API Artifactory

L'application utilise l'API REST d'Artifactory pour :
- Récupérer la liste des fichiers d'un dépôt
- Obtenir les métadonnées des fichiers (taille, date de modification, etc.)

### Sécurité

- Les paramètres utilisateur sont stockés localement dans `%APPDATA%\Local\ArtifactoryBrowser`
- Les informations d'authentification (si utilisées) sont gérées par le système de stockage sécurisé de Windows

## Personnalisation

### Paramètres

Les paramètres de l'application sont stockés dans :
- `Properties\Settings.settings` pour les paramètres par défaut
- `%APPDATA%\Local\ArtifactoryBrowser` pour les préférences utilisateur

### Thèmes

Pour modifier l'apparence de l'application, éditez le fichier `App.xaml` et ajoutez un nouveau style ou un thème.

## Dépannage

### Problèmes courants

- **Connexion impossible** : Vérifiez l'URL et vos droits d'accès
- **Données non mises à jour** : Cliquez sur le bouton "Rechercher" pour forcer le rafraîchissement
- **Problèmes d'affichage** : Redémarrez l'application pour réinitialiser la mise en page

### Journaux

Les journaux d'erreur sont disponibles dans la sortie de débogage de Visual Studio.

## Licence

Ce projet est sous licence MIT. Voir le fichier `LICENSE` pour plus de détails.

## Auteur

Freddy Juhel

## Remerciements

- [JFrog Artifactory](https://jfrog.com/artifactory/)
- [Newtonsoft.Json](https://www.newtonsoft.com/json) pour la sérialisation JSON
- [.NET Framework](https://dotnet.microsoft.com/)