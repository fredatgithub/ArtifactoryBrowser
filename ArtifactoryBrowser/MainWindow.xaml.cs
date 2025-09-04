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
  }
}
