using ModUploadSite.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Converters
{
    public class ConversionManager
    {
        public static Dictionary<string, Func<UploadedMod, UploadedModFile, ConversionResult>> convertors { get; set; } = new Dictionary<string, Func<UploadedMod, UploadedModFile, ConversionResult>>();

        public static bool AddConverter(List<string> extensionsIncludingDot, Func<UploadedMod, UploadedModFile, ConversionResult> conversionFunction, bool overrideExisting = true)
        {
            extensionsIncludingDot.ForEach(x => x = x.ToLower());
            foreach(string extension in extensionsIncludingDot)
            {
                if (!overrideExisting && convertors.ContainsKey(extension)) return false;
                convertors[extension] = conversionFunction;
            }
            
            return true;
        }

        public static void AddDefaultConverters()
        {
            AddConverter(new List<string> { ".wav", ".mp3", ".m4a" }, new Func<UploadedMod, UploadedModFile, ConversionResult>((mod, file) =>
            {
                return ToOggConverter.ConvertToOgg(mod, file);
            }));
        }

        public static ConversionResult ConvertUploadedModFile(UploadedMod mod, UploadedModFile file)
        {
            string extension = Path.GetExtension(file.filename).ToLower();
            if (!convertors.ContainsKey(extension)) return new ConversionResult(false, false, "", "", "No conversion possible", null);
            ConversionResult res = convertors[extension].Invoke(mod, file);
            if(res.converted && res.deleteOld)
            {
                // Todo, remove old file from mod
                File.Delete(PathManager.GetModFile(mod.uploadedModId, file.sHA256));
                for(int i = 0; i < mod.files.Count; i++)
                {
                    if (mod.files[i].sHA256 == file.sHA256)
                    {
                        mod.files.RemoveAt(i);
                        break;
                    }
                }
            }
            if(res.converted)
            {
                mod.files.Add(res.file);
                res.mod = mod;
            }
            return res;
        }
    }
}
