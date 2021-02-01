using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Diagnostics;
using UnityEngine;

namespace MSCLoader
{
    // Copyright (C) Konrad Figura 2021
    // This file is a part of MSCLoader Pro.
    // You cannot use it any other project.
    public class ModUpdater : MonoBehaviour
    {
        bool isBusy;
        public bool IsBusy => isBusy;

        readonly string UpdaterPath = $"{Application.dataPath}/Managed/CoolUpdater.exe";
        
        int downloadTime;
        const int TimeoutTime = 30; // in seconds.

        /// <summary>
        /// Starts looking for the update of the specific mod.
        /// </summary>
        public void LookForUpdates()
        {
            if (IsBusy)
            {
                ModUI.CreatePrompt("Mod loader is busy looking for updates.", "Mod Loader Update");
                return;
            }

            StartCoroutine(CheckForModUpdates());
        }

        /// <summary>
        /// Goes through all mods and checks if an update on GitHub or Nexus is available for them.
        /// </summary>
        IEnumerator CheckForModUpdates()
        {
            isBusy = true;

            foreach (Mod mod in ModLoader.LoadedMods)
            {
                if (string.IsNullOrEmpty(mod.UpdateLink)) continue;

                string url = mod.UpdateLink;
                // Formatting the link.
                if (url.Contains("github.com"))
                {
                    // If is not direct api.github.com link, modify it so it matches it correctly.
                    if (!url.Contains("api."))
                    {
                        url = url.Replace("https://", "").Replace("www.", "").Replace("github.com/", "");
                        url = "https://api.github.com/repos" + url;
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

                Process p = new Process();
                p.StartInfo.FileName = UpdaterPath;
                p.StartInfo.Arguments = "get-metafile " + url;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                string output = "";
                downloadTime = 0;
                while (!p.HasExited)
                {
                    yield return null;

                    downloadTime++;
                    if (downloadTime > TimeoutTime)
                    {
                        ModConsole.Error($"Mod Update Check for {mod.Name} timed-out.");
                        continue;
                    }

                    output = p.StandardOutput.ReadToEnd();
                }
                p.Close();

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
                            mod.ModUpdateData.ZipUrl = s.Split(':')[1].Replace("\"", "").Replace("}", "").Replace("]", "");
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
                    // TODO: There is newer version of mod available. Indicate that in mod settings and enable download button for it.
                }
            }

            isBusy = false;
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

        public void DownloadModUpdate(Mod mod)
        {
            StartCoroutine(DownloadModUpdateRoutine(mod));
        }

        IEnumerator DownloadModUpdateRoutine(Mod mod)
        {
            isBusy = true;

            // TODO
            yield return null;

            isBusy = false;
        }
    }

    /// <summary>
    /// Stores the info about mod update found.
    /// </summary>
    internal struct ModUpdateData
    {
        public string ZipUrl;
        public string LatestVersion;
    }
}
