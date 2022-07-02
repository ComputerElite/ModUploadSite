using ComputerUtils.CommandLine;
using ComputerUtils.Logging;
using ComputerUtils.QR;
using ComputerUtils.RandomExtensions;
using ComputerUtils.Updating;
using System.Reflection;

namespace ModUploadSite
{
    public class Program
    {
        static void Main(string[] args)
        {
            Logger.displayLogInConsole = true;
            Logger.saveOutputInVariable = true;
            CommandLineCommandContainer cla = new CommandLineCommandContainer(args);
            cla.AddCommandLineArgument(new List<string> { "--workingdir" }, false, "Sets the working Directory for MUS", "directory", "");
            cla.AddCommandLineArgument(new List<string> { "update", "--update", "-U" }, true, "Starts in update mode (use with caution. It's best to let it do on it's own)");
            cla.AddCommandLineArgument(new List<string> { "--displayMasterToken", "-dmt" }, true, "Outputs the master token without starting the server");
            if (cla.HasArgument("help"))
            {
                cla.ShowHelp();
                return;
            }

            string workingDir = cla.GetValue("--workingdir");
            if (workingDir.EndsWith("\"")) workingDir = workingDir.Substring(0, workingDir.Length - 1);

            MUSEnvironment.workingDir = workingDir;
            MUSEnvironment.AddVariablesDependentOnVariablesAndFixAllOtherVariables();
            if (cla.HasArgument("update"))
            {
                Updater.UpdateNetApp(Path.GetFileName(Assembly.GetExecutingAssembly().Location), MUSEnvironment.workingDir);
            }
            MUSEnvironment.config = Config.LoadConfig();
            if (MUSEnvironment.config.masterToken == "") MUSEnvironment.config.masterToken = RandomExtension.CreateToken();
            MUSEnvironment.config.Save();
            //Logger.SetLogFile(workingDir + "Log.log");

            if (cla.HasArgument("-dmt"))
            {
                QRCodeGeneratorWrapper.Display(MUSEnvironment.config.masterToken);
                return;
            }
            Server s = new Server();
            s.StartServer();
        }
    }
}