using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite
{
    public class PathManager
    {
        public static string GetModDirectory(string modId)
        {
            return MUSEnvironment.dataDir + "mods" + Path.DirectorySeparatorChar + modId;
        }

        public static string GetModFile(string modId, string fileId)
        {
            return MUSEnvironment.dataDir + "mods" + Path.DirectorySeparatorChar + modId + Path.DirectorySeparatorChar + fileId;
        }
    }
}
