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

        public static void Initialize()
        {
            mongoClient = new MongoClient(MUSEnvironment.config.mongoDBUrl);
            mUSDatabase = mongoClient.GetDatabase(MUSEnvironment.config.mongoDBName);
            modCollection = mUSDatabase.GetCollection<UploadedMod>("mods");

            ConventionPack pack = new ConventionPack();
            pack.Add(new IgnoreExtraElementsConvention(true));
            ConventionRegistry.Register("Ignore extra elements cause it's annoying", pack, t => true);
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
