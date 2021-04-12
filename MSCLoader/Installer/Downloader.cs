using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Threading;

namespace Installer
{
    class Downloader
    {
        const string MetadataUrl = "https://api.github.com/repos/MSCLoaderPro/EarlyAccessRelease/releases";
        const string GitHubHeader = "User-Agent: Other";
        const string ZipName = "MSCModLoaderPro";
        string TempPath => Path.Combine(Path.GetTempPath(), "modloaderpro");
        string ZipPath => Path.Combine(TempPath, "modloaderpro.zip");

        string downloadLink = "", vsTemplateLink = "", unityTemplateLink = "";

        const string VSTemplateName = "MSC_Mod_Loader_Pro_Template";
        const string UnityTemplateName = "MSCTemplateProject";
        const string DebuggerUrl = "https://github.com/MSCLoaderPro/docs/raw/main/ForCreators/_downloads/debug.zip";

        string VSTemplatePath => Path.Combine(TempPath, "vstemplate.vsix");
        string UnityTemplatePath => Path.Combine(TempPath, "MSCTemplateProject.zip");
        string DebuggerPath => Path.Combine(TempPath, "Debug.zip");

        bool downloadFinished;
        public bool DownloadFinished => downloadFinished;

        public Downloader()
        {
            if (Installer.Instance.OfflineMode)
            {
                UnpackZip(, true);
                return;
            }

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

        bool verFound = false;

        private void Client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            string output = e.Result.Replace(",\"", ",\n\"").Replace(":{", ":\n{\n").Replace("},", "\n},").Replace(":[{", ":[{\n").Replace("}],", "\n}],");
            foreach (string s in output.Split('\n'))
            {
                if (s.Contains("\"tag_name\"") && !verFound)
                {
                    verFound = true;
                    Installer.Instance.SetVersionString(s.Split(':')[1].Replace("\"", "").Replace(",", "").Trim());
                }

                if (s.Contains("\"browser_download_url\""))
                {
                    if (s.Contains(ZipName) && downloadLink == "")
                    {
                        downloadLink = s.ConvertToUrl();
                    }

                    if (s.Contains(VSTemplateName) && vsTemplateLink == "")
                    {
                        vsTemplateLink = s.ConvertToUrl();
                    }

                    if (s.Contains(UnityTemplateName) && unityTemplateLink == "")
                    {
                        unityTemplateLink = s.ConvertToUrl();
                    }
                }

                if (!CustomExtensions.IsAnyNullOrEmpty(downloadLink, vsTemplateLink, unityTemplateLink))
                {
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

        internal async void DownloadDevTools()
        {
            if (Installer.Instance.InstallVSTemplate())
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(GitHubHeader);
                    client.DownloadFileCompleted += Client_DownloadVSTemplateCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileAsync(new Uri(vsTemplateLink), VSTemplatePath);
                    while (client.IsBusy)
                        await Task.Run(() => Thread.Sleep(500));
                }
            }

            if (Installer.Instance.InstallUnityTemplate())
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(GitHubHeader);
                    client.DownloadFileCompleted += Client_DownloadUnityTemplateCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileAsync(new Uri(unityTemplateLink), UnityTemplatePath);
                    while (client.IsBusy)
                        await Task.Run(() => Thread.Sleep(500));
                }
            }

            if (Installer.Instance.InstallDebugger())
            {
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(GitHubHeader);
                    client.DownloadFileCompleted += Client_DownloadDebuggerCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.DownloadFileAsync(new Uri(DebuggerUrl), DebuggerPath);
                    while (client.IsBusy)
                        await Task.Run(() => Thread.Sleep(500));
                }
            }

            Installer.Instance.TabEnd();
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Installer.Instance.UpdateStatus(e.ProgressPercentage, $"Downloading ({e.ProgressPercentage}%)...");
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Installer.Instance.UpdateStatus(100, "Mod Loader downloaded successfully!");
            UnpackZip(ZipPath, true);
        }

        private void Client_DownloadVSTemplateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Installer.Instance.UpdateStatus(100, "VS Template downloaded successfully!");
            System.Diagnostics.Process.Start(VSTemplatePath);
        }

        private void Client_DownloadUnityTemplateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Installer.Instance.UpdateStatus(100, "Unity downloaded successfully!");
            UnpackZip(UnityTemplatePath, false);
        }

        private void Client_DownloadDebuggerCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            Installer.Instance.UpdateStatus(100, "Debugger downloaded successfully!");

            UnpackZip(DebuggerPath, false, Installer.Instance.UserModsFolderName());
        }

        async void UnpackZip(string pathToZip, bool goToEnd = false, string toFolder = "")
        {
            Installer.Instance.UpdateStatus(100, "Extracting...");
            using (ZipArchive file = ZipFile.OpenRead(pathToZip))
            {
                string extractPath = Installer.Instance.MscPath;
                if (toFolder != "")
                    extractPath = Path.Combine(extractPath, toFolder);
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

                    await Task.Run(() =>
                    {
                        f.ExtractToFile(path, true);
                    });
                    stage++;
                }
            }
            downloadFinished = true;

            if (goToEnd)
                Installer.Instance.TabEnd();
        }
    }
}
