using System;

namespace ArtifactoryBrowser.Models
{
    public class ArtifactoryFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public double Size { get; set; } // Taille en Mo
        public DateTime LastModified { get; set; }
        public string Environment { get; set; }
    }
}
