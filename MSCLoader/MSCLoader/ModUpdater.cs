using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using MSCLoader.Helper;
using MSCLoader.NexusMods;
using Newtonsoft.Json;
using UnityEngine.Events;

#pragma warning disable CS1591
namespace MSCLoader
{
    // Copyright (C) Konrad Figura 2021
    // This file is a part of MSCLoader Pro.
    // You cannot use it any other project.
    public class ModUpdater : MonoBehaviour
    {
        static ModUpdater instance;
        public static ModUpdater Instance => instance;

        public GameObject headerUpdateAllButton;
        public GameObject headerProgressBar;
        public Slider sliderProgressBar;
        public Text textProgressBar;
        public Text menuLabelUpdateText;

        string message;

        const int MaxDots = 3;

        bool isBusy;
        public bool IsBusy => isBusy;

        internal static string UpdaterDirectory => Path.Combine(Directory.GetCurrentDirectory(), "ModUpdater");
        internal static string UpdaterPath => Path.Combine(UpdaterDirectory, "CoolUpdater.exe");
        string DownloadsDirectory => Path.Combine(UpdaterDirectory, "Downloads");
        const int TimeoutTime = 10; // in seconds.
        const int TimeoutTimeDownload = 60; // in seconds.

        bool autoUpdateChecked;

        ModUpdaterDatabase modUpdaterDatabase;

        const string ModLoaderApiUri = "https://api.github.com/repos/MSCLoaderPro/MSCModLoaderPro/releases";
        const string InstallerApiUri = "https://api.github.com/repos/MSCLoaderPro/docs/releases/latest";
        string modLoaderLatestVersion;
        bool modLoaderUpdateAvailable, installModLoaderUpdate;
        string TempPathModLoaderPro => Path.Combine(Path.GetTempPath(), "modloaderpro");
        const string InstallerName = "installer.exe";
        string InstallerPath => Path.Combine(TempPathModLoaderPro, InstallerName);

        static string SourcesPath => "Sources.txt";
        static string UserSourcesPath => "UserSources.txt";
        const string UserSourcesContent = "// In this file you can add your own sources, in similar fashion as in Sources.txt\n" +
                                           "// This file will NOT be overwritten by future updates (unlike Sources.txt).\n" +
                                           "// Remember that only NexusMods and GitHub links are supported by Mod Loader Pro!\n" +
                                           "// You can add your own sources by adding new line to this text file and following the example:\n" +
                                           "// ModID https://nexusmods.com/mysummercar/mods/xxxxxx \n" +
                                           "// or\n" +
                                           "// ModID https://github.com/user/repository \n\n" +
                                           "// Empty lines and lines starting with double-slash will be skipped\n";
        List<Mod> backwardCompatibilityMods;

        bool gitHubLimitExceeded;

        public ModUpdater()
        {
            instance = this;
        }

        void Start()
        {
            modUpdaterDatabase = new ModUpdaterDatabase();
            backwardCompatibilityMods = new List<Mod>();

            // First we read the Sources.txt file, and populate UpdateLink of mods that don't have a source.
            if (File.Exists(SourcesPath))
            {
                ReadSources(SourcesPath);
            }

            if (!File.Exists(UserSourcesPath))
            {
                File.WriteAllText(UserSourcesPath, UserSourcesContent);
            }
            else
            {
                ReadSources(UserSourcesPath);
            }

            // Populate list from the database.
            if (modUpdaterDatabase.GetAll().Count > 0)
            {
                foreach (var f in modUpdaterDatabase.GetAll())
                {
                    Mod mod = ModLoader.LoadedMods.FirstOrDefault(m => m.ID == f.Key);
                    if (mod != null)
                    {
                        mod.ModUpdateData = f.Value;

                        if (IsNewerVersionAvailable(mod.Version, mod.ModUpdateData.LatestVersion))
                        {
                            mod.ModUpdateData.UpdateStatus = UpdateStatus.Available;
                            mod.modListElement.ToggleUpdateButton(true);
                            headerUpdateAllButton.SetActive(true);
                            ModLoader.modContainer.UpdateModCountText();
                        }
                        else
                        {
                            mod.ModUpdateData.UpdateStatus = UpdateStatus.NotChecked;
                        }
                    }
                }

                StartCoroutine(NotifyUpdatesAvailable());
            }

            if (ShouldCheckForUpdates())
            {
                autoUpdateChecked = true;
                LookForUpdates();
                return;
            }
        }

        void LookForUpdates()
        {
            if (IsBusy)
            {
                ModPrompt.CreatePrompt("MOD LOADER IS BUSY LOOKING FOR UPDATES.", "MOD UPDATER");
                return;
            }
            
            if (!File.Exists(UpdaterPath))
            {
                ModPrompt.CreatePrompt("Cannot check for updates, because the updater component is missing!\n\nPlease reinstall Mod Loader Pro.", "Fatal Error");
                return;
            }

            isBusy = true;
            message = "LOOKING FOR MOD LOADER UPDATES PRO";
            currentSliderText = UpdateSliderText();
            StartCoroutine(currentSliderText);

            StartCoroutine(PullMetadata(ReadModLoaderProUpdate, ModLoaderApiUri));
        }

        #region Mod Loader Pro Update Part
        void ReadModLoaderProUpdate()
        {
            NexusMods.JSONClasses.GitHub.GitHubReleases[] fs = JsonConvert.DeserializeObject<NexusMods.JSONClasses.GitHub.GitHubReleases[]>(lastDataOut);
            NexusMods.JSONClasses.GitHub.GitHubReleases f = fs[0];

            bool modLoaderUpdateAvailable = false;
            string modLoaderLatestVersion = f.tag_name;

            bool isRemoteRC, isLocalRC = false;
            isRemoteRC = f.tag_name.Contains("-RC");
            isLocalRC = ModLoader.Version.Contains("-RC");

            string modLoaderLatestDisplay = f.tag_name;
            if (isRemoteRC)
                modLoaderLatestVersion = f.tag_name.Replace("-RC", ".");

            string localVersion = ModLoader.Version;
            if (isLocalRC)
                localVersion = localVersion.Replace("-RC", ".");

            modLoaderUpdateAvailable = IsNewerVersionAvailable(localVersion, modLoaderLatestVersion);

            if (!isRemoteRC && isLocalRC)
            {
                modLoaderUpdateAvailable = true;
            }

            if (modLoaderUpdateAvailable)
            {
                message = "WAITING FOR USER DECISION";
                ModPrompt.CreateYesNoPrompt($"Mod Loader Pro update is available to download!\n\n" +
                    $"Your version is <color=yellow>{ModLoader.Version}</color> and the newest available is <color=yellow>{modLoaderLatestDisplay}</color>.\n\n" +
                    $"Would you like to download it now?", "Mod Loader Update Available!", DownloadModLoaderUpdate, UpdateAllMods);
            }
            else
            {
                UpdateAllMods();
            }
        }

        void DownloadModLoaderUpdate()
        {
            message = "DOWNLOADING THE MOD LOADER PRO INSTALLER";
            StartCoroutine(PullMetadata(OnInstallerMetadataPulled, InstallerApiUri));
        }

        void OnInstallerMetadataPulled()
        {
            var json = JsonConvert.DeserializeObject<NexusMods.JSONClasses.GitHub.GitHubReleases>(lastDataOut);
            string url = json.assets[0].browser_download_url;
            StartCoroutine(DownloadFile(OnInstallerDownloaded, url, InstallerPath));
        }

        void OnInstallerDownloaded()
        {
            installModLoaderUpdate = true;

            if (File.Exists(InstallerPath))
            {
                ModPrompt.CreateYesNoPrompt("Mod Loader Pro will update after you quit the game. Would you like to do that now?", 
                                            "Mod Loader Pro Update is ready!", 
                                            Application.Quit, 
                                            onPromptClose: () => { isBusy = false; });
            }
        }

        bool waitForInstall;

        void StartInstaller()
        {
            string pathToGame = Path.GetFullPath(ModLoader.ModsFolder).Replace("\\" + MSCLoader.settings.ModsFolderPath, "").Replace(" ", "%20");

            Process p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C \"\" {InstallerName} fast-install {pathToGame}",
                    WorkingDirectory = TempPathModLoaderPro,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            p.Start();
            ModPrompt.CreatePrompt(pathToGame);
        }
        #endregion
        #region Look For Mod Updates
        void UpdateAllMods()
        {
            LookForModUpdates(ModLoader.LoadedMods.Where(x => !string.IsNullOrEmpty(x.UpdateLink)));
        }

        void LookForModUpdates(IEnumerable<Mod> mods)
        {
            if (mods.Count() == 0)
            {
                isBusy = false;
                return;
            }

            isBusy = true;
            message = "LOOKING FOR MOD UPDATES";
            if (currentSliderText == null) StartCoroutine(UpdateSliderText());
            
            modsToCheck = mods.ToList();

            if (!NexusSSO.Instance.IsValid)
            {
                ModPrompt prompt = ModPrompt.CreateYesNoPrompt("Looks like you're not logged into NexusMods.\n" +
                                                          "Some mods require NexusMods to be able to check for updates.\n\n" +
                                                          "Are you sure you want to continue?", "Mod Updater", () => StartModUpdateCheck(), () => { isBusy = false; modsToCheck = null; });
            }
            else
            {
                StartModUpdateCheck();
            }

        }

        List<Mod> modsToCheck;
        void StartModUpdateCheck(int lastIndex = -1)
        {
            if (lastIndex == -1)
            {
                sliderProgressBar.value = 0;
                sliderProgressBar.maxValue = modsToCheck.Count();
                headerProgressBar.SetActive(true);
            }

            if (lastIndex + 1 < modsToCheck.Count)
            {
                for (int i = lastIndex + 1; i < modsToCheck.Count; i++)
                {
                    sliderProgressBar.value = i;

                    Mod mod = modsToCheck[i];
                    string url = modsToCheck[i].UpdateLink;
                    ModConsole.Log($"\nLooking for update of {mod.Name}");

                    // Formatting the link.
                    if (url.Contains("github.com"))
                    {
                        if (gitHubLimitExceeded) continue;

                        // If is not direct api.github.com link, modify it so it matches it correctly.
                        if (!url.Contains("api."))
                        {
                            url = url.Replace("https://", "").Replace("www.", "").Replace("github.com/", "");
                            url = "https://api.github.com/repos/" + url;
                        }

                        if (!url.EndsWith("/releases/latest"))
                        {
                            url += "/releases/latest";
                        }
                        else if (!url.EndsWith("releases/latest"))
                        {
                            url += "releases/latest";
                        }

                        ModConsole.Log($"URL: {url}");
                        ModConsole.Log($"Starting checking for update process...");

                        StartCoroutine(PullMetadata(() => GithubLatestRelease(mod, url, i), url));
                        return;
                    }
                    else if (url.Contains("nexusmods.com"))
                    {
                        // SAMPLE: https://www.nexusmods.com/mysummercar/mods/146
                        // First we need mod ID.
                        if (!NexusSSO.Instance.IsValid)
                        {
                            ModConsole.LogError("Mods that use NexusMods for its updates require user authentication API key. Please provide one first.");
                            continue;
                        }

                        string modID = url.Split('/').Last();
                        string mainModInfo = $"https://api.nexusmods.com/v1/games/mysummercar/mods/{modID}.json";

                        if (string.IsNullOrEmpty(NexusSSO.Instance.ApiKey))
                        {
                            ModConsole.LogError("NexusMods API key is empty.");
                            continue;
                        }

                        // Now we are getting version info.
                        StartCoroutine(PullMetadata(() => NexusModInfo(mod, i), mainModInfo, NexusSSO.Instance.ApiKey));
                        return;
                    }
                }
            }

            // We finally checked for all mod updates.
            sliderProgressBar.value = sliderProgressBar.maxValue;

            // SHOW THE UPDATE ALL BUTTON THEN UPDATE THE MOD COUNT LABEL TO REFLECT HOW MANY MODS HAVE UPDATES AVAILABLE!
            headerUpdateAllButton.SetActive(modsToCheck.Any(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available));
            ModLoader.modContainer.UpdateModCountText();

            IEnumerable<Mod> modsWithUpdates = modsToCheck.Where(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available);
            if (modsWithUpdates.Count() > 0)
            {
                switch (ModLoader.modLoaderSettings.UpdateMode)
                {
                    case 1: // Notify only
                        string modNames = "";
                        foreach (var mod in modsWithUpdates)
                            modNames += $"{mod.Name}, ";
                        modNames = modNames.Remove(modNames.Length - 2, 1);
                        ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                        prompt.Text = $"MOD UPDATE IS AVAILABLE FOR THE FOLLOWING MODS:\n\n<color=yellow>{modNames}</color>\n\n" +
                                      $"YOU CAN USE \"UPDATE ALL MODS\" BUTTON TO QUICKLY UPDATE THEM.";
                        prompt.Title = "MOD UPDATER";
                        prompt.AddButton("UPDATE ALL MODS", () => UpdateAll());
                        prompt.AddButton("CLOSE", null);
                        break;
                    case 2: // Download
                        UpdateAll();
                        break;
                }
            }

            isBusy = false;
        }
        #endregion
        void GithubLatestRelease(Mod mod, string url, int lastIndex)
        {
            bool error = false;

            if (lastDataOut.Contains("(403) Forbidden") && url.Contains("github.com"))
            {
                gitHubLimitExceeded = true;
                ModConsole.LogError("User exceeded API rate limit request on GitHub. Please try checking for updates again in an hour");
            }
            else
            {
                mod.ModUpdateData = new ModUpdateData();

                // Reading the metadata file info that we want.
                try
                {
                    if (url.Contains("github.com"))
                    {
                        if (lastDataOut.Contains("\"message\": \"Not Found\"") || lastDataOut.Contains("(404) Not Found"))
                        {
                            throw new Exception($"Mod Updater: Mod {mod.ID}'s GitHub repository returned \"Not found\" status.");
                        }

                        NexusMods.JSONClasses.GitHub.GitHubReleases release = JsonConvert.DeserializeObject<NexusMods.JSONClasses.GitHub.GitHubReleases>(lastDataOut);
                        mod.ModUpdateData.LatestVersion = release.tag_name;
                        
                        // Get latest (prefer ones which end with .pro.zip extension).
                        for (int i = 0; i < release.assets.Count; i++)
                        {
                            NexusMods.JSONClasses.GitHub.Asset asset = release.assets[i];
                            if (asset.browser_download_url.EndsWith(".pro.zip"))
                            {
                                mod.ModUpdateData.ZipUrl = asset.browser_download_url;
                                break;
                            }

                            mod.ModUpdateData.ZipUrl = asset.browser_download_url;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModConsole.LogError($"An error has occured while reading the metadata of {mod.Name}:\n\n{ex}");
                    error = true;
                }
            }

            if (!error)
                CheckIfNewerVersionAvailable(mod);
            StartModUpdateCheck(lastIndex); // Continue where we left off.
        }
        #region Nexus
        void NexusModInfo(Mod mod, int lastIndex)
        {
            mod.ModUpdateData = new ModUpdateData();
            bool error = false;
            try
            {
                var modInfo = JsonConvert.DeserializeObject<NexusMods.JSONClasses.NexusMods.ModInfo>(lastDataOut);
                mod.ModUpdateData.LatestVersion = modInfo.version;
                mod.ModUpdateData.Summary = modInfo.summary;
                mod.ModUpdateData.PictureUrl = modInfo.picture_url;
                mod.ModUpdateData.ModID = modInfo.mod_id;

                if (backwardCompatibilityMods.Contains(mod))
                {
                    NexusBackwardCompatibilityPullStuff(mod, lastDataOut);
                }
            }
            catch (Exception ex)
            {
                ModConsole.LogError(ex.ToString());
                error = true;
            }

            if (!error)
                CheckIfNewerVersionAvailable(mod);

            if (NexusSSO.Instance.IsPremium && !error)
            {
                string modFiles = $"https://api.nexusmods.com/v1/games/mysummercar/mods/{mod.ModUpdateData.ModID}/files.json?category=main";
                StartCoroutine(PullMetadata(() => NexusModFiles(mod, lastIndex), modFiles, NexusSSO.Instance.ApiKey));
            }
            else
            {
                mod.ModUpdateData.ZipUrl = mod.UpdateLink;
                StartModUpdateCheck(lastIndex); // Continue where we left off.
            }
        }

        void NexusBackwardCompatibilityPullStuff(Mod mod, string jsonString)
        {
            ModConsole.Log($"Pulling mod info from NexusMods for {mod.ID}.");
            string modInfoPath = Path.Combine(NexusSSO.NexusDataFolder, mod.ID);
            if (!Directory.Exists(modInfoPath))
            {
                Directory.CreateDirectory(modInfoPath);
            }

            string infoFile = Path.Combine(modInfoPath, "ModInfo.json");
            File.WriteAllText(infoFile, jsonString);
            mod.Description = mod.ModUpdateData.Summary;

            string icon = Path.Combine(modInfoPath, "icon.png");

            UnityAction postDownloadAction = () => {
                try
                {
                    if (File.Exists(icon))
                    {
                        Texture2D iconTexture = new Texture2D(1, 1);
                        byte[] array = File.ReadAllBytes(icon);
                        iconTexture.LoadImage(array);
                        mod.modListElement.SetModIcon(iconTexture);
                        mod.Description = mod.ModUpdateData.Summary;
                    }
                    else
                    {
                        ModConsole.LogError("Could not download mod picture :( " + mod.ModUpdateData.PictureUrl);
                    }
                } catch (Exception ex)
                {
                    ModConsole.LogError(ex.ToString());
                }
            };

            StartCoroutine(DownloadFile(postDownloadAction, mod.ModUpdateData.PictureUrl, icon, NexusSSO.Instance.ApiKey));
        }

        void NexusModFiles(Mod mod, int lastIndex)
        {
            try
            {
                var modFiles = JsonConvert.DeserializeObject<NexusMods.JSONClasses.NexusMods.ModFiles>(lastDataOut);

                int fileID = -1;

                for (int i = 0; i < modFiles.files.Count; i++)
                {
                    var file = modFiles.files[i];
                    if (file.mod_version == mod.ModUpdateData.LatestVersion)
                    {
                        fileID = file.file_id;

                        if (file.file_name.Contains("Mod Loader Pro") || file.file_name.Contains("ProLoader"))
                        {
                            break;
                        }
                    }
                }

                if (fileID != -1)
                {
                    string requestDownloads = $"https://api.nexusmods.com/v1/games/mysummercar/mods/{mod.ModUpdateData.ModID}/files/{fileID}/download_link.json";
                    StartCoroutine(DownloadFile(() => NexusDownloadSources(mod, lastIndex), requestDownloads, NexusSSO.Instance.ApiKey));
                }
            }
            catch
            {
                StartModUpdateCheck(lastIndex);
            }
        }

        void NexusDownloadSources(Mod mod, int lastIndex)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<NexusMods.JSONClasses.NexusMods.DownloadSources[]>(lastDataOut);
                mod.ModUpdateData.ZipUrl = json[0].URI;
            }
            catch
            {
            }
            StartModUpdateCheck(lastIndex);
        }

        #endregion
        #region DownloadUpdates
        public void UpdateAll()
        {
            if (isBusy) return;

            Mod[] mods = ModLoader.LoadedMods.Where(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available).ToArray();
            foreach (Mod mod in mods)
            {
                if (!updateDownloadQueue.Contains(mod))
                {
                    updateDownloadQueue.Add(mod);
                }
            }

            StartDownload();
        }
        #endregion

        // Helper functions.
        #region External Program
        static string lastDataOut;
        static string lastDownloadData;
        Process currentProcess;
        /// <summary>
        /// Arguments:<br></br><br></br>
        /// 1 - Link<br></br>
        /// 2 - (Optional) Nexus Token<br></br>
        /// </summary>
        IEnumerator PullMetadata(UnityAction onEnd, params string[] args)
        {
            while (currentProcess != null && !currentProcess.HasExited) // wait for it to exit first.
                yield return null;

            string oldMessage = message;
            message = "COMMUNICATING WITH NEXUSMODS";
            int waitTime = 0;
            while (!NexusSSO.Instance.IsReady)
            {
                waitTime++;
                if (waitTime > 20)
                    break;

                yield return new WaitForSeconds(1);
            }

            message = oldMessage;

            currentProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UpdaterPath,
                    Arguments = "get-metafile " + string.Join(" ", args),
                    WorkingDirectory = UpdaterDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            lastDataOut = "";
            currentProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            currentProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.BeginErrorReadLine();

            int timer = 0;
            while (!currentProcess.HasExited)
            {
                timer++;
                if (timer > TimeoutTime)
                {
                    ModConsole.LogError("Pulling metadata timed out: " + args[0]);
                    currentProcess.Kill();
                    break;
                }

                yield return new WaitForSeconds(1);
            }

            onEnd();
        }

        /// <summary>
        /// Arguments:<br></br><br></br>
        /// 1 - Link<br></br>
        /// 2 - Download Path<br></br>
        /// 3 - (Optional) Nexus Token<br></br>
        /// </summary>
        IEnumerator DownloadFile(UnityAction onEnd, params string[] args)
        {
            while (currentProcess != null && !currentProcess.HasExited) // wait for it to exit first.
                yield return null;

            for (int i = 0; i < args.Length; i++)
            {
                args[i] = "\"" + args[i] + "\"";
            }

            currentProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = UpdaterPath,
                    Arguments = "get-file " + string.Join(" ", args),
                    WorkingDirectory = UpdaterDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            lastDownloadData= "";
            currentProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandlerDownload);
            currentProcess.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandlerDownload);

            currentProcess.Start();
            currentProcess.BeginOutputReadLine();
            currentProcess.BeginErrorReadLine();

            int timer = 0;
            while (!currentProcess.HasExited)
            {
                timer++;
                if (timer > TimeoutTimeDownload)
                {
                    ModConsole.LogError("Could not download file: " + string.Join(" ", args));
                    currentProcess.Kill();
                    break;
                }
                yield return new WaitForSeconds(1);
            }

            onEnd();
        }

        static void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            UnityEngine.Debug.Log(e.Data);
            lastDataOut += e.Data + "\n";
        }

        static void OutputHandler(object sendingProcess, DataReceivedEventArgs e)
        {
            lastDataOut += e.Data + "\n";
        }

        static void ErrorHandlerDownload(object sender, DataReceivedEventArgs e)
        {
            UnityEngine.Debug.Log(e.Data);
            lastDownloadData += e.Data + "\n";
        }

        static void OutputHandlerDownload(object sendingProcess, DataReceivedEventArgs e)
        {
            lastDownloadData += e.Data + "\n";
        }

        #endregion
        #region Waiting for install
        // Unity function: https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html
        void OnApplicationQuit()
        {
            modUpdaterDatabase.Save();

            if (currentProcess != null && !currentProcess.HasExited)
            {
                currentProcess.Kill();
            }

            // Mod Loader update has a priority over mods.
            if (installModLoaderUpdate)
            {
                StartInstaller();
                return;
            }

            if (waitForInstall)
            {
                string mscPath = Application.dataPath.Replace("mysummercar_Data", "").Replace(" ", "%20");

                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"\" CoolUpdater.exe update-all {Path.GetFullPath(ModLoader.ModsFolder).Replace(" ", "%20")} {mscPath}",
                        WorkingDirectory = UpdaterDirectory,
                        UseShellExecute = true
                    }
                };

                p.Start();
            }
        }
        #endregion
        #region UI
        IEnumerator currentSliderText;
        IEnumerator UpdateSliderText()
        {
            const string finishedMessage = "UPDATE CHECKING COMPLETE";
            menuLabelUpdateText.gameObject.SetActive(true);
            WaitForSeconds wait = new WaitForSeconds(0.25f);
            int numberOfDots = 0;
            while (isBusy)
            {
                string dots = new string('.', numberOfDots);
                textProgressBar.text = $"{dots}{message}{dots}";
                menuLabelUpdateText.text = $"{message}{dots}";
                numberOfDots++;
                if (numberOfDots > MaxDots)
                {
                    numberOfDots = 0;
                }
                yield return wait;
            }
            textProgressBar.text = finishedMessage;
            menuLabelUpdateText.text = finishedMessage;

            int updateCount = ModLoader.LoadedMods.Count(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available);
            if (updateCount > 0)
            {
                string updateMessage = updateCount > 1 ? $" THERE ARE {updateCount} MOD UPDATES AVAILABLE!" : $" THERE IS {updateCount} MOD UPDATE AVAILABLE!";
                menuLabelUpdateText.text += $"<color=#87f032>{updateMessage}</color>";
            }

            yield return new WaitForSeconds(5f);

            headerProgressBar.SetActive(false);
            menuLabelUpdateText.gameObject.SetActive(false);
        }

        void ClearSliderText()
        {
            headerProgressBar.SetActive(false);
            menuLabelUpdateText.gameObject.SetActive(false);
        }

        IEnumerator NotifyUpdatesAvailable()
        {
            int updateCount = ModLoader.LoadedMods.Count(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available);
            if (updateCount > 0)
            {
                menuLabelUpdateText.gameObject.SetActive(true);

                string updateMessage = updateCount > 1 ? $" THERE ARE {updateCount} MOD UPDATES AVAILABLE!" : $" THERE IS {updateCount} MOD UPDATE AVAILABLE!";
                menuLabelUpdateText.text = $"<color=#87f032>{updateMessage}</color>";

                yield return new WaitForSeconds(5f);
                if (!IsBusy) menuLabelUpdateText.gameObject.SetActive(false);
            }
        }
        #endregion
        #region Helpers
        void CheckIfNewerVersionAvailable(Mod mod)
        {
            if (IsNewerVersionAvailable(mod.Version, mod.ModUpdateData.LatestVersion))
            {
                mod.ModUpdateData.UpdateStatus = UpdateStatus.Available;
                ModConsole.Log($"<color=green>{mod.ID} has an update available!</color>");
                mod.modListElement.ToggleUpdateButton(true);
            }
            else
            {
                mod.ModUpdateData.UpdateStatus = UpdateStatus.NotAvailable;
                ModConsole.Log($"<color=green>{mod.ID} is up-to-date!</color>");
                mod.modListElement.ToggleUpdateButton(false);
            }

            ModConsole.Log($"Mod Updater: {mod.ID} Latest version: {mod.ModUpdateData.LatestVersion}");
            ModConsole.Log($"Mod Updater: {mod.ID} Your version:   {mod.Version}");
            ModConsole.Log($"Mod Updater: {mod.ID} Link: {mod.ModUpdateData.ZipUrl}");
        }

        bool IsNewerVersionAvailable(string currentVersion, string serverVersion)
        {
            // Messy af, but reliably compares version numbers of the currently installed mod,
            // and the version that is available on the server.

            // The best thing is it won't show an outdated mod info, 
            // if the local mod version is newer than the publicly available one.

            // First we convert string version to individual integers.
            try
            {
                int modMajor, modMinor, modRevision = 0;
                string[] modVersionSpliited = currentVersion.Split('.');
                modMajor = int.Parse(modVersionSpliited[0]);
                modMinor = int.Parse(modVersionSpliited[1]);
                if (modVersionSpliited.Length == 3)
                    modRevision = int.Parse(modVersionSpliited[2]);

                // Same for the newest server version.
                int major, minor, revision = 0;
                string[] verSplitted = serverVersion.Split('.');
                major = int.Parse(verSplitted[0]);
                minor = int.Parse(verSplitted[1]);
                if (verSplitted.Length == 3)
                    revision = int.Parse(verSplitted[2]);

                // And now we finally compare numbers.
                bool isOutdated = false;
                if (major > modMajor)
                {
                    isOutdated = true;
                }
                else
                {
                    if (minor > modMinor && major == modMajor)
                    {
                        isOutdated = true;
                    }
                    else
                    {
                        if (revision > modRevision && minor == modMinor && major == modMajor)
                        {
                            isOutdated = true;
                        }
                    }
                }

                return isOutdated;
            }
            catch
            {
                //ModConsole.LogError($"Mod Updater: Incorrectly formated version tag: {currentVersion} | {serverVersion}");
                //return false;

                // Accurate parsing failed. Try simple comparison instead.
                return currentVersion.ToLower() != serverVersion.ToLower();
            }
        }

        bool ShouldCheckForUpdates()
        {
            if (autoUpdateChecked)
            {
                return false;
            }

            DateTime now = DateTime.Now;
            DateTime lastCheck = ModLoader.modLoaderSettings.lastUpdateCheckDate;

            switch (ModLoader.modLoaderSettings.updateInterval.Value)
            {
                default:
                    return false;
                case 0: // Every launch
                    return true;
                case 1: // Daily
                    return now >= lastCheck.AddDays(1);
                case 2: // Weekly
                    return now >= lastCheck.AddDays(7);
                case 3: // Never
                    return false;
            }
        }

        static string GetUrl(string input)
        {
            string[] separated = input.Split(':');
            return (separated[1] + ":" + separated[2]).Replace("\"", "").Replace("}", "").Replace("]", "");
        }
        #endregion
        #region Sources
        void ReadSources(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath).Where(l => !l.StartsWith("//") && !string.IsNullOrEmpty(l)).ToArray();
            foreach (string l in lines)
            {
                try
                {
                    string modID = l.Split(' ')[0];
                    string link = l.Split(' ')[1];

                    if (!link.Contains("github.com") && !link.Contains("nexusmods.com"))
                    {
                        throw new UriFormatException("Not a NexusMods or GitHub URL.");
                    }

                    Mod mod = ModLoader.GetMod(modID, true);
                    if (mod == null)
                    {
                        continue;
                    }

                    if (backwardCompatibilityMods.Contains(mod))
                    {
                        continue;
                    }

                    // Overwrite only if UpdateLink is null or empty.
                    if (string.IsNullOrEmpty(mod.UpdateLink))
                    {
                        mod.UpdateLink = link;
                        backwardCompatibilityMods.Add(mod);
                        ModConsole.Log(modID + " " + link);
                    }
                }
                catch (Exception ex)
                {
                    ModConsole.LogError($"[Mod Updater] Failed to read line: {l}\n{ex.Message}");
                }
            }
        }
        #endregion
        #region Downloading the updates
        List<Mod> updateDownloadQueue = new List<Mod>();
        int currentModInQueue;

        public void DownloadModUpdate(Mod mod)
        {
            if (!File.Exists(UpdaterPath))
            {
                throw new MissingComponentException("Updater component is missing!");
            }

            if (mod.UpdateLink.Contains("nexusmods.com"))
            {
                if (!NexusSSO.Instance.IsPremium)
                {
                    ModPrompt.CreateYesNoPrompt($"MOD <color=yellow>{mod.Name}</color> USES NEXUSMODS FOR UPDATE DOWNLOADS. " +
                                            $"UNFORTUNATELY, DUE TO NEXUSMODS POLICY, ONLY PREMIUM USERS CAN USE AUTO UPDATE FEATURE.\n\n" +
                                            $"YOUR VERSION IS <color=yellow>{mod.Version}</color> AND THE NEWEST VERSION IS <color=yellow>{mod.ModUpdateData.LatestVersion}</color>.\n\n" +
                                            $"WOULD YOU LIKE TO OPEN MOD PAGE TO DOWNLOAD THE UPDATE MANUALLY?\n\n" +
                                            $"<color=red>WARNING: THIS WILL OPEN YOUR DEFAULT WEB BROWSER.</color>"
                                            , "MOD UPDATER", () => ModHelper.OpenWebsite(mod.UpdateLink));
                    return;
                }
            }

            if (ModLoader.modLoaderSettings.AskBeforeDownload)
            {
                ModPrompt prompt = ModPrompt.CreateCustomPrompt();
                prompt.Text = $"ARE YOU SURE YOU WANT TO DOWNLOAD UPATE FOR MOD:\n\n<color=yellow>\"{mod.Name}\"</color>\n\n" +
                              $"YOUR VERSION IS {mod.Version} AND THE NEWEST VERSION IS {mod.ModUpdateData.LatestVersion}.";
                prompt.Title = "MOD UPDATER";
                prompt.AddButton("YES", () => AddModToDownloadQueue(mod));
                prompt.AddButton("YES, AND DON'T ASK AGAIN", () => { ModLoader.modLoaderSettings.AskBeforeDownload = false; AddModToDownloadQueue(mod); });
                prompt.AddButton("NO", null);
            }
            else
            {
                AddModToDownloadQueue(mod);
            }
        }

        void AddModToDownloadQueue(Mod mod)
        {
            if (!updateDownloadQueue.Contains(mod))
            {
                updateDownloadQueue.Add(mod);
                sliderProgressBar.maxValue = updateDownloadQueue.Count();
            }

            StartDownload();
        }

        void StartDownload()
        {
            if (isBusy)
            {
                return;
            }

            if (currentDownloadRoutine != null)
            {
                return;
            }
            currentDownloadRoutine = DownloadModUpdateRoutine();
            StartCoroutine(currentDownloadRoutine);
        }

        private IEnumerator currentDownloadRoutine;
        IEnumerator DownloadModUpdateRoutine()
        {
            isBusy = true;

            int i = 0;
            sliderProgressBar.value = i;
            headerProgressBar.SetActive(true);
            message = "DOWNLOADING UPDATES";
            StartCoroutine(UpdateSliderText());

            for (; currentModInQueue < updateDownloadQueue.Count(); currentModInQueue++)
            {
                Mod mod = updateDownloadQueue[currentModInQueue];
                ModConsole.Log($"\nMod Updater: Downloading mod update of {mod.ID}...");

                if (!Directory.Exists(DownloadsDirectory))
                {
                    Directory.CreateDirectory(DownloadsDirectory);
                }

                // If a ZipUrl couldn't be obtained, or the link doesn't end with .ZIP file, we open the Mod.UpdateLink website.
                // We are also assuming that mod has been updated by the user.
                if (string.IsNullOrEmpty(mod.ModUpdateData.ZipUrl) || !mod.ModUpdateData.ZipUrl.Contains(".zip"))
                {
                    Process.Start(mod.UpdateLink);
                    mod.ModUpdateData.UpdateStatus = UpdateStatus.Downloaded;
                    continue;
                }

                string fileFormat = mod.ModUpdateData.ZipUrl.Split('.').Last();
                string downloadToPath = Path.Combine(DownloadsDirectory, $"{mod.ID}.{fileFormat}");
                string args = $"get-file \"{mod.ModUpdateData.ZipUrl}\" \"{downloadToPath}\"";
                if (mod.ModUpdateData.ZipUrl.Contains("nexusmods.com"))
                {
                    args += $" \"{NexusSSO.Instance.ApiKey}\"";
                }

                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = UpdaterPath,
                        Arguments = args,
                        WorkingDirectory = UpdaterDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };
                p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                p.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                int downloadTime = 0;
                while (!p.HasExited)
                {
                    downloadTime++;
                    if (downloadTime > TimeoutTimeDownload)
                    {
                        ModConsole.LogError($"Mod Update Check for {mod.ID} timed-out.");
                        p.Kill();
                        break;
                    }

                    yield return new WaitForSeconds(1);
                }

                if (File.Exists(downloadToPath))
                {
                    ModConsole.Log($"Mod Updater: Update downloading for {mod.ID} completed!");
                    mod.ModUpdateData.UpdateStatus = UpdateStatus.Downloaded;
                }
                else
                {
                    ModConsole.Log($"<color=red>Mod Updater: Update downloading for {mod.ID} failed.</color>");
                }
                i++;
                sliderProgressBar.value = i;
            }

            currentDownloadRoutine = null;
            isBusy = false;

            // Asking user if he wants to update now or later.
            int downloadedUpdates = ModLoader.LoadedMods.Where(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Downloaded).Count();
            if (downloadedUpdates > 0)
            {
                ModPrompt.CreateYesNoPrompt($"THERE {(downloadedUpdates > 1 ? "ARE" : "IS")} <color=yellow>{downloadedUpdates}</color> MOD UPDATE{(downloadedUpdates > 1 ? "S" : "")} READY TO BE INSTALLED.\n\n" +
                                        $"WOULD YOU LIKE TO INSTALL THEM NOW?\n\n" +
                                        $"<color=red>WARNING: THIS WILL CLOSE YOUR GAME, AND ALL UNSAVED PROGRESS WILL BE LOST!</color>",
                                        "MOD UPDATER", () => { waitForInstall = true; Application.Quit(); }, null, () => { waitForInstall = true; });
            }
        }
        #endregion
    }

    enum UpdateStatus { NotChecked, NotAvailable, Available, Downloaded }

    /// <summary> Stores the info about mod update found. </summary>
    internal struct ModUpdateData
    {
        public string ZipUrl;
        public string LatestVersion;
        public UpdateStatus UpdateStatus;

        // Nexus
        public int ModID;
        public string Summary; 
        public string PictureUrl;
    }

    internal class ModUpdaterDatabase
    {
        // Because of some weird conflict between Newtonsoft.Json.Linq and System.Linq conflict,
        // we are forced to use a custom database solution.

        string DatabaseFile = Path.Combine(ModUpdater.UpdaterDirectory, "Updater.txt");

        Dictionary<string, ModUpdateData> modUpdateData;

        public ModUpdaterDatabase()
        {
            if (!File.Exists(DatabaseFile))
            {
                File.Create(DatabaseFile);
            }

            modUpdateData = new Dictionary<string, ModUpdateData>();

            string[] fileContent = File.ReadAllLines(DatabaseFile);
            foreach (var s in fileContent)
            {
                if (string.IsNullOrEmpty(s))
                {
                    continue;
                }

                try
                {
                    string id, url, latest = "";
                    string[] spliitted = s.Split(',');
                    id = spliitted[0];
                    url = spliitted[1];
                    latest = spliitted[2];

                    ModUpdateData data = new ModUpdateData
                    {
                        ZipUrl = url,
                        LatestVersion = latest
                    };

                    modUpdateData.Add(id, data);
                }
                catch
                {
                    continue;
                }
            }
        }

        internal void Save()
        {
            IEnumerable<Mod> mods = ModLoader.LoadedMods.Where(m => m.ModUpdateData.UpdateStatus == UpdateStatus.Available);
            string output = "";
            foreach (Mod mod in mods)
            {
                string updateLink = string.IsNullOrEmpty(mod.ModUpdateData.ZipUrl) ? mod.UpdateLink : mod.ModUpdateData.ZipUrl;
                output += $"{mod.ID},{updateLink},{mod.ModUpdateData.LatestVersion}\n";
            }

            if (File.Exists(DatabaseFile))
                File.Delete(DatabaseFile);

            File.WriteAllText(DatabaseFile, output);
        }

        internal ModUpdateData Get(Mod mod)
        {
            return modUpdateData.FirstOrDefault(m => m.Key == mod.ID).Value;
        }

        internal Dictionary<string, ModUpdateData> GetAll()
        {
            return modUpdateData;
        }
    }
}