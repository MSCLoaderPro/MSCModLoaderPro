using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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
        public string Version { get => version.text.Remove(0, 9); set { version.text = $"VERSION: {value}"; } }

        public SettingToggle skipGameLauncher;
        public bool SkipGameLauncher { get => skipGameLauncher.Value; set { skipGameLauncher.Value = value; } }

        public SettingToggle skipSplashScreen;
        public bool SkipSplashScreen { get => skipSplashScreen.Value; set { skipSplashScreen.Value = value; } }

        public SettingToggle useVsyncInMenu;
        public bool UseVsyncInMenu { get => useVsyncInMenu.Value; set { useVsyncInMenu.Value = value; } }

        public SettingToggle checkUpdatesAutomatically;
        public bool CheckUpdatesAutomatically { get => checkUpdatesAutomatically.Value; set { checkUpdatesAutomatically.Value = value; } }

        public SettingKeybind openConsoleKey;
        public KeyCode OpenConsoleKeyKeybind { get => openConsoleKey.keybind; set { openConsoleKey.keybind = value; } }
        public KeyCode[] OpenConsoleKeyModifiers { get => openConsoleKey.modifiers; set { openConsoleKey.modifiers = value; } }

        public SettingSlider consoleFontSize;
        public float ConsoleFontSize { get => consoleFontSize.Value; set { consoleFontSize.Value = value; } }

        public SettingRadioButtons consoleAutoOpen;
        public int ConsoleAutoOpen { get => consoleAutoOpen.Value; set { consoleAutoOpen.Value = value; } }

        public SettingSlider consoleWindowHeight;
        public float ConsoleWindowHeight
        {
            get => consoleWindowHeight.Value; set
            {
                consoleWindowHeight.MaxValue = Screen.height;
                consoleWindowHeight.Value = value;
            }
        }

        public SettingSlider consoleWindowWidth;
        public float ConsoleWindowWidth
        {
            get => consoleWindowWidth.Value; set
            {
                consoleWindowWidth.MaxValue = Screen.width;
                consoleWindowWidth.Value = value;
            }
        }

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
        public GameObject modSettings;//, prefabSettingTypes;

        void Awake()
        {
            modSettings.SetActive(false);
            //prefabSettingTypes.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
