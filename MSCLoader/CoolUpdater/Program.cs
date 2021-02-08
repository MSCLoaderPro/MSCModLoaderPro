using System;
using System.Threading;
using System.Net;
using System.ComponentModel;

namespace CoolUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Copyright(C) Konrad \"Athlon\" Figura 2021\n\n" +
                                  "This program is a part of MSC Mod Loader Pro.\n" +
                                  "You cannot distribute, modify or use this software outside of Mod Loader Pro,\n" +
                                  "unless you received an agreement from the copyright holder.\n\n" +
                                  "Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            switch (args[0])
            {
                default:
                    throw new ArgumentException("Invalid argument: " + args[0]);
                case "get-metafile":
                    if (!args[1].Contains("github.com") && !args[1].Contains("nexusmods.com"))
                    {
                        throw new UriFormatException("Downloader only supports Nexusmods or GitHub links.");
                    }

                    DownloadMetafile(args[1]);
                    break;
                case "get-file":
                    if (!args[1].Contains("github.com") && !args[1].Contains("nexusmods.com"))
                    {
                        throw new UriFormatException("Downloader only supports Nexusmods or GitHub links.");
                    }

                    if (string.IsNullOrEmpty(args[2]))
                    {
                        throw new Exception("Save path is null or empty.");
                    }

                    DownloadFile(args[1], args[2]);
                    break;
                case "update-all":
                    UpdateAll();
                    break;
                case "update-modloader":
                    UpdateModLoader();
                    break;
            }
        }

        static void DownloadMetafile(string url)
        {
            Thread t = new Thread(() => {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("User-Agent: Other");
                    client.DownloadStringCompleted += Client_DownloadStringCompleted;
                    client.DownloadStringAsync(new Uri(url));
                }
            });

            t.Start();
            Console.ReadKey();
        }

        private static void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            Console.WriteLine(e.Result.Replace(",", ",\n"));
            Environment.Exit(1);
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
            Environment.Exit(1);
        }

        private static void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage + "%");
        }

        private static void UpdateModLoader()
        {
            throw new NotImplementedException();
        }

        private static void UpdateAll()
        {
            throw new NotImplementedException();
        }
    }
}
