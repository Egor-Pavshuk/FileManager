using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Models
{
    public class GoogleDriveFile
    {
        public string MimeType { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Kind { get; set; }
    }

}
