using ComputerUtils.Webserver;
using ModUploadSite.Populators;
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

            // Init MongoDBClient
            MongoDBInteractor.Initialize();
            PopulatorManager.AddDefaultPopulators();
            ValidationManager.AddDefaultValidators();

            // Mod upload
            server.AddRoute("POST", "/api/v1/uploadmodfile/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleUploadeOfModFile(request.pathDiff, request.bodyBytes, request.queryString.Get("filename")));
                return true;
            }), true, true, true);
            server.AddRoute("DELETE", "/api/v1/removemodfile/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleDeleteUploadedModFile(request.pathDiff.Split('/')[0], request.pathDiff.Split('/')[1]));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/autopopulatemod/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.HandleDeleteUploadedModFile(request.pathDiff.Split('/')[0], request.pathDiff.Split('/')[1]));
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/startmodupload/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.CreateModUploadSession());
                return true;
            }), true, true, true);
            server.AddRoute("POST", "/api/v1/publishmod/", new Func<ServerRequest, bool>(request =>
            {
                HandleGenericResponse(request, UploadHandler.PublishMod(request.pathDiff));
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
                HandleGenericResponse(request, UploadHandler.HandleUpdateModInfo(request.bodyString));
                return true;
            }), false, true, true);



            server.AddRouteFile("/", frontend + "index.html", replace, true, true, true);
            server.AddRouteFile("/upload", frontend + "upload.html", replace, true, true, true);
            server.AddRouteFile("/style.css", frontend + "style.css", true, true, true);
            server.AddRouteFile("/script.js", frontend + "script.js", true, true, true);
            server.StartServer(config.port);
        }
    }
}