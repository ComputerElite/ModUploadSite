using ComputerUtils.Logging;
using System.Text.Json;

namespace ModUploadSite
{
    public class Config
    {
        public string publicAddress { get; set; } = "";
        public string crashPingId { get; set; } = "631189193825058826";
        public int port { get; set; } = 507;
        public string masterToken { get; set; } = "";
        public string mongoDBName { get; set; } = "MUS";
        public string mongoDBUrl { get; set; } = "";
        public string masterWebhookUrl { get; set; } = "";
        public string emailSender { get; set; } = "";
        public string emailPassword { get; set; } = "";
        public string emailServer { get; set; } = "";
        public long modSizeLimit { get; set; } = 104857600; // 100 MB
        public string modFolder { get; set; } = "";

        public static Config LoadConfig()
        {
            string configLocation = MUSEnvironment.workingDir + "data" + Path.DirectorySeparatorChar + "config.json";
            if (!File.Exists(configLocation)) File.WriteAllText(configLocation, JsonSerializer.Serialize(new Config()));
            return JsonSerializer.Deserialize<Config>(File.ReadAllText(configLocation));
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(MUSEnvironment.workingDir + "data" + Path.DirectorySeparatorChar + "config.json", JsonSerializer.Serialize(this));
            }
            catch (Exception e)
            {
                Logger.Log("couldn't save config: " + e.ToString(), LoggingType.Warning);
            }
        }
    }
}