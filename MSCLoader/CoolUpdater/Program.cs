using System;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.IO;

namespace CoolUpdater
{
    class Program
    {
        public const string Downloads = "Downloads";
        public const string Temp = "Temp";

        const string NexusHeader = "User-Agent: mscmodloaderpro/{0} ({1})";
        const string GitHubHeader = "User-Agent: Other";
        const string ApiKeyFormat = "apikey: {0}";

        readonly static string[] AllowedDownloadSources = { "github.com", "githubusercontent.com", "nexusmods.com", "gravatar.com" };

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Info view = new Info();
                Application.Run(view);
            }

            switch (args[0])
            {
                default:
                    throw new ArgumentException("Invalid argument: " + args[0]);
                case "get-metafile":
                    string link, token = "";

                    if (args.Length < 2)
                    {
                        throw new ArgumentException("Missing argument");
                    }

                    if (!args[1].Contains("github.com") && !args[1].Contains("nexusmods.com"))
                    {
                        throw new UriFormatException("Downloader only supports Nexusmods or GitHub links.");
                    }
                    else
                    {
                        link = args[1];
                    }

                    if (args[1].Contains("nexusmods.com"))
                    {
                        if (args.ElementAtOrDefault(2) == null)
                        {
                            throw new ArgumentException("Missing user token!");
                        }
                        token = args[2];
                    }

                    DownloadMetafile(link, token);
                    break;
                case "get-file":
                    //if (!args[1].Contains("github.com") && !args[1].Contains("nexusmods.com") && !args[1].Contains("gravatar.com
                    if (AllowedDownloadSources.Contains(args[1]))
                    {
                        throw new UriFormatException("Downloader only supports Nexusmods or GitHub links.");
                    }

                    if (args.ElementAtOrDefault(2) == null)
                    {
                        throw new Exception("Save path is null or empty.");
                    }

                    string token2 = "";
                    if (args[1].Contains("nexusmods.com"))
                    {
                        token2 = args[3];
                    }

                    DownloadFile(args[1], args[2].Replace("%20", " "), token2);
                    break;
                case "update-all":
                    if (!IsUserAdministrator())
                    {
                        // Restart as an admin, if no admin right has been given
                        Process p = new Process();
                        p.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
                        p.StartInfo.Arguments = string.Join(" ", args);
                        p.StartInfo.Verb = "runas";
                        p.Start();
                        Environment.Exit(0);
                        return;
                    }

                    string pathToMods = args.Length < 2 ? "" : args[1].Replace("%20", " ");
                    string mscPath = args.Length < 3 ? "" : args[2].Replace("%20", " ");
                    UpdateView view = new UpdateView(pathToMods, mscPath);
                    Application.Run(view);
                    break;
                case "nexus-login":
                    if (Process.GetCurrentProcess().Parent().ProcessName == "mysummercar")
                    {
                        string tok = "";
                        if (args.Length > 1)
                        {
                            tok = args[1];
                        }
                        new NexusLoginSystem(tok);
                    }
                    else
                    {
                        Console.WriteLine("Parent process is not a My Summer Car.");
                        Environment.Exit(0);
                    }

                    break;
            }
        }

        static string GetSystemVersion()
        {
            return Environment.OSVersion.VersionString;
        }

        static string Version
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return version.Major + "." + version.Minor + "." + version.Build;
            }
        }

        static void DownloadMetafile(string url, string token = "")
        {
            bool nexus = url.Contains("nexusmods.com");

            Thread t = new Thread(() =>
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(nexus ? string.Format(NexusHeader, Version, GetSystemVersion()) : GitHubHeader);
                    if (nexus)
                    {
                        client.Headers.Add(string.Format(ApiKeyFormat, token));
                    }
                    client.DownloadStringCompleted += Client_DownloadStringCompleted;
                    client.DownloadStringAsync(new Uri(url));

                }
            });
            t.Start();
            Console.ReadKey();
        }

        private static void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                Console.WriteLine(e.Result.Replace(",\"", ",\n\"").Replace(":{", ":\n{\n").Replace("},", "\n},").Replace(":[{", ":[{\n").Replace("}],", "\n}],"));
            }
            catch (TargetInvocationException ex)
            {
                if (ex.ToString().Contains("(401) Unauthorized"))
                {
                    Console.WriteLine("ERROR:401");
                }

                if (ex.ToString().Contains("(403) Forbidden"))
                {
                    Console.WriteLine("ERROR:401");
                }
            }
            Environment.Exit(0);
        }

        private static void DownloadFile(string url, string savepath, string token = "")
        {
            bool nexus = url.Contains("nexusmods.com");
            Thread t = new Thread(() =>
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.Headers.Add(nexus ? string.Format(NexusHeader, Version, GetSystemVersion()) : GitHubHeader);
                    if (nexus)
                    {
                        client.Headers.Add(string.Format(ApiKeyFormat, token));
                    }

                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadFileAsync(new Uri(url), savepath);
                }
            });
            t.Start();
            Console.ReadKey();
        }


        private static void Client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Environment.Exit(0);
        }

        private static void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage + "%");
        }

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}