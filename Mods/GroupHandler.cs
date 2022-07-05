using ModUploadSite.Users;
using ModUploadSite.Validators;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModUploadSite.Mods
{
    public class GroupHandler
    {
        public static string GetBestMatchingGroupID(string id)
        {
            List<UploadedModGroup> groups = MongoDBInteractor.GetModGroups();
            foreach(UploadedModGroup group in groups)
            {
                if (group.groupId == id || group.populationAlternateIDs.FirstOrDefault(x => x.ToLower() == id.ToLower()) != null) return group.groupId;
            }
            return "";
        }

        public static bool IsUserAllowedToEditGroups(string token)
        {
            User u = MongoDBInteractor.GetUserByToken(token);
            if (u == null) return false;
            return u.HasPermission(UserPermission.ModGroups);
        }

        public static GenericRequestResponse CreateGroup(string token)
        {
            if (!IsUserAllowedToEditGroups(token)) return new GenericRequestResponse(403, "You're not allowed to create groups");
            UploadedModGroup group = new UploadedModGroup();
            group.groupId = IdValidator.GetNewId();
            MongoDBInteractor.AddGroup(group);
            return new GenericRequestResponse(200, group.groupId);
        }

        public static GenericRequestResponse DeleteGroup(string token, string groupId)
        {
            if (!IsUserAllowedToEditGroups(token)) return new GenericRequestResponse(403, "You're not allowed to delete groups");
            MongoDBInteractor.CreateBackup(MongoDBInteractor.GetGroup(groupId).ToBsonDocument());
            MongoDBInteractor.RemoveGroup(groupId);
            return new GenericRequestResponse(200, "Deleted");
        }

        public static GenericRequestResponse UpdateGroup(string token, string group)
        {
            if (!IsUserAllowedToEditGroups(token)) return new GenericRequestResponse(403, "You're not allowed to edit groups");

            UploadedModGroup g = JsonSerializer.Deserialize<UploadedModGroup>(group);
            MongoDBInteractor.UpdateGroup(g);
            return new GenericRequestResponse(200, "Probably updated if you didn't send shit :)");
        }


        public static GenericRequestResponse GetGroups()
        {
            return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModGroups()));
        }

        public static GenericRequestResponse GetVersionsOfGroup(string groupId)
        {
            return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModGroupVersions(groupId)));
        }
    }
}
