using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite
{
    [BsonIgnoreExtraElements]
    public class UploadedMod
    {
        

        public UploadedModStatus status { get; set; } = UploadedModStatus.Unpublished;
        public string name { get; set; } = ""; // Mod
        public string author { get; set; } = ""; // ComputerElite
        public string porter { get; set; } = "";
        public string version { get; set; } = ""; // 1.0.0
        public string packageId { get; set; } = ""; // com.beatgames.beatsaber
        public string packageVersion { get; set; } = ""; // 1.23.0
        public string uploaderId { get; set; } = ""; 
        public string description { get; set; } = ""; // A really great mod
        public string uploadedModId { get; set; } = "";
        public string modId { get; set; } = "";
        public List<string> links { get; set; } = new List<string>();
        public List<UploadedModFile> files { get; set; } = new List<UploadedModFile>();
    }
}
