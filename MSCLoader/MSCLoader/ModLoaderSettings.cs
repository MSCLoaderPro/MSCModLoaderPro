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
        public SettingToggle skipGameLauncher, skipSplashScreen, useVsyncInMenu, checkUpdatesAutomatically;
        public SettingKeybind openConsoleKey;
        public SettingSlider consoleFontSize;
        public SettingRadioButtons consoleAutoOpen;
        public SettingSlider consoleWindowHeight, consoleWindowWidth;

        public string Version { get => version.text.Remove(0, 9); set => version.text = $"VERSION: {value}"; }
        public bool SkipGameLauncher { get => skipGameLauncher.Value; set => skipGameLauncher.Value = value; }
        public bool SkipSplashScreen { get => skipSplashScreen.Value; set => skipSplashScreen.Value = value; }
        public bool UseVsyncInMenu { get => useVsyncInMenu.Value; set => useVsyncInMenu.Value = value; }
        public bool CheckUpdatesAutomatically { get => checkUpdatesAutomatically.Value; set => checkUpdatesAutomatically.Value = value; }
        public KeyCode OpenConsoleKeyKeybind { get => openConsoleKey.keybind; set => openConsoleKey.keybind = value; }
        public KeyCode[] OpenConsoleKeyModifiers { get => openConsoleKey.modifiers; set => openConsoleKey.modifiers = value; }
        public float ConsoleFontSize { get => consoleFontSize.Value; set => consoleFontSize.Value = value; }
        public int ConsoleAutoOpen { get => consoleAutoOpen.Value; set => consoleAutoOpen.Value = value; }
        public float ConsoleWindowHeight { get => consoleWindowHeight.Value; set => consoleWindowHeight.Value = value; }
        public float ConsoleWindowWidth { get => consoleWindowWidth.Value; set => consoleWindowWidth.Value = value; }

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
            MSCLoader.settings.SkipGameLauncher = SkipGameLauncher;
            MSCLoader.settings.SkipSplashScreen = SkipSplashScreen;
            MSCLoader.settings.UseVsyncInMenu = UseVsyncInMenu;
            MSCLoader.settings.CheckUpdateAutomatically = CheckUpdatesAutomatically;

            List<KeyCode> keycodeList = new List<KeyCode>() { OpenConsoleKeyKeybind };
            keycodeList.AddRange(OpenConsoleKeyModifiers);
            MSCLoader.settings.OpenConsoleKey = keycodeList.ToArray();
            MSCLoader.settings.ConsoleFontSize = (int)ConsoleFontSize;
            MSCLoader.settings.ConsoleAutoOpen = ConsoleAutoOpen;
            MSCLoader.settings.ConsoleWindowHeight = (int)ConsoleWindowHeight;
            MSCLoader.settings.ConsoleWindowWidth = (int)ConsoleWindowWidth;

            MSCLoader.settings.SaveSettings();
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
