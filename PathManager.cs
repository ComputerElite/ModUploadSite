using ComputerUtils.Encryption;
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
            return MUSEnvironment.config.modFolder + Path.DirectorySeparatorChar + modId;
        }

        public static string GetModFile(string modId, string fileId)
        {
            return MUSEnvironment.config.modFolder + Path.DirectorySeparatorChar + modId + Path.DirectorySeparatorChar + fileId;
        }

        public static string GetSHA256OfFile(string fileLocation)
        {
            return Hasher.GetSHA256OfByteArray(File.ReadAllBytes(fileLocation));
        }

        public static long GetFileSize(string fileLocation)
        {
            return new FileInfo(fileLocation).Length;
        }

        public static string GetTempFileLocation()
        {
            return MUSEnvironment.config.modFolder + Path.DirectorySeparatorChar + "temp" + Path.DirectorySeparatorChar + DateTime.UtcNow.Ticks.ToString();
        }
    }
}
