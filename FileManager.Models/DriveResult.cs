using System.Collections.ObjectModel;

namespace FileManager.Models
{
    public class DriveResult
    {
        public string NextPageToken { get; set; }
        public Collection<GoogleDriveFile> Files { get; set; }
    }

}
