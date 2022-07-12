using ComputerUtils.Encryption;
using ComputerUtils.Logging;
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
            if (!IsUserAllowedToEditMod(mod, MongoDBInteractor.GetGroup(mod.group))) return new GenericRequestResponse(400, "Unpublish the mod before editing (approval needed after edit) or create a new mod");
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");
            // Check if file exists
            mod.RemoveModFile(fileHash);
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
            if (!IsUserAllowedToEditMod(cmod, MongoDBInteractor.GetGroup(mod.group))) return new GenericRequestResponse(400, "Unpublish the mod before editing (approval needed after edit) or create a new mod");
            mod.uploader = cmod.uploader;
            mod.status = cmod.status;
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
            return IsUserOwnerOfMod(MongoDBInteractor.GetUserByToken(token), mod);
        }

        public static bool IsUserOwnerOfMod(User u, UploadedMod mod)
        {
            if (u == null) return false;
            return u.username == mod.uploader;
        }

        public static bool IsUserAllowedToEditMod(UploadedMod mod, UploadedModGroup group)
        {
            if (group.requireApproval && mod.status != UploadedModStatus.Unpublished) return false;
            return true;
        }

        public static GenericRequestResponse HandleModFileAutoPopulate(string modId, string fileHash, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            if (!IdValidator.IsIdValid(fileHash)) return new GenericRequestResponse(400, "File hash is not valid");

            // Check if user is owner of mod
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(token, mod)) return new GenericRequestResponse(403, "You do not own this mod. Modifications are not allowed");
            if (!IsUserAllowedToEditMod(mod, MongoDBInteractor.GetGroup(mod.group))) return new GenericRequestResponse(400, "Unpublish the mod before editing (approval needed after edit) or create a new mod");

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

            UploadedModGroup group = MongoDBInteractor.GetGroup(mod.group);
            if (!IsUserAllowedToEditMod(mod, group)) return new GenericRequestResponse(400, "Unpublish the mod before editing (approval needed after edit) or create a new mod");


            string fileHash = Hasher.GetSHA256OfByteArray(file);
            // Check if file already exists
            string filePath = PathManager.GetModFile(modId, fileHash);
            if (File.Exists(filePath)) mod.RemoveModFile(fileHash);
            // Save file
            File.WriteAllBytes(filePath, file);

            

            ValidationResult result = ValidationManager.ValidateFile(filename, filePath);
            bool isAllowed = false;
            if (group != null && group.fileTypes.Contains(Path.GetExtension(filename).ToLower())) isAllowed = true;
            if (result.valid)
            {
                // Add file to mod if file is valid
                UploadedModFile modfile = new UploadedModFile(fileHash, filename, new FileInfo(filePath).Length);
                mod.files.Add(modfile);

                ConversionResult conversionResult = ConversionManager.ConvertUploadedModFile(mod, modfile);
                if(conversionResult.converted)
                {
                    mod = conversionResult.mod;
                    if (group != null && group.fileTypes.Contains(conversionResult.to.ToLower()))
                    {
                        isAllowed = true;
                    } else if(!isAllowed)
                    {
                        mod.RemoveModFile(conversionResult.file.sHA256);
                    }
                }
            } else mod.RemoveModFile(fileHash);
            if(!isAllowed)
            {
                result.valid = false;
                result.msg = "File type not allowed." + (group != null ? " Please use any of: " + String.Join(", ", group.fileTypes) : "");
                mod.RemoveModFile(fileHash);
            }
            if(group == null)
            {
                result.valid = false;
                result.msg = "This group does not exist. Please select a group first and save your changes before uploading a file";
            }
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(result.valid ? 200 : 400, result.msg);
        }

        public static GenericRequestResponse CreateModUploadSession(string token)
        {
            User u = MongoDBInteractor.GetUserByToken(token);
            if (u == null) return new GenericRequestResponse(400, "Invalid user session");
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

        public static GenericRequestResponse UpdateModStatus(string modId, string status, string token)
        {
            if (!IdValidator.IsIdValid(modId)) return new GenericRequestResponse(400, "ModId is not valid");
            // Get User
            User u = MongoDBInteractor.GetUserByToken(token);
            // Check if user is owner of mod or allowed to edit
            UploadedMod mod = MongoDBInteractor.GetMod(modId);
            if (!IsUserOwnerOfMod(u, mod) && !u.HasPermission(UserPermission.ApproveAndDeclineMods)) return new GenericRequestResponse(403, "You are not allowed to change this mods status");

            UploadedModStatus newModStatus;
            try
            {
                int s = Convert.ToInt32(status);
                newModStatus = (UploadedModStatus)s;
            } catch
            {
                return new GenericRequestResponse(400, "Status must be an int");
            }
            UploadedModGroup group = MongoDBInteractor.GetGroup(mod.group);
            if(newModStatus == UploadedModStatus.Approved || newModStatus == UploadedModStatus.Declined)
            {
                if(!u.HasPermission(UserPermission.ApproveAndDeclineMods) && group.requireApproval)
                {
                    return new GenericRequestResponse(403, "You are not allowed to approve or decline mods");
                }
            }
            mod.status = newModStatus;
            MongoDBInteractor.UpdateMod(mod);
            return new GenericRequestResponse(200, "Mod state set to " + Enum.GetName(mod.status));
        }
    }
}
