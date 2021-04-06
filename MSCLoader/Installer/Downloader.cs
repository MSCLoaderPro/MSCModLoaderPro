using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Installer
{
    class Downloader
    {
        const string MetadataUrl = "https://api.github.com/repos/MSCLoaderPro/EarlyAccessRelease/releases";
        const string GitHubHeader = "User-Agent: Other";
        const string ZipName = "MSCModLoaderPro";
        string TempPath => Path.Combine(Path.GetTempPath(), "modloaderpro");
        string ZipPath => Path.Combine(TempPath, "modloaderpro.zip");

        string downloadLink;

        bool downloadFinished;
        public bool DownloadFinished => downloadFinished;

        public Downloader()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                Installer.Instance.UpdateStatus(0, "Creating temporary folder...");
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                }

                Installer.Instance.UpdateStatus(0, "Getting latest version info...");

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(GitHubHeader);
                    client.DownloadStringCompleted += Client_DownloadStringCompleted;
                    client.DownloadStringAsync(new Uri(MetadataUrl));
                }
            }
            catch
            {

            }            
        }

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string output = e.Result.Replace(",\"", ",\n\"").Replace(":{", ":\n{\n").Replace("},", "\n},").Replace(":[{", ":[{\n").Replace("}],", "\n}],");
            foreach (string s in output.Split('\n'))
            {
                if (s.Contains("\"tag_name\""))
                {
                    Installer.Instance.SetVersionString(s.Split(':')[1].Replace("\"", "").Replace(",", "").Trim());
                }

                if (s.Contains("\"browser_download_url\"") && s.Contains(ZipName))
                {
                    string[] link = s.Split(':');
                    downloadLink = (link[1] + ":" + link[2]).Replace("\"", "").Replace("}", "").Replace(",", "").Trim();
                    break;
                }
            }
            
            Installer.Instance.UpdateStatus(0, "Latest version found!");

            DownloadModLoader();
        }

        void DownloadModLoader()
        {
            using (WebClient client = new WebClient())
            {
                client.Headers.Add(GitHubHeader);
                client.DownloadFileCompleted += Client_DownloadFileCompleted;
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                client.DownloadFileAsync(new Uri(downloadLink), ZipPath);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Installer.Instance.UpdateStatus(e.ProgressPercentage, $"Downloading ({e.ProgressPercentage}%)...");
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Installer.Instance.UpdateStatus(100, "Mod Loader downloaded successfully!");
            Unpack();
        }

        async void Unpack()
        {
            Installer.Instance.UpdateStatus(100, "Extracting...");
            await Task.Run(() =>
            {
                using (ZipArchive file = ZipFile.OpenRead(ZipPath))
                {
                    string extractPath = Installer.Instance.MscPath;
                    int stage = 0;
                    foreach (var f in file.Entries)
                    {
                        int percentage = (int)(((double)stage / (double)file.Entries.Count) * 100);
                        Installer.Instance.UpdateStatus(percentage, $"Extracting ({percentage}%)...");
                        string path = Path.Combine(extractPath, f.FullName);
                        if (path.EndsWith("/")) continue;
                        string directory = f.FullName.Replace(f.Name, "");
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(Path.Combine(extractPath, directory)))
                        {
                            Directory.CreateDirectory(Path.Combine(extractPath, directory));
                        }

                        // Don't override user settings.
                        if (f.Name == "ModLoaderSettings.ini" && File.Exists(path))
                        {
                            continue;
                        }

                        f.ExtractToFile(path, true);
                        stage++;
                    }
                }
            });
            downloadFinished = true;
            Installer.Instance.TabEnd();
        }
    }
}
