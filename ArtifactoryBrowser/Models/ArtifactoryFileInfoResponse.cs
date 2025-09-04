using System;
using Newtonsoft.Json;

namespace ArtifactoryBrowser.Models
{
  public class ArtifactoryFileInfoResponse
  {
    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("lastModified")]
    public DateTime LastModified { get; set; }

    [JsonProperty("downloadCount")]
    public int DownloadCount { get; set; }
  }
}
