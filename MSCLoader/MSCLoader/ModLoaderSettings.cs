using MSCLoader.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591
namespace MSCLoader
{
    public class ModLoaderSettings : MonoBehaviour
    {
        [SerializeField] internal ModContainer modContainer;
        [SerializeField] internal Toggle modLoaderSettingsToggle;

        [SerializeField] internal Text version;
        [SerializeField] internal Text menuLabelText;

        [SerializeField] internal SettingToggle skipGameLauncher, skipSplashScreen, useVsyncInMenu;

        [SerializeField] internal SettingRadioButtons updateMode;
        [SerializeField] internal SettingText lastUpdateCheck;
        [SerializeField] internal SettingRadioButtons updateInterval;
        [SerializeField] internal SettingToggle askBeforeDownload;

        [SerializeField] internal SettingKeybind openConsoleKey;
        [SerializeField] internal SettingSlider consoleFontSize;
        [SerializeField] internal SettingRadioButtons consoleAutoOpen;
        [SerializeField] internal SettingSlider consoleWindowHeight, consoleWindowWidth;

        public string Version { get => ModLoader.Version; internal set
            {
                version.text = $"VERSION: {value}";
                menuLabelText.text = $"VERSION: {value}";
            }
        }
        public bool SkipGameLauncher { get => skipGameLauncher.Value; internal set => skipGameLauncher.Value = value; }
        public bool SkipSplashScreen { get => skipSplashScreen.Value; internal set => skipSplashScreen.Value = value; }
        public bool UseVsyncInMenu { get => useVsyncInMenu.Value; internal set => useVsyncInMenu.Value = value; }

        public int UpdateMode { get => updateMode.Value; internal set => updateMode.Value = value; }
        public string LastUpdateCheck { get => lastUpdateCheckDate.ToString("u"); internal set => lastUpdateCheck.Text = $"LAST CHECKED FOR UPDATES: <color=yellow>{value}</color>"; }
        public int UpdateInterval { get => updateInterval.Value; internal set => updateInterval.Value = value; }
        public bool AskBeforeDownload { get => askBeforeDownload.Value; internal set => askBeforeDownload.Value = value; }

        public KeyCode OpenConsoleKeyKeybind { get => openConsoleKey.keybind; internal set => openConsoleKey.keybind = value; }
        public KeyCode[] OpenConsoleKeyModifiers { get => openConsoleKey.modifiers; internal set => openConsoleKey.modifiers = value; }
        public float ConsoleFontSize { get => consoleFontSize.Value; internal set => consoleFontSize.Value = value; }
        public int ConsoleAutoOpen { get => consoleAutoOpen.Value; internal set => consoleAutoOpen.Value = value; }
        public float ConsoleWindowHeight { get => consoleWindowHeight.Value; internal set => consoleWindowHeight.Value = value; }
        public float ConsoleWindowWidth { get => consoleWindowWidth.Value; internal set => consoleWindowWidth.Value = value; }

        [SerializeField] internal DateTime lastUpdateCheckDate;

        [SerializeField] internal bool disableSave = false;
        public void SaveSettings()
        {
            StopAllCoroutines();
            if (disableSave) return;
            StartCoroutine(SaveToINI());
        }

        readonly WaitForSeconds saveWait = new WaitForSeconds(0.1f);
        IEnumerator SaveToINI()
        {
            yield return saveWait;
            SaveINISettings();
        }

        public void SaveINISettings()
        {
            MSCLoader.settings.SaveSettings(this);
        }

        bool suspendAction = false;
        public void ToggleMenuOff()
        {
            suspendAction = true;
            modLoaderSettingsToggle.isOn = false;
            gameObject.SetActive(false);
            suspendAction = false;
        }

        public void ToggleMenu()
        {
            if (suspendAction) return;

            foreach (ModListElement otherMod in modContainer.modListDictionary.Values) otherMod.ToggleSettingsOff();
            gameObject.SetActive(modLoaderSettingsToggle.isOn);
        }

        public void OpenModLoaderSite()
        {
            ModPrompt.CreateYesNoPrompt(
                "THIS WILL OPEN A WEBSITE IN YOUR DEFAULT WEB BROWSER AND MINIMIZE THE GAME.", 
                "OPEN MOD LOADER WEBSITE?", 
                () => ModHelper.OpenWebsite(ModLoader.modLoaderURL));
        }
        public void OpenModsFolder()
        {
            ModPrompt.CreateYesNoPrompt(
                "THIS WILL OPEN THE MODS FOLDER IN THE EXPLORER AND MINIMIZE THE GAME.", 
                "OPEN MODS FOLDER?", 
                () => ModHelper.OpenFolder(ModLoader.ModsFolder));
        }
        public void OpenGameFolder()
        {
            ModPrompt.CreateYesNoPrompt(
                "THIS WILL OPEN THE GAME FOLDER IN THE EXPLORER AND MINIMIZE THE GAME.", 
                "OPEN GAME FOLDER?", 
                () => ModHelper.OpenFolder($@"{Path.GetFullPath(".")}"));
        }
        public void OpenSaveFolder()
        {
            ModPrompt.CreateYesNoPrompt(
                "THIS WILL OPEN THE SAVE FOLDER IN THE EXPLORER AND MINIMIZE THE GAME.", 
                "OPEN SAVE FOLDER?", 
                () => ModHelper.OpenFolder(Application.persistentDataPath));
        }
        public void OpenOutputLog()
        {
            ModPrompt.CreateYesNoPrompt(
                "THIS WILL OPEN THE OUTPUT LOG IN YOUR DEFAULT TEXT EDITOR AND MINIMIZE THE GAME.", 
                "OPEN OUTPUT LOG?", 
                () => ModHelper.OpenFolder($@"{Path.GetFullPath(".")}\output_log.txt"));
        }

        public void RefreshUpdateCheckTime()
        {
            lastUpdateCheckDate = DateTime.Now;
            LastUpdateCheck = $"{lastUpdateCheckDate:u}".TrimEnd('Z');
            SaveINISettings();
        }

        public void ParseUpdateCheckTime(string date)
        {
            try { lastUpdateCheckDate = DateTime.Parse(date); } catch { lastUpdateCheckDate = DateTime.Now; }
            LastUpdateCheck = $"{lastUpdateCheckDate:u}".TrimEnd('Z');
        }
    }
    
    internal class LoaderSettings
    {
        readonly ModINI settingINI;

        public bool SkipGameLauncher = false;
        public bool SkipSplashScreen = false;
        public bool UseVsyncInMenu = true;

        public int UpdateMode = 2;
        public string LastUpdateCheck = "2000-01-01 00:00:00Z";
        public int UpdateInterval = 1;
        public bool AskBeforeDownload = true;

        public KeyCode[] OpenConsoleKey = { KeyCode.BackQuote };
        public int ConsoleFontSize = 12;
        public int ConsoleAutoOpen = 3;
        public int ConsoleWindowHeight = 175;
        public int ConsoleWindowWidth = 250;

        public bool EnableModLoader = true;
        public bool UseOutputLog = true;
        public string ModsFolderPath = "Mods";

        public LoaderSettings()
        {
            settingINI = new ModINI("ModLoaderSettings");

            if (!File.Exists("ModLoaderSettings.ini")) WriteValues();

            SkipGameLauncher = settingINI.Read("SkipGameLauncher", "General", SkipGameLauncher);
            SkipSplashScreen = settingINI.Read("SkipSplashScreen", "General", SkipSplashScreen);
            UseVsyncInMenu = settingINI.Read("UseVsyncInMenu", "General", UseVsyncInMenu);

            UpdateMode = settingINI.Read("UpdateMode", "Updates", UpdateMode);
            LastUpdateCheck = settingINI.Read("LastUpdateCheck", "Updates", LastUpdateCheck);
            UpdateInterval = settingINI.Read("UpdateInterval", "Updates", UpdateInterval);
            AskBeforeDownload = settingINI.Read("AskBeforeDownload", "Updates", AskBeforeDownload);

            OpenConsoleKey = settingINI.Read("OpenConsoleKey", "Console", OpenConsoleKey[0].ToString()).Split(';').Select(x => (KeyCode)Enum.Parse(typeof(KeyCode), x, true)).ToArray();
            ConsoleFontSize = settingINI.Read("ConsoleFontSize", "Console", ConsoleFontSize);
            ConsoleAutoOpen = settingINI.Read("ConsoleAutoOpen", "Console", ConsoleAutoOpen);
            ConsoleWindowHeight = settingINI.Read("ConsoleWindowHeight", "Console", ConsoleWindowHeight);
            ConsoleWindowWidth = settingINI.Read("ConsoleWindowWidth", "Console", ConsoleWindowWidth);

            EnableModLoader = settingINI.Read("EnableModLoader", "Hidden", EnableModLoader);
            UseOutputLog = settingINI.Read("UseOutputLog", "Hidden", UseOutputLog);
            ModsFolderPath = settingINI.Read("ModsFolderPath", "Hidden", ModsFolderPath);
        }

        public void SaveSettings(ModLoaderSettings modLoaderSettings)
        {
            SkipGameLauncher = modLoaderSettings.SkipGameLauncher;
            SkipSplashScreen = modLoaderSettings.SkipSplashScreen;
            UseVsyncInMenu = modLoaderSettings.UseVsyncInMenu;

            UpdateMode = modLoaderSettings.UpdateMode;
            LastUpdateCheck = modLoaderSettings.LastUpdateCheck;
            UpdateInterval = modLoaderSettings.UpdateInterval;
            AskBeforeDownload = modLoaderSettings.AskBeforeDownload;

            List<KeyCode> keycodeList = new List<KeyCode>() { modLoaderSettings.OpenConsoleKeyKeybind };
            keycodeList.AddRange(modLoaderSettings.OpenConsoleKeyModifiers);
            OpenConsoleKey = keycodeList.ToArray();
            ConsoleFontSize = (int)modLoaderSettings.ConsoleFontSize;
            ConsoleAutoOpen = modLoaderSettings.ConsoleAutoOpen;
            ConsoleWindowHeight = (int)modLoaderSettings.ConsoleWindowHeight;
            ConsoleWindowWidth = (int)modLoaderSettings.ConsoleWindowWidth;

            WriteValues();
        }
        void WriteValues()
        {
            settingINI.Write("SkipGameLauncher", "General", SkipGameLauncher);
            settingINI.Write("SkipSplashScreen", "General", SkipSplashScreen);
            settingINI.Write("UseVsyncInMenu", "General", UseVsyncInMenu);

            settingINI.Write("UpdateMode", "Updates", UpdateMode);
            settingINI.Write("LastUpdateCheck", "Updates", LastUpdateCheck);
            settingINI.Write("UpdateInterval", "Updates", UpdateInterval);
            settingINI.Write("AskBeforeDownload", "Updates", AskBeforeDownload);

            settingINI.Write("OpenConsoleKey", "Console", string.Join(";", Array.ConvertAll(OpenConsoleKey, x => x.ToString())));
            settingINI.Write("ConsoleFontSize", "Console", ConsoleFontSize);
            settingINI.Write("ConsoleAutoOpen", "Console", ConsoleAutoOpen);
            settingINI.Write("ConsoleWindowHeight", "Console", ConsoleWindowHeight);
            settingINI.Write("ConsoleWindowWidth", "Console", ConsoleWindowWidth);

            settingINI.Write("EnableModLoader", "Hidden", EnableModLoader);
            settingINI.Write("UseOutputLog", "Hidden", UseOutputLog);
            settingINI.Write("UseOutputLog", "Hidden", ModsFolderPath);
        }

        public void ApplySettings(ModLoaderSettings modLoaderSettings)
        {
            // Disable saving to the INI while the settings are loaded.
            modLoaderSettings.disableSave = true;

            // Apply the various settings.
            modLoaderSettings.Version = ModLoader.Version;
            modLoaderSettings.SkipGameLauncher = SkipGameLauncher;
            modLoaderSettings.SkipSplashScreen = SkipSplashScreen;

            modLoaderSettings.UseVsyncInMenu = UseVsyncInMenu;
            modLoaderSettings.useVsyncInMenu.OnValueChanged.AddListener((value) =>
            {
                if (!ModLoader.modLoaderInstance.vSyncEnabled && ModLoader.CurrentScene == CurrentScene.MainMenu)
                    QualitySettings.vSyncCount = modLoaderSettings.UseVsyncInMenu ? 1 : 0;
            });

            modLoaderSettings.UpdateMode = UpdateMode;
            modLoaderSettings.ParseUpdateCheckTime(LastUpdateCheck);
            modLoaderSettings.UpdateInterval = UpdateInterval;
            modLoaderSettings.AskBeforeDownload = AskBeforeDownload;

            modLoaderSettings.OpenConsoleKeyKeybind = OpenConsoleKey[0];
            modLoaderSettings.OpenConsoleKeyModifiers = OpenConsoleKey.Skip(1).ToArray();
            modLoaderSettings.openConsoleKey.PostBind.AddListener(modLoaderSettings.SaveSettings);

            modLoaderSettings.ConsoleFontSize = ConsoleFontSize;
            modLoaderSettings.ConsoleAutoOpen = ConsoleAutoOpen;
            modLoaderSettings.ConsoleWindowHeight = ConsoleWindowHeight;
            modLoaderSettings.ConsoleWindowWidth = ConsoleWindowWidth;

            // Enable saving again if any of the values are changed.
            modLoaderSettings.disableSave = false;
        }
    }
}
