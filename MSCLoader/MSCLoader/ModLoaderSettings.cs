using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS1591
namespace MSCLoader
{
    public class ModLoaderSettings : MonoBehaviour
    {
        public ModContainer modContainer;
        public Toggle modLoaderSettingsToggle;

        public Text version;
        public Text menuLabelText;

        public SettingToggle skipGameLauncher, skipSplashScreen, useVsyncInMenu;

        public SettingRadioButtons updateMode;
        public SettingText lastUpdateCheck;
        public SettingRadioButtons updateInterval;

        public SettingKeybind openConsoleKey;
        public SettingSlider consoleFontSize;
        public SettingRadioButtons consoleAutoOpen;
        public SettingSlider consoleWindowHeight, consoleWindowWidth;

        public string Version { get => version.text.Remove(0, 9); set
            {
                version.text = $"VERSION: {value}";
                menuLabelText.text = $"MOD LOADER PRO v{value}";
            }
        }
        public bool SkipGameLauncher { get => skipGameLauncher.Value; set => skipGameLauncher.Value = value; }
        public bool SkipSplashScreen { get => skipSplashScreen.Value; set => skipSplashScreen.Value = value; }
        public bool UseVsyncInMenu { get => useVsyncInMenu.Value; set => useVsyncInMenu.Value = value; }

        public int UpdateMode { get => updateMode.Value; set => updateMode.Value = value; }
        public string LastUpdateCheck { get => lastUpdateCheckDate.ToString("u"); set => lastUpdateCheck.Text = $"LAST CHECKED FOR UPDATES: <color=yellow>{value}</color>"; }
        public int UpdateInterval { get => updateInterval.Value; set => updateInterval.Value = value; }

        public KeyCode OpenConsoleKeyKeybind { get => openConsoleKey.keybind; set => openConsoleKey.keybind = value; }
        public KeyCode[] OpenConsoleKeyModifiers { get => openConsoleKey.modifiers; set => openConsoleKey.modifiers = value; }
        public float ConsoleFontSize { get => consoleFontSize.Value; set => consoleFontSize.Value = value; }
        public int ConsoleAutoOpen { get => consoleAutoOpen.Value; set => consoleAutoOpen.Value = value; }
        public float ConsoleWindowHeight { get => consoleWindowHeight.Value; set => consoleWindowHeight.Value = value; }
        public float ConsoleWindowWidth { get => consoleWindowWidth.Value; set => consoleWindowWidth.Value = value; }

        public DateTime lastUpdateCheckDate;


        public bool disableSave = false;
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
            ModUI.CreateYesNoPrompt("THIS WILL OPEN A WEBSITE IN YOUR DEFAULT WEB BROWSER AND MINIMIZE THE GAME.", "OPEN MOD LOADER WEBSITE?", () => ModHelper.OpenWebsite(ModLoader.modLoaderURL));
        }
        public void OpenModsFolder()
        {
            ModUI.CreateYesNoPrompt("THIS WILL THE MODS FOLDER IN WINDOWS EXPLORER AND MINIMIZE THE GAME.", "OPEN MODS FOLDER?", () => ModHelper.OpenFolder($@"{Path.GetFullPath(".")}\Mods"));
        }
        public void OpenGameFolder()
        {
            ModUI.CreateYesNoPrompt("THIS WILL THE GAME FOLDER IN WINDOWS EXPLORER AND MINIMIZE THE GAME.", "OPEN GAME FOLDER?", () => ModHelper.OpenFolder($@"{Path.GetFullPath(".")}"));
        }
        public void OpenSaveFolder()
        {
            ModUI.CreateYesNoPrompt("THIS WILL THE SAVE FOLDER IN WINDOWS EXPLORER AND MINIMIZE THE GAME.", "OPEN SAVE FOLDER?", () => ModHelper.OpenFolder(Application.persistentDataPath));
        }
        public void OpenOutputLog()
        {
            ModUI.CreateYesNoPrompt("THIS WILL THE OUTPUT LOG IN YOUR DEFAULT TEXT EDITOR AND MINIMIZE THE GAME.", "OPEN OUTPUT LOG?", () => ModHelper.OpenFolder($@"{Path.GetFullPath(".")}\output_log.txt"));
        }

        public void RefreshUpdateCheckTime()
        {
            lastUpdateCheckDate = DateTime.Now;
            LastUpdateCheck = $"{lastUpdateCheckDate:u}".TrimEnd('Z');
        }

        public void ParseUpdateCheckTime(string date)
        {
            try { lastUpdateCheckDate = DateTime.Parse(date); } catch { lastUpdateCheckDate = DateTime.Now; }
            LastUpdateCheck = $"{lastUpdateCheckDate:u}".TrimEnd('Z');
        }
    }
    
    public class StartDisable : MonoBehaviour
    {
        public GameObject[] objectToDisable;

        void Awake()
        {
            for (int i = 0; i < objectToDisable.Length; i++)
                objectToDisable[i].SetActive(false);

            gameObject.SetActive(false);
        }
    }
}
