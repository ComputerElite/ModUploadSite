using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Mods
{
    public enum UploadedModStatus
    {
        Unpublished = 0,
        Pending = 1,
        Approved = 2,
        Declined = 3
    }
}
