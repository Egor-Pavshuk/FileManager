using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Models
{
    public class DriveResult
    {
        public string NextPageToken { get; set; }
        public Collection<GoogleDriveFile> Files { get; set; }
    }

}
