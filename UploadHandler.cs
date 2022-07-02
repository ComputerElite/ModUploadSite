using ComputerUtils.Encryption;
using ComputerUtils.RandomExtensions;
using ModUploadSite.Populators;
using ModUploadSite.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModUploadSite
{
    public class UploadHandler
    {
        public static GenericRequestResponse HandleDeleteUploadedModFile(string modId, string fileHash)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (!IdValidator.IsIdValid(fileHash)) return new GenericRequestResponse(400, "File hash is not valid");
            // Check if file exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (!File.Exists(filePath)) return new GenericRequestResponse(400, "ModFile does not exist");
            // Delete file from Disk
            File.Delete(filePath);
            // Remove file from mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            for(int i = 0; i < mod.files.Count; i++)
            {
                if (mod.files[i].sHA256 != fileHash) continue;
                mod.files.RemoveAt(i);
                i--;
            }
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "File removed");
        }

        public static GenericRequestResponse HandleUpdateModInfo(string modJson)
        {
            return HandleUpdateModInfo(JsonSerializer.Deserialize<UploadedMod>(modJson));
        }

        public static GenericRequestResponse HandleUpdateModInfo(UploadedMod mod)
        {
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "Updated mod");
        }

        public static GenericRequestResponse HandleModFileAutoPopulate(string modId, string fileHash)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (!IdValidator.IsIdValid(fileHash)) return new GenericRequestResponse(400, "File hash is not valid");
            // Check if file exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (!File.Exists(filePath)) return new GenericRequestResponse(400, "ModFile does not exist");
            // Remove file from mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            PopulationResult result = PopulatorManager.PopulateUploadedMod(mod, filePath);
            if(result.populated)
            {
                mod = result.populatedMod;
                MongoDBInteractor.UpdateMod(mod);
            }
           
            return new GenericRequestResponse(result.populated ? 200 : 400, result.msg);
        }

        public static GenericRequestResponse HandleUploadeOfModFile(string modId, byte[] file, string filename)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (filename == null) return new GenericRequestResponse(400, "Missing filename");
            string fileHash = Hasher.GetSHA256OfByteArray(file);
            // Check if file already exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (File.Exists(filePath)) return new GenericRequestResponse(400, "File already exists");
            // Save file
            File.WriteAllBytes(filePath, file);

            UploadedMod mod = MongoDBInteractor.GetMod(modId);

            ValidationResult result = ValidationManager.ValidateFile(filePath);
            if (result.valid)
            {
                // Add file to mod if file is valid
                mod.files.Add(new UploadedModFile(fileHash, filename, new FileInfo(filePath).Length));
                MongoDBInteractor.UpdateMod(mod);
            }
            return new GenericRequestResponse(result.valid ? 200 : 400, result.msg);
        }

        public static GenericRequestResponse CreateModUploadSession()
        {
            string modId = IdValidator.GetNewModId(); // Chance that id exists is very low but ofc can happen. Chance is 1 in 2^256
            UploadedMod newMod = new UploadedMod();
            newMod.uploadedModId = modId;
            // Create mod directory for files
            Directory.CreateDirectory(PathManager.GetModDirectory(newMod.uploadedModId));
            // Add mod to database
            MongoDBInteractor.AddUploadedMod(newMod);
            return new GenericRequestResponse(200, newMod.uploadedModId);
        }

        public static GenericRequestResponse PublishMod(string modId)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            // Get mod and set state to Pending
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            mod.status = UploadedModStatus.Pending;
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "Mod state set to Pending");
        }
    }
}
