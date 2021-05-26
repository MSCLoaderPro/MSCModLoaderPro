using System;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Win32;
using System.Reflection;

namespace Installer
{
    class Downloader
    {
        const string MetadataUrl = "https://api.github.com/repos/MSCLoaderPro/MSCModLoaderPro/releases";
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

        StreamWriter logWriter;
        public readonly string InstallLogPath;

        public Downloader()
        {
            if (Installer.Instance.OfflineMode)
            {
                UnpackZip(Installer.Instance.OfflineZipPath, true);
                return;
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            InstallLogPath = Path.Combine(TempPath, $"install_log_{DateTime.Now:yyyy-MM-dd-HH-mm}.txt");
            logWriter = new StreamWriter(InstallLogPath);
            Log($"Installer {Installer.Version}");

            try
            {
                Installer.Instance.UpdateStatus(0, "Creating temporary folder...");
                if (!Directory.Exists(TempPath))
                {
                    Directory.CreateDirectory(TempPath);
                    Log($"Created new TempPath.");
                }

                Installer.Instance.UpdateStatus(0, "Getting latest version info...");

                using (WebClient client = new WebClient())
                {
                    client.Headers.Add(GitHubHeader);
                    client.DownloadStringCompleted += Client_DownloadStringCompleted;
                    client.DownloadStringAsync(new Uri(MetadataUrl));
                    Log($"Downloading metadata: {MetadataUrl}");
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
                    string version = s.Split(':')[1].Replace("\"", "").Replace(",", "").Trim();
                    Installer.Instance.SetVersionString(version);
                    Log($"Mod Loader Pro version {version}");
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

        long size;

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

                    try
                    {
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
                        size += new FileInfo(path).Length;
                        Log($"Successfully extracted {f.FullName}");
                    }
                    catch (Exception ex)
                    {
                        Log($"Failed to unpack file {f.FullName}: {ex}");
                    }
                }

                file.Dispose();
            }
            downloadFinished = true;

            Log($"Finished installing. Final size: {size / 100000} MB");

            Installer.Instance.UpdateStatus(100, "Creating registry entries...");
            await Task.Run(() =>
            {
                CreateUninstaller();
            });

            logWriter.Close();
            logWriter = null;

            if (goToEnd)
                Installer.Instance.TabEnd();
        }

        void Log(string input)
        {
            if (logWriter == null) logWriter = new StreamWriter(InstallLogPath);
            logWriter.WriteLine(input);
        }

        internal void DeleteTemporaryFiles()
        {
            try
            {
                if (Directory.Exists(TempPath))
                {
                    Directory.Delete(TempPath, true);
                }
            }
            catch 
            { 

            }
        }

        const string UninstallGuid = "{ef4c06bc-ec46-4bbb-9250-6fc5a25323bf}";

        private void CreateUninstaller()
        {
            if (Process.GetProcessesByName("winlogon").Length == 0) //Linux
            {
                Log("winlogon not found. Skipping Uninstaller creation.");
                return;
            }

            Log("Creating uninstaller.");

            using (RegistryKey parent = Registry.CurrentUser.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                if (parent == null)
                {
                    throw new Exception("Uninstall registry key not found.");
                }
                try
                {
                    RegistryKey key = null;

                    try
                    {
                        string guidText = UninstallGuid;
                        key = parent.OpenSubKey(guidText, true) ??
                              parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new Exception(String.Format("Unable to create uninstaller."));
                        }

                        Assembly asm = GetType().Assembly;
                        Version v = Assembly.LoadFrom(Path.Combine(Installer.Instance.MscPath, "mysummercar_Data/Managed/MSCLoader.dll").Replace("\\", "/")).GetName().Version;
                        string exe = Path.Combine(Installer.Instance.MscPath, "Uninstaller.exe").Replace("/", "\\");


                        key.SetValue("DisplayName", "MSC Mod Loader Pro");
                        key.SetValue("ApplicationVersion", v.ToString());
                        key.SetValue("Publisher", "Mod Loader Pro Team");
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString());
                        key.SetValue("URLInfoAbout", "http://mscloaderpro.github.io/");
                        key.SetValue("Contact", "");
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", $"\"{exe}\"");
                        key.SetValue("EstimatedSize", size / 1000, RegistryValueKind.DWord);
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "An error occurred writing uninstall information to the registry.  The service is fully installed but can only be uninstalled manually through the Uninstaller.exe.",
                        ex);
                    Log("Failed to create uninstaller.");
                }
            }
        }
    }
}
