using ModUploadSite.Mods;
using QuestPatcher.QMod;

namespace ModUploadSite.Populators
{
    public class QModPopulator
    {
        public static PopulationResult PopulateQMod(UploadedMod mod, string filePath)
        {
            try
            {
                Stream s = File.Open(filePath, FileMode.Open);
                QMod loadedQmod = QMod.ParseAsync(s).Result;
                s.Close();
                mod.group = GroupHandler.GetBestMatchingGroupID(loadedQmod.PackageId);
                mod.author = loadedQmod.Author;
                mod.description = loadedQmod.Description;
                mod.version = loadedQmod.Version.ToString();
                mod.groupVersion = loadedQmod.PackageVersion;
                mod.modId = loadedQmod.Id;
                mod.name = loadedQmod.Name;
                mod.porter = loadedQmod.Porter;
            } catch(Exception e)
            {
                return new PopulationResult(false, e.Message, mod);
            }
            return new PopulationResult(true, "Populated mod info", mod);
        }
    }
}