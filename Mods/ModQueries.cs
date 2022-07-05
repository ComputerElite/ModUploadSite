using ComputerUtils.Webserver;
using ModUploadSite.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModUploadSite.Mods
{
    public class ModQueries
    {
        public static GenericRequestResponse GetMods(ServerRequest request, string token)
        {
            User u = MongoDBInteractor.GetUserByToken(token);
            if(request.queryString.Get("mymods") != null)
            {
                // Return mods by uploader
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsByUser(u.username)));
            }
            if (request.queryString.Get("group") != null && request.queryString.Get("version") != null)
            {
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsOfGroupAndVersion(request.queryString.Get("group"), request.queryString.Get("version"))));
            }
            if (request.queryString.Get("group") != null)
            {
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsOfGroup(request.queryString.Get("group"))));
            }
            return new GenericRequestResponse(200, "[]");
        }
    }
}
