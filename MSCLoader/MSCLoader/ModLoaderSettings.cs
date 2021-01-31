using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MSCLoader
{
    public class ToggleSettingMenu : MonoBehaviour
    {
        public GameObject settingMenu;
    }

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

        public void SetupKeyAction()
        {
            openConsoleKey.bindPostfix = SaveSettings;
        }

        public bool disableSave = false;
        public void SaveSettings()
        {
            StopAllCoroutines();
            if (disableSave) return;
            StartCoroutine(SaveToINI());
        }

        WaitForSeconds saveWait = new WaitForSeconds(0.1f);
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

            foreach (ModListElement otherMod in modContainer.modListDictionary.Values)
                otherMod.ToggleSettingsOff();

            gameObject.SetActive(modLoaderSettingsToggle.isOn);
        }
    }

    public class StartDisable : MonoBehaviour
    {
        public GameObject modSettings;

        void Awake()
        {
            modSettings.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
