using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Users
{
    [BsonIgnoreExtraElements]
    public class User
    {
        public string username { get; set; } = "invalid";
        public string password { get; set; } = "invalid";
        public string email { get; set; } = "invalid";
        public string currentToken { get; set; } = "";
        public string currentPasswordResetToken { get; set; } = "";
        public DateTime passwordResetRequestTime { get; set; } = DateTime.Now;
        public List<UserPermission> permissions { get; set; } = new List<UserPermission>();

        public bool HasPermission(UserPermission permission)
        {
            return permissions.Contains(permission);
        }
    }
}
