using ComputerUtils.Logging;
using ComputerUtils.Webserver;
using ModUploadSite.Converters;
using ModUploadSite.Mods;
using ModUploadSite.Populators;
using ModUploadSite.Users;
using ModUploadSite.Validators;
using System.Text.Json;

namespace ModUploadSite
{
    public class Server
    {
        public HttpServer server = new HttpServer();
        public Config config
        {
            get
            {
                return MUSEnvironment.config;
            }
        }
        public Dictionary<string, string> replace = new Dictionary<string, string>
        {
            {"{meta}", "<meta name=\"theme-color\" content=\"#63fac3\">\n<meta property=\"og:site_name\" content=\"Mod Upload Site\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n<link rel=\"stylesheet\" href=\"/style.css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,400italic,700,700italic\" rel=\"stylesheet\" type=\"text/css\">" }
        };

        public string GetToken(ServerRequest request, bool send403 = true)
        {
            string token = request.context.Request.Headers["token"];
            if (token == null)
            {
                token = request.queryString["token"];
                if (token == null)
                {
                    token = request.cookies["token"] == null ? "" : request.cookies["token"].Value;
                    if (token == null)
                    {
                        if (send403) request.Send403();
                        return "";
                    }
                }
            }
            return token;
        }

        public bool IsUserAdmin(ServerRequest request, bool send403 = true)
        {
            return GetToken(request, send403) == config.masterToken;
        }

        public void HandleGenericResponse(ServerRequest request, GenericRequestResponse response)
        {
            request.SendString(response.message, response.contentType, response.statusCode);
        }

        public void StartServer()
        {
            server = new HttpServer();
            server.DefaultCacheValidityInSeconds = 0;
            string frontend = "frontend" + Path.DirectorySeparatorChar;

            // Init everything
            MongoDBInteractor.Initialize();
            PopulatorManager.AddDefaultPopulators();
            ValidationManager.AddDefaultValidators();
            ConversionManager.AddDefaultConverters();
            UserSystem.Initialize();

            // cdn
            server.AddRoute("GET", "/cdn/", new Func<ServerRequest, bool>(request =>
            {
                string[] m = request.pathDiff.Split('/');
                if(m.Length < 2)
                {
                    request.Send404();
                    return true;
                }
                UploadedMod mod = MongoDBInteractor.GetMod(m[0]);
                if(mod == null)
                {
                    request.Send404();
                    return true;
                }
                foreach(UploadedModFile file in mod.files)
                {
                    if(file.sHA256 == m[1])
                    {
                        request.SendFile(PathManager.GetModFile(mod.uploadedModId, file.sHA256), "", 200, true, new Dictionary<string, string> { { "content-disposition", "attachment; filename=\"" + file.filename + "\"" } });
                    }
                }
                request.Send404();
                return true;
            }), true, true, true);
            
            // Groups
            server.AddRoute("GET", "/api/v1/groups", new Func<ServerRequest, bool>(request => {
                HandleGenericResponse(request, GroupHandler.GetGroups());
                return true;
            }));
            server.AddRoute("GET", "/api/v1/versions", new Func<ServerRequest, bool>(request => {
                HandleGenericResponse(request, GroupHandler.GetVersionsOfGroup(request.queryString.Get("group")));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/creategroup", new Func<ServerRequest, bool>(request => {
                HandleGenericResponse(request, GroupHandler.CreateGroup(GetToken(request)));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/deletegroup", new Func<ServerRequest, bool>(request => {
                HandleGenericResponse(request, GroupHandler.DeleteGroup(GetToken(request), request.bodyString));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/updategroup/", new Func<ServerRequest, bool>(request => {
                HandleGenericResponse(request, GroupHandler.UpdateGroup(GetToken(request), request.bodyString));
                return true;
            }), true, true, true);

            // Users
            server.AddRoute("GET", "/api/v1/me", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.GetUserByToken(GetToken(request)));
                return true;
            }));
            server.AddRoute("GET", "/api/v1/amiloggedin", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.GetLoggedInUser(GetToken(request)));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/login", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.Login(request.bodyString));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/register", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.Register(request.bodyString));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/requestpasswordreset", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.RequestPasswordReset(request.bodyString));
                return true;
            }));
            server.AddRoute("POST", "/api/v1/confirmpasswordreset", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UserSystem.ResetPasswordConfirmed(request.bodyString));
                return true;
            }));

            // Mod upload
            server.AddRoute("POST", "/api/v1/uploadmodfile/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleUploadeOfModFile(request.pathDiff, request.bodyBytes, request.queryString.Get("filename"), GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("DELETE", "/api/v1/removemodfile/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleDeleteUploadedModFile(request.pathDiff.Split('/')[0], request.pathDiff.Split('/')[1], GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/autopopulatemod/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleModFileAutoPopulate(request.pathDiff.Split('/')[0], request.pathDiff.Split('/')[1], GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/startmodupload/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.CreateModUploadSession(GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/updatemodstatus/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.UpdateModStatus(request.pathDiff, request.bodyString, GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("GET", "/api/v1/mod/", new Func<ServerRequest, bool>(request =>
            {
                UploadedMod mod = MongoDBInteractor.GetMod(request.pathDiff);
                if (mod == null) request.SendString("{}", "text/plain", 404);
                else request.SendString(JsonSerializer.Serialize(mod));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/mod/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleUpdateModInfo(request.bodyString, GetToken(request)));
                return true;
            }), false, true, true);
            server.AddRoute("DELETE", "/api/v1/mod/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleDeleteMod(request.pathDiff, GetToken(request)));
                return true;
            }), true, true, true);
            server.AddRoute("GET", "/api/v1/getmods", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, ModQueries.GetMods(request, GetToken(request)));
                return true;
            }), false, true, true);


            server.AddRouteFile("/login", frontend + "login.html", replace, true, true, true);
            server.AddRouteFile("/register", frontend + "register.html", replace, true, true, true);
            server.AddRouteFile("/requestpasswordreset", frontend + "requestpasswordreset.html", replace, true, true, true);
            server.AddRouteFile("/confirmpasswordreset", frontend + "confirmpasswordreset.html", replace, true, true, true);


            server.AddRouteFile("/", frontend + "index.html", replace, true, true, true);

            server.AddRouteFile("/upload", frontend + "upload.html", replace, true, true, true);
            server.AddRouteFile("/mymods", frontend + "mymods.html", replace, true, true, true);
            server.AddRouteFile("/mods", frontend + "mods.html", replace, true, true, true);
            server.AddRouteFile("/approve", frontend + "approve.html", replace, true, true, true);
            server.AddRoute("GET", "/mod/", new Func<ServerRequest, bool>(request =>
            {
                request.SendStringReplace(File.ReadAllText(frontend + "mod.html").Replace("{0}", request.pathDiff), "text/html", 200, replace);
                return true;
            }), true, true, false, true);

            server.AddRouteFile("/groups", frontend + "groups.html", replace, true, true, true);
            server.AddRouteFile("/editgroup", frontend + "editgroup.html", replace, true, true, true);

            server.AddRouteFile("/style.css", frontend + "style.css", true, true, true);
            server.AddRouteFile("/script.js", frontend + "script.js", true, true, true);
            server.StartServer(config.port);
        }
    }
}