using ComputerUtils.Encryption;
using ComputerUtils.Logging;
using ModUploadSite.Mods;
using ModUploadSite.Users;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite
{
    public class MongoDBInteractor
    {
        public static MongoClient mongoClient = null;
        public static IMongoDatabase mUSDatabase = null;
        public static IMongoCollection<UploadedMod> modCollection = null;
        public static IMongoCollection<UploadedModGroup> groupCollection = null;
        public static IMongoCollection<User> userCollection = null;
        public static IMongoCollection<BsonDocument> backupCollection = null;


        public static void Initialize()
        {
            mongoClient = new MongoClient(MUSEnvironment.config.mongoDBUrl);
            mUSDatabase = mongoClient.GetDatabase(MUSEnvironment.config.mongoDBName);
            modCollection = mUSDatabase.GetCollection<UploadedMod>("mods");
            userCollection = mUSDatabase.GetCollection<User>("users");
            groupCollection = mUSDatabase.GetCollection<UploadedModGroup>("groups");
            backupCollection = mUSDatabase.GetCollection<BsonDocument>("backups");

            ConventionPack pack = new ConventionPack();
            pack.Add(new IgnoreExtraElementsConvention(true));
            ConventionRegistry.Register("Ignore extra elements cause it's annoying", pack, t => true);
        }

        public static void CreateBackup(BsonDocument d)
        {
            backupCollection.InsertOne(d);
        }

        public static void UpdateGroup(UploadedModGroup group)
        {
            groupCollection.ReplaceOne(x => x.groupId == group.groupId, group);
        }

        public static void AddGroup(UploadedModGroup group)
        {
            groupCollection.InsertOne(group);
        }

        public static UploadedModGroup GetGroup(string groupId)
        {
            return groupCollection.Find(x => x.groupId == groupId).FirstOrDefault();
        }

        public static void RemoveGroup(string groupId)
        {
            groupCollection.DeleteOne(x => x.groupId == groupId);
        }


        public static List<UploadedModGroup> GetModGroups()
        {
            return groupCollection.Find(x => true).ToList();
        }

        public static List<string> GetModGroupVersions(string groupId)
        {
            return modCollection.Distinct(x => x.groupVersion, x => x.group == groupId).ToList();
        }

        public static List<UploadedMod> GetModsByUser(string username, List<int> statuses)
        {
            return modCollection.Find(GetStatusFilter(new List<FilterDefinition<UploadedMod>> { Builders<UploadedMod>.Filter.Eq("uploader", username) }, statuses)).ToList();

        }

        public static List<UploadedMod> GetModsOfGroup(string game, List<int> statuses)
        {
            return modCollection.Find(GetStatusFilter(new List<FilterDefinition<UploadedMod>> { Builders<UploadedMod>.Filter.Eq("group", game) }, statuses)).ToList();

        }
        public static List<UploadedMod> GetModsOfGroupAndVersion(string game, string gameversion, List<int> statuses)
        {
            return modCollection.Find(GetStatusFilter(new List<FilterDefinition<UploadedMod>> { Builders<UploadedMod>.Filter.Eq("group", game), Builders<UploadedMod>.Filter.Eq("groupVersion", gameversion) }, statuses)).ToList();
        }

        public static FilterDefinition<UploadedMod> GetStatusFilter(List<FilterDefinition<UploadedMod>> mustMatch, List<int> statuses)
        {
            List<FilterDefinition<UploadedMod>> filters = new List<FilterDefinition<UploadedMod>>();
            foreach (int status in statuses)
            {
                Logger.Log(status.ToString());
                filters.Add(Builders<UploadedMod>.Filter.Eq("status", status));
            }
            mustMatch.Add(Builders<UploadedMod>.Filter.Or(filters));
            return Builders<UploadedMod>.Filter.And(mustMatch);
        }

        public static void DeleteMod(string modId)
        {
            modCollection.DeleteOne(x => x.uploadedModId == modId);
        }

        public static User GetUser(User u)
        {
            string sha = Hasher.GetSHA256OfString(u.password);
            return userCollection.Find(x => x.username == u.username && x.password == sha).FirstOrDefault();
        }

        public static User GetUserByUsernameAndEmail(User u)
        {
            return userCollection.Find(x => x.username == u.username && x.email.ToLower() == u.email.ToLower()).FirstOrDefault();
        }

        public static User GetUserByUsername(string username)
        {
            return userCollection.Find(x => x.username == username).FirstOrDefault();
        }

        public static void AddUser(User u)
        {
            userCollection.InsertOne(u);
        }

        public static User GetUserByToken(string token)
        {
            string sha = Hasher.GetSHA256OfString(token);
            return userCollection.Find(x => x.currentToken == token).FirstOrDefault();
        }
        public static void UpdateUser(User u)
        {
            string sha = Hasher.GetSHA256OfString(u.password);
            userCollection.ReplaceOne(x => x.username == u.username, u);
        }

        public static void UpdateMod(UploadedMod mod)
        {
            modCollection.ReplaceOne(x => x.uploadedModId == mod.uploadedModId, mod);
        }

        public static UploadedMod GetMod(string modId)
        {
            return modCollection.Find(x => x.uploadedModId == modId).FirstOrDefault();
        }

        public static void AddUploadedMod(UploadedMod mod)
        {
            modCollection.InsertOne(mod);
        }
    }
}
