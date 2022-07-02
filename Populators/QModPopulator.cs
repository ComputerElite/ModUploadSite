using QuestPatcher.QMod;

namespace ModUploadSite.Populators
{
    public class QModPopulator
    {
        public static PopulationResult PopulateQMod(UploadedMod mod, string filePath)
        {
            try
            {
                QMod loadedQmod = QMod.ParseAsync(File.Open(filePath, FileMode.Open)).Result;
                mod.packageId = loadedQmod.PackageId;
                mod.author = loadedQmod.Author;
                mod.description = loadedQmod.Description;
                mod.version = loadedQmod.Version.ToString();
                mod.packageVersion = loadedQmod.PackageVersion;
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