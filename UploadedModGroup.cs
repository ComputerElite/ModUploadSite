using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite
{
    public class UploadedModGroup
    {
        public string name { get; set; } = "";
        public string description { get; set; } = "";
        public List<string> fileTypes { get; set; } = new List<string>();
    }
}
