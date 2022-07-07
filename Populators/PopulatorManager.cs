using ModUploadSite.Mods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Populators
{
    public class PopulatorManager
    {
        public static Dictionary<string, Func<UploadedMod, string, PopulationResult>> populators { get; set; } = new Dictionary<string, Func<UploadedMod, string, PopulationResult>>();

        public static bool AddPopulator(string extensionIncludingDot, Func<UploadedMod, string, PopulationResult> populationFunction, bool overrideExisting = true)
        {
            extensionIncludingDot = extensionIncludingDot.ToLower();
            if (!overrideExisting && populators.ContainsKey(extensionIncludingDot)) return false;
            populators[extensionIncludingDot] = populationFunction;
            return true;
        }

        public static bool CanPopulate(string filename)
        {
            return populators.ContainsKey(Path.GetExtension(filename).ToLower());
        }

        public static PopulationResult PopulateUploadedMod(UploadedMod mod, string fileName, string filePath)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            if (!populators.ContainsKey(extension)) return new PopulationResult(false, "No population possile", mod);
            return populators[extension].Invoke(mod, filePath);
        }

        public static void AddDefaultPopulators()
        {
            AddPopulator(".qmod", new Func<UploadedMod, string, PopulationResult>((mod, filePath) =>
            {
                return QModPopulator.PopulateQMod(mod, filePath);
            }));
        }
    }
}
