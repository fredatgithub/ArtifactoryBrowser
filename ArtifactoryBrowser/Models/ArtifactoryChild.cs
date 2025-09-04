using Newtonsoft.Json;

namespace ArtifactoryBrowser.Models
{
  public class ArtifactoryChild
  {
    [JsonProperty("uri")]
    public string Uri { get; set; }

    [JsonProperty("folder")]
    public bool? Folder { get; set; }
  }
}
