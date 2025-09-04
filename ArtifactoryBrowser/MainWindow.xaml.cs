using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ArtifactoryBrowser.Models;
using Newtonsoft.Json;
using System.IO;

namespace ArtifactoryBrowser
{
    public partial class MainWindow : Window
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
                    baseUrl += "/";

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
    }
}
