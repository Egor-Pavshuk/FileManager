using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManager.Models
{
    public class FtpFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Size { get; set; }
        public string Type { get; set; }
        public string CreationTime { get; set; }
    }
}
