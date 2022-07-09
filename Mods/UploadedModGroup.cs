using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModUploadSite.Mods
{
    [BsonIgnoreExtraElements]
    public class UploadedModGroup
    {
        public string name { get; set; } = "";
        public string groupId { get; set; } = "";
        public string description { get; set; } = "";
        public bool hasVersions { get; set; } = true;
        public bool requireApproval { get; set; } = true;
        public List<string> populationAlternateIDs { get; set; } = new List<string>();
        public List<string> fileTypes { get; set; } = new List<string>();
    }
}
