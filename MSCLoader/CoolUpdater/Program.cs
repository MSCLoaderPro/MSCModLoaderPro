using System;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace CoolUpdater
{
    class Program
    {
        const string Version = "0.1";

        public const string Downloads = "Downloads";
        public const string Temp = "Temp";

        const string NexusHeader = "User-Agent: MSCLoaderPro/{0} ({1})";
        const string GitHubHeader = "User-Agent: Other";
        const string ApiKeyFormat = "apikey: {0}";

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Copyright(C) Konrad \"Athlon\" Figura 2021\n\n" +
                                  "This program is a part of MSC Mod Loader Pro.\n" +
                                  "You cannot distribute, modify or use this software outside of Mod Loader Pro,\n" +
                                  "unless you've received an agreement from the copyright holder.\n\n" +
                                  "Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
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
                    if (!args[1].Contains("github.com") && !args[1].Contains("nexusmods.com"))
                    {
                        throw new UriFormatException("Downloader only supports Nexusmods or GitHub links.");
                    }

                    if (args.ElementAtOrDefault(2) == null)
                    {
                        throw new Exception("Save path is null or empty.");
                    }

                    DownloadFile(args[1], args[2]);
                    break;
                case "update-all":
                    UpdateView view = new UpdateView();
                    Application.Run(view);
                    break;
                case "update-modloader":
                    UpdateModLoader();
                    break;
            }
        }

        static string GetSystemVersion()
        {
            return Environment.OSVersion.VersionString;
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
            Console.WriteLine(e.Result.Replace(",\"", ",\n\"").Replace(":{", ":\n{\n").Replace("},", "\n},").Replace(":[{", ":[{\n").Replace("}],", "\n}],"));
            Environment.Exit(0);
        }

        private static void DownloadFile(string url, string savepath)
        {
            Thread t = new Thread(() =>
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");

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

        private static void UpdateModLoader()
        {
            throw new NotImplementedException();
        }       
    }
}