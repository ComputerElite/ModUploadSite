using ComputerUtils.Encryption;
using ComputerUtils.RandomExtensions;
using ModUploadSite.Converters;
using ModUploadSite.Mods;
using ModUploadSite.Populators;
using ModUploadSite.Users;
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
        public static GenericRequestResponse HandleDeleteUploadedModFile(string modId, string fileHash, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (!IdValidator.IsIdValid(fileHash)) return new GenericRequestResponse(400, "File hash is not valid");
            // Check if user is owner of mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");
            // Check if file exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (!File.Exists(filePath)) return new GenericRequestResponse(400, "ModFile does not exist");
            // Delete file from Disk
            File.Delete(filePath);
            // Remove file from mod
            for(int i = 0; i < mod.files.Count; i++)
            {
                if (mod.files[i].sHA256 != fileHash) continue;
                mod.files.RemoveAt(i);
                i--;
            }
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "File removed");
        }

        public static GenericRequestResponse HandleUpdateModInfo(string modJson, string token)
        {
            return HandleUpdateModInfo(JsonSerializer.Deserialize<UploadedMod>(modJson), token);
        }

        public static GenericRequestResponse HandleUpdateModInfo(UploadedMod mod, string token)
        {
            // Check if user is owner of mod
            UploadedMod cmod = MongoDBInteractor.GetMod(mod.uploadedModId);
            if (!IsUserOwnerOfMod(token, cmod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");
            mod.uploader = cmod.uploader;
            if (MongoDBInteractor.GetModGroups().FirstOrDefault(x => x.groupId == mod.group) == null && mod.group != "") return new GenericRequestResponse(400, "This group does not exist. Please put in a group id");
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "Updated mod");
        }

        public static GenericRequestResponse HandleDeleteMod(string modId, string token)
        {
            // Check if user is owner of mod
            UploadedMod cmod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, cmod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");
            Directory.Delete(PathManager.GetModDirectory(modId), true);
            MongoDBInteractor.DeleteMod(modId);
            return new GenericRequestResponse(200, "Updated mod");
        }

        public static bool IsUserOwnerOfMod(string token, UploadedMod mod)
        {
            User u = MongoDBInteractor.GetUserByToken(token);
            if(u == null) return false;
            return u.username == mod.uploader;
        }

        public static GenericRequestResponse HandleModFileAutoPopulate(string modId, string fileHash, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (!IdValidator.IsIdValid(fileHash)) return new GenericRequestResponse(400, "File hash is not valid");

            // Check if user is owner of mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");

            // Check if file exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (!File.Exists(filePath)) return new GenericRequestResponse(400, "ModFile does not exist");
            // Remove file from mod
            
            string fileName = mod.files.FirstOrDefault(x => x.sHA256 == fileHash).filename;
            if(fileName == null) return new GenericRequestResponse(400, "ModFile is not registered");
            PopulationResult result = PopulatorManager.PopulateUploadedMod(mod, fileName, filePath);
            if(result.populated)
            {
                mod = result.populatedMod;
                MongoDBInteractor.UpdateMod(mod);
            }
           
            return new GenericRequestResponse(result.populated ? 200 : 400, result.msg);
        }

        public static GenericRequestResponse HandleUploadeOfModFile(string modId, byte[] file, string filename, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (filename == null) return new GenericRequestResponse(400, "Missing filename");

            // Check if user is owner of mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");

            string fileHash = Hasher.GetSHA256OfByteArray(file);
            // Check if file already exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (File.Exists(filePath)) return new GenericRequestResponse(400, "File already exists");
            // Save file
            File.WriteAllBytes(filePath, file);

            ValidationResult result = ValidationManager.ValidateFile(filename, filePath);
            if (result.valid)
            {
                // Add file to mod if file is valid
                UploadedModFile modfile = new UploadedModFile(fileHash, filename, new FileInfo(filePath).Length);
                mod.files.Add(modfile);

                ConversionResult conversionResult = ConversionManager.ConvertUploadedModFile(mod, modfile);
                if(conversionResult.converted)
                {
                    mod = conversionResult.mod;
                }

                MongoDBInteractor.UpdateMod(mod);
            }
            return new GenericRequestResponse(result.valid ? 200 : 400, result.msg);
        }

        public static GenericRequestResponse CreateModUploadSession(string token)
        {
            User u = MongoDBInteractor.GetUserByToken(token);
            if (u == null) return new GenericRequestResponse(400, "Invalid session");
            string modId = IdValidator.GetNewId(); // Chance that id exists is very low but ofc can happen. Chance is 1 in 2^256
            UploadedMod newMod = new UploadedMod();
            newMod.uploadedModId = modId;
            newMod.uploader = u.username;
            // Create mod directory for files
            Directory.CreateDirectory(PathManager.GetModDirectory(newMod.uploadedModId));
            // Add mod to database
            MongoDBInteractor.AddUploadedMod(newMod);
            return new GenericRequestResponse(200, newMod.uploadedModId);
        }

        public static GenericRequestResponse PublishMod(string modId, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");

            // Check if user is owner of mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");

            // Get mod and set state to Pending
            mod.status = UploadedModStatus.Pending;
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "Mod state set to Pending");
        }
    }
}
