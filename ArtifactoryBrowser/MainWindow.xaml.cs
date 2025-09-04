using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

namespace ArtifactoryBrowser
{
  public partial class MainWindow: Window
  {
    private bool _isNavigating = false;
    private string _userDataFolder;

    public MainWindow()
    {
      InitializeComponent();
      Loaded += MainWindow_Loaded;
      _userDataFolder = Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
          "ArtifactoryBrowser",
          "WebView2"
      );
      InitializeAsync();
      
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      await InitializeWebView();
    }

    private async void InitializeAsync()
    {
      try
      {
        // Configurez le dossier de données utilisateur pour WebView2
        var webView2Environment = await CoreWebView2Environment.CreateAsync(
            userDataFolder: _userDataFolder
        );

        await webView.EnsureCoreWebView2Async(webView2Environment);

        // Chargez l'URL sauvegardée ou l'URL par défaut
        if (!string.IsNullOrEmpty(Properties.Settings.Default.LastUrl))
        {
          webView.Source = new Uri(Properties.Settings.Default.LastUrl);
          txtArtifactoryUrl.Text = Properties.Settings.Default.LastUrl;
        }
        else
        {
          webView.Source = new Uri("https://www.jfrog.com/artifactory/");
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Erreur lors de l'initialisation de WebView2 : {ex.Message}",
            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
    {
      _isNavigating = true;
      UpdateUI();
      lblStatus.Text = "Chargement...";
    }

    private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
    {
      _isNavigating = false;
      UpdateUI();
      lblStatus.Text = "Prêt";

      // Mettre à jour l'URL dans la barre d'adresse
      txtArtifactoryUrl.Text = webView.Source?.ToString() ?? "";

      // Sauvegarder l'URL
      if (!string.IsNullOrEmpty(webView.Source?.ToString()))
      {
        Properties.Settings.Default.LastUrl = webView.Source.ToString();
        Properties.Settings.Default.Save();
      }
    }

    private void WebView_CoreWebView2Initialized(object sender, CoreWebView2InitializedEventArgs e)
    {
      // Configuration supplémentaire de WebView2
      if (webView.CoreWebView2 != null)
      {
        // Activer les outils de développement (F12)
        webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
      }
    }

    private async void BtnGo_Click(object sender, RoutedEventArgs e)
    {
      await NavigateToUrl(txtArtifactoryUrl.Text.Trim());
    }

    private async void TxtArtifactoryUrl_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        await NavigateToUrl(txtArtifactoryUrl.Text.Trim());
      }
    }

    private async Task NavigateToUrl(string url)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(url))
        {
          return;
        }

        // Ajouter le protocole si manquant
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
        {
          url = "https://" + url;
        }

        await webView.EnsureCoreWebView2Async();
        webView.Source = new Uri(url);
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Impossible de charger l'URL : {ex.Message}",
            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
      if (webView.CanGoBack)
      {
        webView.GoBack();
      }
    }

    private void BtnForward_Click(object sender, RoutedEventArgs e)
    {
      if (webView.CanGoForward)
      {
        webView.GoForward();
      }
    }

    private void BtnRefresh_Click(object sender, RoutedEventArgs e)
    {
      webView.Reload();
    }

    private void UpdateUI()
    {
      btnBack.IsEnabled = webView.CanGoBack;
      btnForward.IsEnabled = webView.CanGoForward;
      btnGo.IsEnabled = !_isNavigating;
      btnRefresh.IsEnabled = !_isNavigating;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // Sauvegarder la taille et la position de la fenêtre
      if (WindowState == WindowState.Normal)
      {
        Properties.Settings.Default.WindowTop = Top;
        Properties.Settings.Default.WindowLeft = Left;
        Properties.Settings.Default.WindowHeight = Height;
        Properties.Settings.Default.WindowWidth = Width;
      }
      else
      {
        var r = RestoreBounds;
        Properties.Settings.Default.WindowTop = r.Top;
        Properties.Settings.Default.WindowLeft = r.Left;
        Properties.Settings.Default.WindowHeight = r.Height;
        Properties.Settings.Default.WindowWidth = r.Width;
      }

      Properties.Settings.Default.WindowState = WindowState;
      Properties.Settings.Default.Save();
    }

    private async void InitializeWebView()
    {
      try
      {
        // Créer un dossier pour les données utilisateur de WebView2
        var userDataFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ArtifactoryBrowser",
            "WebView2"
        );

        // Configurer l'environnement WebView2
        var webView2Environment = await CoreWebView2Environment.CreateAsync(
            userDataFolder: userDataFolder
        );

        // Initialiser le contrôle WebView2
        await webView.EnsureCoreWebView2Async(webView2Environment);

        // Configurer des paramètres supplémentaires
        webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;

        // Charger l'URL sauvegardée ou l'URL par défaut
        if (!string.IsNullOrEmpty(Properties.Settings.Default.LastUrl))
        {
          webView.Source = new Uri(Properties.Settings.Default.LastUrl);
          txtArtifactoryUrl.Text = Properties.Settings.Default.LastUrl;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Erreur lors de l'initialisation de WebView2 : {ex.Message}",
            "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }
  }
}