using ModUploadSite.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Populators
{
    public class PopulationResult
    {
        public PopulationResult(bool populated, string msg, UploadedMod populatedMod)
        {
            this.populated = populated;
            this.msg = msg;
            this.populatedMod = populatedMod;
        }

        public bool populated { get; set; } = false;
        public string msg { get; set; } = "";
        public UploadedMod populatedMod { get; set; } = new UploadedMod();
    }
}
