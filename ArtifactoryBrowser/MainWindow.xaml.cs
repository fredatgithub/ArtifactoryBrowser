using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArtifactoryBrowser.Models;
using Newtonsoft.Json;

namespace ArtifactoryBrowser
{
  public partial class MainWindow: Window
  {
    private readonly HttpClient _httpClient;

    public MainWindow()
    {
      InitializeComponent();
      _httpClient = new HttpClient();
      
      // Restaurer l'URL sauvegardée
      txtArtifactoryUrl.Text = Properties.Settings.Default.ArtifactoryUrl ?? string.Empty;
      
      // Restaurer la taille et la position de la fenêtre
      this.Left = Properties.Settings.Default.WindowLeft;
      this.Top = Properties.Settings.Default.WindowTop;
      this.Width = Properties.Settings.Default.WindowWidth;
      this.Height = Properties.Settings.Default.WindowHeight;
      this.WindowState = Properties.Settings.Default.WindowState;
      
      // S'assurer que la fenêtre est visible sur l'écran
      if (this.Left < 0 || this.Top < 0 || 
          this.Left > SystemParameters.VirtualScreenWidth || 
          this.Top > SystemParameters.VirtualScreenHeight)
      {
        this.Left = 100;
        this.Top = 100;
      }
    }

    private async void BtnSearch_Click(object sender, RoutedEventArgs e)
    {
      await SearchArtifactoryFiles();
    }

    private async void TxtArtifactoryUrl_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        await SearchArtifactoryFiles();
      }
    }

    private async Task SearchArtifactoryFiles()
    {
      string baseUrl = txtArtifactoryUrl.Text.Trim();

      if (string.IsNullOrEmpty(baseUrl))
      {
        MessageBox.Show("Veuillez entrer une URL Artifactory valide.", "Erreur",
            MessageBoxButton.OK, MessageBoxImage.Error);
        return;
      }

      try
      {
        btnSearch.IsEnabled = false;
        lblStatus.Text = "Recherche en cours...";

        // Nettoyer l'URL
        if (!baseUrl.EndsWith("/"))
        {
          baseUrl += "/";
        }

        // Créer les URLs pour les environnements
        string devUrl = new Uri(new Uri(baseUrl), "dev/").ToString();
        string prodUrl = new Uri(new Uri(baseUrl), "prod/").ToString();

        // Récupérer les fichiers en parallèle
        var devTask = GetArtifactoryFilesAsync(devUrl, "DEV");
        var prodTask = GetArtifactoryFilesAsync(prodUrl, "PROD");

        await Task.WhenAll(devTask, prodTask);

        // Mettre à jour les DataGrid
        dgvDev.ItemsSource = devTask.Result;
        dgvProd.ItemsSource = prodTask.Result;

        lblStatus.Text = $"Terminé. {devTask.Result.Count} fichiers DEV, {prodTask.Result.Count} fichiers PROD trouvés.";
      }
      catch (Exception ex)
      {
        MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur",
            MessageBoxButton.OK, MessageBoxImage.Error);
        lblStatus.Text = "Erreur lors de la recherche.";
      }
      finally
      {
        btnSearch.IsEnabled = true;
      }
    }

    private async Task<List<ArtifactoryFile>> GetArtifactoryFilesAsync(string baseUrl, string environment)
    {
      var files = new List<ArtifactoryFile>();

      try
      {
        // Construire l'URL de l'API Artifactory
        string apiUrl = $"{baseUrl}api/storage/{Uri.EscapeDataString(baseUrl)}";

        // Ajouter le header pour accepter JSON
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        // Effectuer la requête
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        // Désérialiser la réponse
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ArtifactoryResponse>(content);

        // Filtrer et formater les fichiers
        if (result?.Children != null)
        {
          foreach (var child in result.Children)
          {
            if (child.Folder == null || !child.Folder.Value)
            {
              var fileInfo = await GetFileInfoAsync($"{baseUrl.TrimEnd('/')}/{child.Uri.TrimStart('/')}");
              if (fileInfo != null)
              {
                files.Add(new ArtifactoryFile
                {
                  Name = Path.GetFileName(child.Uri.TrimStart('/')),
                  Path = child.Uri.TrimStart('/'),
                  Size = fileInfo.Size / (1024.0 * 1024.0), // Convertir en Mo
                  LastModified = fileInfo.LastModified,
                  Environment = environment
                });
              }
            }
          }
        }

        // Trier par date de modification (du plus récent au plus ancien)
        return files.OrderByDescending(f => f.LastModified).ToList();
      }
      catch (Exception ex)
      {
        // En cas d'erreur, on retourne une liste vide et on log l'erreur
        Console.WriteLine($"Erreur lors de la récupération des fichiers {environment}: {ex.Message}");
        return new List<ArtifactoryFile>();
      }
    }

    private async Task<ArtifactoryFileInfo> GetFileInfoAsync(string fileUrl)
    {
      try
      {
        string apiUrl = $"{fileUrl}?properties";
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ArtifactoryFileInfoResponse>(content);

        return new ArtifactoryFileInfo
        {
          Size = result?.DownloadCount > 0 ? result.Size : 0,
          LastModified = result?.LastModified ?? DateTime.MinValue
        };
      }
      catch
      {
        return null;
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      // Sauvegarder l'URL
      Properties.Settings.Default.ArtifactoryUrl = txtArtifactoryUrl.Text.Trim();
      
      // Sauvegarder la taille et la position de la fenêtre
      if (this.WindowState == WindowState.Normal)
      {
        Properties.Settings.Default.WindowTop = this.Top;
        Properties.Settings.Default.WindowLeft = this.Left;
        Properties.Settings.Default.WindowHeight = this.Height;
        Properties.Settings.Default.WindowWidth = this.Width;
      }
      else
      {
        // Si la fenêtre est maximisée ou minimisée, sauvegarder les valeurs restaurées
        var r = this.RestoreBounds;
        Properties.Settings.Default.WindowTop = r.Top;
        Properties.Settings.Default.WindowLeft = r.Left;
        Properties.Settings.Default.WindowHeight = r.Height;
        Properties.Settings.Default.WindowWidth = r.Width;
      }
      
      Properties.Settings.Default.WindowState = this.WindowState;
      
      // Sauvegarder tous les paramètres
      Properties.Settings.Default.Save();
    }
  }
}
