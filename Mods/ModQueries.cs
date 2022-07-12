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
            List<int> statuses = new List<int> { 2 };
            if(request.queryString.Get("status") != null)
            {
                statuses = request.queryString.Get("status").Split(',').ToList().ConvertAll(x => Convert.ToInt32(x));
            }

            if(request.queryString.Get("mymods") != null)
            {
                // Return mods by uploader
                if (request.queryString.Get("status") == null) statuses = new List<int> { 0, 1, 2, 3 };
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsByUser(u.username, statuses)));
            }
            if (request.queryString.Get("group") != null && request.queryString.Get("version") != null)
            {
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsOfGroupAndVersion(request.queryString.Get("group"), request.queryString.Get("version"), statuses)));
            }
            if (request.queryString.Get("group") != null)
            {
                return new GenericRequestResponse(200, JsonSerializer.Serialize(MongoDBInteractor.GetModsOfGroup(request.queryString.Get("group"), statuses)));
            }
            return new GenericRequestResponse(200, "[]");
        }
    }
}
