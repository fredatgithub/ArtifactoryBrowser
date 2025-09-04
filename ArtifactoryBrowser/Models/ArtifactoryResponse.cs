using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArtifactoryBrowser.Models
{
  /// <summary>
  /// Classes pour la désérialisation des réponses de l'API Artifactory
  /// </summary>
  public class ArtifactoryResponse
  {
    [JsonProperty("children")]
    public List<ArtifactoryChild> Children { get; set; }
  }
}
