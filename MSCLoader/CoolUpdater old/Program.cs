﻿using System;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;
using System.Text;
using System.Linq;

namespace CoolUpdater
{
    class Program
    {
        const string Version = "0.1";

        static bool restartGame;

        const string Downloads = "Downloads";
        const string Temp = "Temp";

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
                    if (args.Length > 1 && args[1] == "restart")
                    {
                        restartGame = true;
                    }

                    UpdateAll();
                    break;
                case "update-modloader":
                    if (args.Length > 1 && args[1] == "restart")
                    {
                        restartGame = true;
                    }

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
            if (url.Contains("nexusmods.com"))
            {
                Thread t = new Thread(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add($"User-Agent: MSCLoaderPro/{Version} ({GetSystemVersion()})");
                        client.Headers.Add($"apikey: {token}");
                        client.DownloadStringCompleted += Client_DownloadStringCompleted;
                        //client.DownloadStringAsync(new Uri("https://api.nexusmods.com/v1/games/mysummercar/mods/146.json")); //MAIN INFO
                        //client.DownloadStringAsync(new Uri("https://api.nexusmods.com/v1/games/mysummercar/mods/146/files.json")); //FILES
                        //client.DownloadStringAsync(new Uri("https://api.nexusmods.com/v1/games/mysummercar/mods/1232/files/146/download_link.json")); // Download link for newest version
                        //client.DownloadStringAsync(new Uri("https://api.nexusmods.com/v1/users/validate.json")); // User Info
                        client.DownloadStringAsync(new Uri(url));
                    }
                });

                t.Start();
            }
            else
            {
                Thread t = new Thread(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("User-Agent: Other");
                        client.DownloadStringCompleted += Client_DownloadStringCompleted;
                        client.DownloadStringAsync(new Uri(url));
                    }
                });

                t.Start();
            }
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

        static string GetSeconds(Stopwatch s)
        {
            return s.Elapsed.TotalSeconds.ToString() + "s";
        }

        private static void UpdateAll()
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                Console.WriteLine($"({GetSeconds(stopwatch)}) Mod Loader Pro Auto-Update Tool Initialized!\n");

                Process[] mscProcess = Process.GetProcessesByName("mysummercar");
                if (mscProcess.Length > 0)
                {
                    foreach (Process process in mscProcess)
                    {
                        Console.WriteLine($"({GetSeconds(stopwatch)}) Killing processes {process.ProcessName}...");
                        process.Kill();
                    }
                    Console.WriteLine($"({GetSeconds(stopwatch)}) All MSC have been killed!\n");
                }

                if (!Directory.Exists(Downloads))
                {
                    throw new DirectoryNotFoundException("Downloads folder doesn't exist!");
                }

                // A small workaround for "not accessible files".
                Console.WriteLine($"({GetSeconds(stopwatch)}) Loading update procedure...\n");
                Thread.Sleep(1000);

                string modsFolder = @"..\Mods";
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                DirectoryInfo di = new DirectoryInfo(Downloads);
                FileInfo[] files = di.GetFiles("*.zip");
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    try
                    {
                        Console.WriteLine($"({GetSeconds(stopwatch)}) ({i}/{files.Length}) Unpacking {file.Name}...");
                        using (ZipFile zip = ZipFile.Read(file.FullName))
                        {
                            zip.ExtractAll(modsFolder, ExtractExistingFileAction.OverwriteSilently);
                        }
                        Console.WriteLine($"({GetSeconds(stopwatch)}) {file.Name} unpacking completed!\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n===============================================\nThere was an error while extracting {file.Name} :(\n\n" + ex.ToString() + "\n\n");
                    }
                }

                // Cleanup after install.
                Directory.Delete(Downloads, true);

                Console.WriteLine($"({GetSeconds(stopwatch)}) All mods have been updated, have a nice day :)");
                if (restartGame)
                {
                    Console.WriteLine($"Restarting game now using Steam");
                    Process cmd = new Process();
                    cmd.StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C start \"\" \"steam://rungameid/516750\""
                    };
                    cmd.Start();
                }
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}