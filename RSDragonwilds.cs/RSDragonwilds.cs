using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Engine;
using WindowsGSM.GameServer.Query;

namespace WindowsGSM.Plugins
{
    public class RSDragonwilds : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.RSDragonwilds", // WindowsGSM.XXXX
            author = "darkharasho",
            description = "WindowsGSM plugin for supporting RuneScape: Dragonwilds Dedicated Server",
            version = "0.1.1",
            url = "https://github.com/darkharasho/WindowsGSM.RunescapeDragonwilds", // Github repository link (Best practice)
            color = "#8B0000" // Color Hex
        };

        // - Standard Constructor and properties
        public RSDragonwilds(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;
        public string Error, Notice;

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "4019830"; /* https://dragonwilds.runescape.wiki/w/Dedicated_Servers */

        // - Game server Fixed variables
        // Point directly at the Unreal shipping binary (not the small RSDragonwildsServer.exe stub in the
        // install root) so WindowsGSM tracks the real server process instead of a launcher that exits immediately.
        public override string StartPath => @"RSDragonwilds\Binaries\Win64\RSDragonwildsServer-Win64-Shipping.exe"; // Game server start path
        public string FullName = "RuneScape: Dragonwilds Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;  // Does this server support output redirect?
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation (7777 -> 7778)
        public object QueryMethod = null; // Dragonwilds does not expose an A2S/Steam query endpoint

        // - Game server default values
        public string ServerName = "RuneScape Dragonwilds Server";
        public string Defaultmap = "My World"; // DefaultWorldName
        public string Maxplayers = "6"; // Server is hard capped at 6 players by the game
        public string Port = "7777"; // Game port (UDP)
        public string QueryPort = "7778"; // Not used for querying; reserved so a second instance does not collide
        public string Additional = "-log"; // Recommended launch options (the wiki also suggests -NewConsole for a standalone window)

        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            // DedicatedServer.ini lives under Saved\Config\WindowsServer and is created on first launch.
            // We seed a default so the server can start without the user editing files blindly.
            string configPath = Path.Combine(
                ServerPath.GetServersServerFiles(_serverData.ServerID),
                "RSDragonwilds", "Saved", "Config", "WindowsServer", "DedicatedServer.ini");

            Directory.CreateDirectory(Path.GetDirectoryName(configPath));

            if (File.Exists(configPath))
            {
                return; // Don't clobber an existing config (e.g. on re-install)
            }

            var sb = new StringBuilder();
            sb.AppendLine("[/Script/Dominion.DedicatedServerSettings]");
            sb.AppendLine("; OwnerId is REQUIRED - the server will not start without it.");
            sb.AppendLine("; Find your Player ID at the bottom of the in-game Settings menu.");
            sb.AppendLine("OwnerId=");
            sb.AppendLine($"ServerName={_serverData.ServerName}");
            sb.AppendLine($"DefaultWorldName={Defaultmap}");
            sb.AppendLine("AdminPassword=");
            sb.AppendLine("; WorldPassword - leave empty for a public world.");
            sb.AppendLine("WorldPassword=");

            await Task.Run(() => File.WriteAllText(configPath, sb.ToString()));
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            string exePath = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);
            if (!File.Exists(exePath))
            {
                Error = $"{Path.GetFileName(exePath)} not found ({exePath})";
                return null;
            }

            // Warn (but do not block) if the mandatory OwnerId hasn't been set.
            string configPath = Path.Combine(
                ServerPath.GetServersServerFiles(_serverData.ServerID),
                "RSDragonwilds", "Saved", "Config", "WindowsServer", "DedicatedServer.ini");
            if (File.Exists(configPath) && !File.ReadAllText(configPath).Contains("OwnerId="))
            {
                Notice = "OwnerId is empty in DedicatedServer.ini - the server will not start until it is set.";
            }

            StringBuilder param = new StringBuilder();
            param.Append($" {_serverData.ServerParam} ");
            param.Append($"-port={_serverData.ServerPort} ");

            // Prepare Process
            var p = new Process
            {
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = exePath,
                    Arguments = param.ToString().Trim(),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                },
                EnableRaisingEvents = true
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                var serverConsole = new ServerConsole(_serverData.ServerID);
                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            // Start Process
            try
            {
                p.Start();
                if (AllowsEmbedConsole)
                {
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                }

                return p;
            }
            catch (Exception e)
            {
                Error = e.Message;
                return null; // return null if fail to start
            }
        }

        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
            });
            await Task.Delay(2000);
        }

        // - Update server function
        public async Task<Process> Update(bool validate = false, string custom = null)
        {
            var (p, error) = await Installer.SteamCMD.UpdateEx(serverData.ServerID, AppId, validate, custom: custom, loginAnonymous: loginAnonymous);
            Error = error;
            await Task.Run(() => { p.WaitForExit(); });
            return p;
        }

        public bool IsInstallValid()
        {
            return File.Exists(Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath));
        }

        public bool IsImportValid(string path)
        {
            string exePath = Path.Combine(path, StartPath);
            Error = $"Invalid Path! Fail to find {Path.GetFileName(exePath)}";
            return File.Exists(exePath);
        }

        public string GetLocalBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return steamCMD.GetLocalBuild(_serverData.ServerID, AppId);
        }

        public async Task<string> GetRemoteBuild()
        {
            var steamCMD = new Installer.SteamCMD();
            return await steamCMD.GetRemoteBuild(AppId);
        }
    }
}
