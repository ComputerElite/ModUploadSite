using ComputerUtils.VarUtils;
using ModUploadSite.Populators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Mods
{
    public class UploadedModFile
    {
        public UploadedModFile(string sHA256, string filename, long size)
        {
            this.sHA256 = sHA256;
            this.filename = filename;
            this.size = size;
        }

        public string filename { get; set; } = "";
        public string sHA256 { get; set; } = "";
        public long size { get; set; } = 0;
        public string sizeString
        {
            get
            {
                return SizeConverter.ByteSizeToString(size);
            }
        }
        public bool supportsModInfoPopulation
        {
            get
            {
                return PopulatorManager.CanPopulate(filename);
            }
        }
    }
}
