using ModUploadSite.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Converters
{
    public class ConversionResult
    {
        public bool converted { get; set; } = false;
        public bool deleteOld { get; set; } = false;
        public string from { get; set; } = "";
        public string to { get; set; } = "";
        public string msg { get; set; } = "";
        public UploadedModFile file { get; set; } = null;
        public UploadedMod mod { get; set; } = null;

        public ConversionResult(bool converted, bool deleteOld, string from, string to, string msg, UploadedModFile file)
        {
            this.converted = converted;
            this.deleteOld = deleteOld;
            this.from = from;
            this.to = to;
            this.msg = msg;
            this.file = file;
        }
    }
}
