using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#pragma warning disable CS1591
namespace MSCLoader
{
    // Copyright (C) Konrad Figura 2021
    // This file is a part of MSCLoader Pro.
    // You cannot use it any other project.
    public class ModUpdater : MonoBehaviour
    {
        public GameObject headerProgressBar;
        public Slider sliderProgressBar;
        public Text textProgressBar;

        bool isBusy;
        public bool IsBusy => isBusy;

        string UpdaterDirectory => Path.Combine(Directory.GetCurrentDirectory(), "ModUpdater");
        string UpdaterPath => Path.Combine(UpdaterDirectory, "CoolUpdater.exe");
        string DownloadsDirectory => Path.Combine(UpdaterDirectory, "Downloads");

        int downloadTime;
        const int TimeoutTime = 10; // in seconds.
        const int TimeoutTimeDownload = 20; // in seconds.

        #region Looking for updates
        /// <summary> Starts looking for the update of the specific mod. </summary>
        public void LookForUpdates()
        {
            if (IsBusy)
            {
                ModUI.CreatePrompt("Mod loader is busy looking for updates.", "Mod Updater");
                return;
            }

            if (!File.Exists(UpdaterPath))
            {
                throw new MissingComponentException("Updater component does not exist!");
            }

            StartCoroutine(CheckForModUpdates(ModLoader.LoadedMods.Where(x => !string.IsNullOrEmpty(x.UpdateLink))));
        }

        /// <summary> Goes through all mods and checks if an update on GitHub or Nexus is available for them. </summary>
        IEnumerator CheckForModUpdates(IEnumerable<Mod> mods)
        {
            if (mods.Count() == 0) yield break;

            isBusy = true;

            // Enable the progress bar.
            int i = 0;
            sliderProgressBar.value = i;
            headerProgressBar.SetActive(true);
            sliderProgressBar.maxValue = mods.Count();
            StartCoroutine(UpdateSliderText());

            foreach (Mod mod in mods)
            {
                ModConsole.Log($"\nLooking for update of {mod.Name}");
                string url = mod.UpdateLink;
                // Formatting the link.
                if (url.Contains("github.com"))
                {
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
                }
                else if (url.Contains("nexusmods.com"))
                {
                    // TODO
                }
                ModConsole.Log($"URL: {url}");
                
                ModConsole.Log($"Starting checking for update process...");
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = UpdaterPath,
                        Arguments = "get-metafile " + url,
                        WorkingDirectory = UpdaterDirectory,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                lastDataOut = "";
                p.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                p.ErrorDataReceived += new DataReceivedEventHandler(ErrorHandler);

                string output = "";
                downloadTime = 0;
                
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                while (!p.HasExited)   
                {
                    downloadTime++;
                    if (downloadTime > TimeoutTime)
                    {
                        ModConsole.LogError($"Mod Updater: Getting metadata of {mod.ID} timed-out.");
                        break;
                    }

                    yield return new WaitForSeconds(1);
                }

                p.Close();
                ModConsole.Log($"Mod Updater: {mod.ID} - pulling metadata succeeded!");

                output = lastDataOut;

                // Reading the metadata file info that we want.
                mod.ModUpdateData = new ModUpdateData();
                if (url.Contains("github.com"))
                {
                    string[] outputArray = output.Split(',');
                    foreach (string s in outputArray)
                    {
                        // Finding tag of the latest release, this servers as latest version number.
                        if (s.Contains("\"tag_name\""))
                        {
                            mod.ModUpdateData.LatestVersion = s.Split(':')[1].Replace("\"", "");
                        }
                        else if (s.Contains("\"browser_download_url\""))
                        {
                            string[] separated = s.Split(':');
                            mod.ModUpdateData.ZipUrl = (separated[1] + separated[2]).Replace("\"", "").Replace("}", "").Replace("]", "");
                        }

                        // Breaking out of the loop, if we found all that we've been looking for.
                        if (!string.IsNullOrEmpty(mod.ModUpdateData.ZipUrl) && !string.IsNullOrEmpty(mod.ModUpdateData.LatestVersion))
                        {
                            break;
                        }
                    }
                }

                if (IsNewerVersionAvailable(mod.Version, mod.ModUpdateData.LatestVersion))
                {
                    mod.ModUpdateData.UpdateStatus = UpdateStatus.Available;
                    ModConsole.Log($"<color=green>Mod Updater: {mod.ID} has an update available!</color>");
                }
                else
                {
                    mod.ModUpdateData.UpdateStatus = UpdateStatus.NotAvailable;
                    ModConsole.Log($"<color=green>Mod Updater: {mod.ID} is up-to-date!</color>");
                }

                ModConsole.Log($"Mod Updater: {mod.ID} Available version: {mod.ModUpdateData.LatestVersion} ");
                ModConsole.Log($"Mod Updater: {mod.ID} Your version:      {mod.Version} ");

                i++;
                sliderProgressBar.value = i;
            }

            isBusy = false;
        }

        private void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            ModConsole.Log(e.Data);
        }

        static string lastDataOut;
        static void OutputHandler(object sendingProcess, DataReceivedEventArgs e)
        {
            lastDataOut += e.Data + "\n";
        }

        IEnumerator UpdateSliderText()
        {
            WaitForSeconds wait = new WaitForSeconds(0.25f);
            while (isBusy)
            {
                textProgressBar.text = "CHECKING FOR UPDATES";
                yield return wait;
                textProgressBar.text = ".CHECKING FOR UPDATES.";
                yield return wait;
                textProgressBar.text = "..CHECKING FOR UPDATES..";
                yield return wait;
                textProgressBar.text = "...CHECKING FOR UPDATES...";
                yield return wait;
            }
            yield return new WaitForSeconds(5f);
            headerProgressBar.SetActive(false);
        }

        bool IsNewerVersionAvailable(string currentVersion, string serverVersion)
        {
            // Messy af, but reliably compares version numbers of the currently installed mod,
            // and the version that is available on the server.

            // The best thing is it won't show an outdated mod info, 
            // if the local mod version is newer than the publicly available one.

            // First we convert string version to individual integers.
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
        #endregion
        #region Downloading the updates
        List<Mod> updateCheckQueue = new List<Mod>();
        int currentModInQueue;

        public void DownloadModUpdate(Mod mod)
        {
            if (!File.Exists(UpdaterPath))
            {
                throw new MissingComponentException("Updater component does not exist!");
            }

            if (!updateCheckQueue.Contains(mod))
            {
                updateCheckQueue.Add(mod);
                sliderProgressBar.maxValue = updateCheckQueue.Count();
            }

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
            StartCoroutine(UpdateSliderText());

            for (; currentModInQueue < updateCheckQueue.Count() - 1; currentModInQueue++)
            {
                Mod mod = updateCheckQueue[currentModInQueue];
                ModConsole.Log($"\nMod Updater: Downloading mod update of {mod.ID}...");

                if (!Directory.Exists(DownloadsDirectory))
                {
                    Directory.CreateDirectory(DownloadsDirectory);
                }

                string downloadToPath = Path.Combine(DownloadsDirectory, $"{mod.ID}.zip");
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = UpdaterPath,
                        Arguments = $"get-file {mod.ModUpdateData.ZipUrl} {downloadToPath}",
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
                downloadTime = 0;
                while (!p.HasExited)
                {
                    downloadTime++;
                    if (downloadTime > TimeoutTimeDownload)
                    {
                        ModConsole.LogError($"Mod Update Check for {mod.ID} timed-out.");
                        break;
                    }

                    yield return new WaitForSeconds(1);
                }

                if (File.Exists(downloadToPath))
                {
                    ModConsole.Log($"Mod Updater: Update downloading for {mod.ID} completed!");
                }
                else
                {
                    ModConsole.Log($"<color=red>Mod Updater: Update downloading for {mod.ID} failed.</color");
                }
                i++;
                sliderProgressBar.value = i;
            }

            currentDownloadRoutine = null;
            isBusy = false;
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
    }
}