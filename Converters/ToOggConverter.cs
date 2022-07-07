using ModUploadSite.Mods;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Converters
{
    public class ToOggConverter
    {
        public static ConversionResult ConvertToOgg(UploadedMod mod, UploadedModFile file)
        {
            string tempFileLocation = PathManager.GetTempFileLocation() + ".ogg";
            Process p = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-i \"" + PathManager.GetModFile(mod.uploadedModId, file.sHA256) + "\" \"" + tempFileLocation + "\""
            });
            p.WaitForExit();
            string newHash = PathManager.GetSHA256OfFile(tempFileLocation);
            string newLocation = PathManager.GetModFile(mod.uploadedModId, newHash);
            File.Copy(tempFileLocation, newLocation, true);
            return new ConversionResult(true, true, Path.GetExtension(file.filename), ".ogg", "Converted with ffmpeg", new UploadedModFile(newHash, file.filename.Substring(0, file.filename.Length - Path.GetExtension(file.filename).Length) + ".ogg", PathManager.GetFileSize(newLocation)));
        }
    }
}
