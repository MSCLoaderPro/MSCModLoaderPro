using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MSCLoader
{
    public class ModContainer : MonoBehaviour
    {
        public Dictionary<Mod, ModSettings> settingsDictionary = new Dictionary<Mod, ModSettings>();
        public Dictionary<Mod, ModListElement> modListDictionary = new Dictionary<Mod, ModListElement>();

        public ModLoaderSettings modLoaderSettings;

        public Transform modList;
        public GameObject modListElementPrefab;

        public Transform settingsList;
        public GameObject settingsWindowPrefab;

        public ModListElement CreateModListElement(Mod mod)
        {
            ModListElement modListElement = Instantiate(modListElementPrefab).GetComponent<ModListElement>();
            modListElement.modContainer = this;
            modListElement.Name = mod.Name;
            modListElement.Author = mod.Author;
            modListElement.Version = mod.Version;

            if (mod.Icon != null)
            {
                Texture2D iconTexture = new Texture2D(1, 1);
                iconTexture.LoadImage(mod.Icon);

                modListElement.SetModIcon(iconTexture);
            }

            modListElement.transform.SetParent(modList, false);
            modListDictionary.Add(mod, modListElement);

            modListElement.gameObject.SetActive(true);

            return modListElement;
        }

        public ModSettings CreateModSettingWindow(Mod mod)
        {
            ModSettings modSettings = Instantiate(settingsWindowPrefab).GetComponent<ModSettings>();
            modSettings.modContainer = this;
            modSettings.mod = mod;
            modSettings.Name = mod.Name;
            modSettings.ID = mod.ID;
            modSettings.Description = mod.Description;

            modSettings.transform.SetParent(settingsList, false);
            RectTransform rect = modSettings.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);

            mod.modListElement.modSettings = modSettings;
            mod.modListElement.ToggleSettingsOff();

            settingsDictionary.Add(mod, modSettings);

            return modSettings;
        }
    }

    public class ModListElement : MonoBehaviour
    {
        public ModContainer modContainer;

        public Mod mod;
        public ModSettings modSettings;

        public Toggle modToggle, modSettingsToggle;

        public Text nameText, authorText, versionText;
        public RawImage iconImage;

        public string Name
        {
            get => nameText.text; set
            {
                nameText.text = value;
                gameObject.name = value;
            }
        }
        public string Author { get => authorText.text; set => authorText.text = $"AUTHOR(S): {value}"; }
        public string Version { get => versionText.text; set => versionText.text = $"VERSION: {value}"; }

        public void SetModIcon(Texture2D icon)
        {
            iconImage.texture = icon;
        }

        bool suspendAction = false;
        public void ToggleSettingsActive()
        {
            if (suspendAction) return;

            modContainer.modLoaderSettings.ToggleMenuOff();
            foreach (ModListElement otherMod in modContainer.modListDictionary.Values.Where(x => x != this))
                otherMod.ToggleSettingsOff();

            modSettings.gameObject.SetActive(modSettingsToggle.isOn);
        }

        public void ToggleSettingsOff()
        {
            suspendAction = true;

            modSettingsToggle.isOn = false;
            modSettings.gameObject.SetActive(false);

            suspendAction = false;
        }

        public void ToggleModEnabled()
        {
            mod.isDisabled = modToggle.isOn;
        }
    }

    public class ModSettings : MonoBehaviour
    {
        public ModContainer modContainer;
        public Mod mod;

        public Transform settingsList;
        public List<ModSetting> settings = new List<ModSetting>();

        public Text nameText;
        public string Name
        {
            get => nameText.text; set
            {
                nameText.text = value;
                gameObject.name = value;
            }
        }

        public Text idText;
        public string ID
        {
            get => idText.text; set
            {
                gameObject.name = value;
                idText.text = $"ID: {value}";
            }
        }

        public GameObject descriptionHeader;
        public Text descriptionText;
        public string Description
        {
            get => descriptionText.text;
            set
            {
                descriptionHeader.SetActive(!string.IsNullOrEmpty(value));
                descriptionText.text = value;
            }
        }

        public GameObject headerPrefab;
        public SettingHeader AddHeader(string text)
        {
            SettingHeader header = Instantiate(headerPrefab).GetComponent<SettingHeader>();
            header.Text = text;

            header.transform.SetParent(settingsList, false);

            return header;
        }
        public SettingHeader AddHeader(string text, Color backgroundColor)//, Color outlineColor)
        {
            SettingHeader header = Instantiate(headerPrefab).GetComponent<SettingHeader>();
            header.Text = text;
            header.BackgroundColor = backgroundColor;
            //header.OutlineColor = outlineColor;

            header.transform.SetParent(settingsList, false);

            return header;
        }

        public GameObject keybindPrefab;
        public SettingKeybind AddKeybind(string id, string name, KeyCode key, KeyCode[] modifiers = null)
        {
            SettingKeybind keybind = Instantiate(keybindPrefab).GetComponent<SettingKeybind>();
            keybind.ID = id;
            keybind.Name = name;
            keybind.keybind = key;
            keybind.modifiers = modifiers;
            keybind.defaultKeybind = key;
            keybind.defaultModifiers = modifiers;
            keybind.KeyText = keybind.AdjustKeyNames();

            keybind.transform.SetParent(settingsList, false);

            // Something like that
            //Keybind loadedSettings = Settings.LoadKeybind(mod, id);
            //keybind.keybind = loadedSettings.keybind;
            //keybind.modifiers = loadedSettings.modifiers;

            //SaveKeybind(mod, this)

            return keybind;
        }

    }


    public class ModSettings_menu : Mod
    {
        public override string ID => "MSCLoader_Settings";
        public override string Name => "Settings (Main)";
        public override string Author => "Fredrik";
        public override string Version => ModLoader.ModLoaderVersion;
        public override bool LoadInMenu => true;

        private Keybind menuKey = new Keybind("Open", "Open menu", KeyCode.M, KeyCode.LeftControl);

        internal static Settings expWarning = new Settings("mscloader_expWarning", "Show experimental warning", true);
        private static Settings modSetButton = new Settings("mscloader_modSetButton", "Show settings button in bottom right corner", true, ModSettingsToggle);
        internal static Settings forceMenuVsync = new Settings("mscloader_forceMenuVsync", "60FPS limit in Main Menu", true, VSyncSwitchCheckbox);
        internal static Settings openLinksOverlay = new Settings("mscloader_openLinksOverlay", "Open URLs in steam overlay", true);

        private static Settings expUIScaling = new Settings("mscloader_expUIScaling", "Ultra-widescreen UI scaling", false, ExpUIScaling);
        private static Settings tuneScaling = new Settings("mscloader_tuneScale", "Tune scaling:", 1f, ChangeUIScaling);

        private static Settings updateButton = new Settings("mscloader_updateButton", "Check Mod Updates", CheckForUpdateButton);

        public SettingsView settings;
        public GameObject UI,  ModButton, ModButton_Invalid, ModLabel, KeyBind, Checkbox, setBtn, slider, textBox, header, modSettingsButton, modUpdateButton;

        static ModSettings_menu instance;

        public override void ModSettings()
        {
            instance = this;

            Keybind.Add(this, menuKey);

            Settings.AddHeader(this, "Basic Settings", new Color32(0, 128, 0, 255));
            //Settings.AddText(this, "All basic settings for MSCLoader");
            //Settings.AddCheckBox(this, expWarning);
            //Settings.AddCheckBox(this, modPath);
            //Settings.AddCheckBox(this, modSetButton);
            Settings.AddCheckBox(this, forceMenuVsync);
            Settings.AddHeader(this, "Mod Update Check", new Color32(0, 128, 0, 255));
            Settings.AddButton(this, updateButton, "Check for mod updates.");

            //Settings.AddHeader(this, "Experimental Scaling", new Color32(101, 34, 18, 255), new Color32(254, 254, 0, 255));
            //Settings.AddText(this, "This option enables <color=orange>experimental UI scaling</color> for <color=orange>ultra-widescreen monitor setup</color>. Turn on this checkbox first, then run game in ultra-widescreen resolution. You can then tune scaling using slider below, but default value (1) should be ok.");
            //Settings.AddCheckBox(this, expUIScaling);
            //Settings.AddSlider(this, tuneScaling, 0f, 1f);

            LoadSettings();
        }

        public override void ModSettingsLoaded()
        {
            ModSettingsToggle();
            ExpUIScaling();
        }

        private static void ModSettingsToggle() => instance.modSettingsButton.SetActive((bool)modSetButton.GetValue());

        private static void ChangeUIScaling()
        {
            if ((bool)expUIScaling.GetValue())
                ModUI.GetCanvas().GetComponent<CanvasScaler>().matchWidthOrHeight = float.Parse(tuneScaling.GetValue().ToString());
        }

        private static void ExpUIScaling()
        {
            if ((bool)expUIScaling.GetValue())
            {
                ModUI.GetCanvas().GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                ChangeUIScaling();
            }
            else
                ModUI.GetCanvas().GetComponent<CanvasScaler>().screenMatchMode = CanvasScaler.ScreenMatchMode.Shrink;
        }

        private static void VSyncSwitchCheckbox()
        {
            if (ModLoader.GetCurrentScene() == CurrentScene.MainMenu)
                    QualitySettings.vSyncCount = (bool)forceMenuVsync.GetValue() ? 1 : 0;
        }

        private static void CheckForUpdateButton()
        {
           // if (Application.loadedLevelName == "MainMenu")
           //     ModLoader.Instance.InitMetadata();
        }

        public override void OnMenuLoad()
        {
            CreateSettingsUI();
        }

        public void CreateSettingsUI()
        {
            //AssetBundle ab = AssetBundle.CreateFromMemoryImmediate(Properties.Resources.settingsui);
            ////AssetBundle ab = LoadAssets.LoadBundle(this, "settingsui.unity3d");
            //
            //UI = ab.LoadAsset<GameObject>("MSCLoader Settings.prefab");
            //
            //ModButton = ab.LoadAsset<GameObject>("ModButton.prefab");
            //ModButton_Invalid = ab.LoadAsset<GameObject>("ModButton_Invalid.prefab");
            //
            //ModLabel = ab.LoadAsset<GameObject>("ModViewLabel.prefab");
            //
            //KeyBind = ab.LoadAsset<GameObject>("KeyBind.prefab");
            //
            //modSettingsButton = ab.LoadAsset<GameObject>("Button_ms.prefab");
            ////For mod settings
            //Checkbox = ab.LoadAsset<GameObject>("Checkbox.prefab");
            //setBtn = ab.LoadAsset<GameObject>("Button.prefab");
            //slider = ab.LoadAsset<GameObject>("Slider.prefab");
            //textBox = ab.LoadAsset<GameObject>("TextBox.prefab");
            //header = ab.LoadAsset<GameObject>("Header.prefab");
            //
            //UI = UnityEngine.Object.Instantiate(UI);
            //UI.AddComponent<ModUIDrag>();
            //UI.name = "MSCLoader Settings";
            //
            //settings = UI.AddComponent<SettingsView>().Setup(this);
            //
            //modUpdateButton = UnityEngine.Object.Instantiate(modSettingsButton);
            //modUpdateButton.name = "MSCLoader Update button";
            //modUpdateButton.transform.SetParent(ModUI.GetCanvas().transform, false);
            //modUpdateButton.GetComponent<Button>().onClick.AddListener(() => CheckForUpdateButton());
            //modUpdateButton.transform.GetChild(0).GetComponent<Text>().text = "Check Updates";
            //modUpdateButton.SetActive((bool)modSetButton.GetValue());
            //modUpdateButton.AddComponent<SetButtonPos>().button = modUpdateButton;
            //
            //modSettingsButton = UnityEngine.Object.Instantiate(modSettingsButton);
            //modSettingsButton.name = "MSCLoader Settings button";
            //modSettingsButton.transform.SetParent(ModUI.GetCanvas().transform, false);
            //modSettingsButton.GetComponent<Button>().onClick.AddListener(() => settings.ToggleVisibility());
            //modSettingsButton.SetActive((bool)modSetButton.GetValue());
            //
            //ab.Unload(false);
            //
            //// FREDTWEAK
            //Transform canvas = GameObject.Find("MSCLoader Canvas").transform.Find("MSCLoader Settings/MSCLoader SettingsContainer/Settings/SettingsView");
            //canvas.GetChild(0).gameObject.SetActive(false);
            //canvas.GetChild(1).gameObject.SetActive(false);
            //canvas.GetChild(2).gameObject.SetActive(false);
            //
            //GameObject.Find("MSCLoader Canvas").transform.Find("MSCLoader Settings/MSCLoader SettingsContainer/ModKeyBinds/KeyBindsView/Text").GetComponent<Text>().text = "<color=lime><b>LMB</b></color> - Cancel\n<color=lime><b>RMB</b></color> - Set to None";
        }

        class SetButtonPos : MonoBehaviour
        {
            public GameObject button;
            void Start() => StartCoroutine(SetButton());

            IEnumerator SetButton()
            {
                yield return new WaitForSeconds(0.01f);
                button.transform.localPosition += new Vector3(0, 35, 0);
                Destroy(this);
            }
        }

        // Reset keybinds
        public static void ResetBinds(Mod mod)
        {
            if (mod != null)
            {
                // Revert binds
                foreach (Keybind bind in Keybind.Get(mod))
                {
                    Keybind original = Keybind.GetDefault(mod).Find(x => x.ID == bind.ID);

                    if (original != null)
                    {
                        bind.Key = original.Key;
                        bind.Modifier = original.Modifier;
                    }
                }

                // Save binds
                SaveModBinds(mod);
            }
        }

        public static void ResetSettings(Mod mod)
        {
            if (mod != null)
            {
                // Revert settings
                foreach (Settings set in Settings.Get(mod))
                {
                    Settings original = Settings.GetDefault(mod).Find(x => x.ID == set.ID);

                    if (original != null)
                    {
                        set.Value = original.Value;
                    }
                }

                // Save settings
                SaveSettings(mod);
            }
        }
        public static void ResetSpecificSettings(Mod mod, Settings[] sets)
        {
            if (mod != null)
            {
                // Revert settings
                foreach (Settings set in sets)
                {
                    Settings original = Settings.GetDefault(mod).Find(x => x.ID == set.ID);

                    if (original != null)
                    {
                        ModConsole.Print(set.Value + " replaced " + original.Value);
                        set.Value = original.Value;
                    }
                }

                // Save settings
                SaveSettings(mod);
            }
        }

        // Save all keybinds to config files.
        public static void SaveAllBinds()
        {
            foreach (Mod mod in ModLoader.LoadedMods) SaveModBinds(mod);
        }

        // Save keybind for a single mod to config file.
        public static void SaveModBinds(Mod mod)
        {
            KeybindList list = new KeybindList();
            foreach (Keybind bind in Keybind.Get(mod))
            {
                if (bind.ID == null || bind.Vals != null)
                    continue;

                Keybinds keybinds = new Keybinds
                {
                    ID = bind.ID,
                    Key = bind.Key,
                    Modifier = bind.Modifier
                };

                list.keybinds.Add(keybinds);
            }

            if (list.keybinds.Count > 0)
                File.WriteAllText(Path.Combine(ModLoader.GetModSettingsFolder(mod), "keybinds.json"), Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));
        }

        // Save settings for a single mod to config file.
        public static void SaveSettings(Mod mod)
        {
            SettingsList list = new SettingsList { isDisabled = mod.isDisabled };
            foreach (Settings set in Settings.Get(mod).Where(set => 
            set.type != SettingsType.Button && set.type != SettingsType.RButton && set.type != SettingsType.Header && set.type != SettingsType.Text))
            {
                Setting sets = new Setting
                {
                    ID = set.ID,
                    Value = set.Value
                };

                list.settings.Add(sets);
            }

            if (list.isDisabled || list.settings.Count > 0)
                File.WriteAllText(Path.Combine(ModLoader.GetModSettingsFolder(mod), "settings.json"), Newtonsoft.Json.JsonConvert.SerializeObject(list, Newtonsoft.Json.Formatting.Indented));
            else if (Directory.Exists(Path.Combine(ModLoader.SettingsFolder, mod.ID)))
            {
                if (File.Exists(Path.Combine(ModLoader.SettingsFolder, mod.ID + "/settings.json")))
                    File.Delete(Path.Combine(ModLoader.SettingsFolder, mod.ID + "/settings.json"));

                if (Directory.GetDirectories(Path.Combine(ModLoader.SettingsFolder, mod.ID)).Length == 0 && Directory.GetFiles(Path.Combine(ModLoader.SettingsFolder, mod.ID)).Length == 0)
                    Directory.Delete(Path.Combine(ModLoader.SettingsFolder, mod.ID));
            }
        }

        // Load all keybinds.
        public static void LoadBinds()
        {
            foreach (Mod mod in ModLoader.LoadedMods.Where(mod => Keybind.Get(mod).Count > 0))
            {
                // Check if there is custom keybinds file (if not, create)
                string path = Path.Combine(ModLoader.GetModSettingsFolder(mod), "keybinds.json");
                if (!File.Exists(path))
                {
                    SaveModBinds(mod);
                    continue;
                }

                //Load and deserialize 
                KeybindList keybinds = Newtonsoft.Json.JsonConvert.DeserializeObject<KeybindList>(File.ReadAllText(path));

                if (keybinds.keybinds.Count == 0) continue;

                foreach (Keybinds kb in keybinds.keybinds)
                {
                    Keybind bind = Keybind.Keybinds.Find(x => x.Mod == mod && x.ID == kb.ID);

                    if (bind == null) continue;

                    bind.Key = kb.Key;
                    bind.Modifier = kb.Modifier;
                }
            }
        }

        // Load all settings.
        public static void LoadSettings()
        {
            foreach (Mod mod in ModLoader.LoadedMods)
            {
                // Check if there is custom settings file (if not, ignore)
                if (true)//Settings.Get(mod).Where(set => set.type != SettingsType.Button && set.type != SettingsType.Header && set.type != SettingsType.Text).ToList().Count > 0)
                {
                    string path = Path.Combine(ModLoader.GetModSettingsFolder(mod, false), "settings.json");
                    if (!File.Exists(path)) SaveSettings(mod);

                    if (File.Exists(path))
                    {
                        SettingsList settings = Newtonsoft.Json.JsonConvert.DeserializeObject<SettingsList>(File.ReadAllText(path));
                        mod.isDisabled = settings.isDisabled;

                        foreach (Setting kb in settings.settings)
                        {
                            Settings set = Settings.modSettings.Find(x => x.Mod == mod && x.ID == kb.ID);

                            if (set == null) continue;

                            set.Value = kb.Value;
                        }
                        mod.ModSettingsLoaded();
                    }
                }

                try
                {
                    if (mod.LoadInMenu && !mod.isDisabled && mod.fileName != null) mod.OnMenuLoad();
                }
                catch (Exception e)
                {
                    StackFrame frame = new StackTrace(e, true).GetFrame(0);

                    string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                    ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    ModConsole.Error(e.ToString());
                    System.Console.WriteLine(e);
                }
            }
        }

        // Open menu if the key is pressed.
        public override void Update()
        {
            // Open menu
            if (menuKey.GetKeybindDown()) settings.ToggleVisibility();
        }

        // SETUP LOGIC FOR THE MOD SETTINGS BUTTON (FRED TWEAK)
        public void ButtonHandler()
        {
            GameObject.Find("Systems").transform.Find("OptionsMenu").gameObject.AddComponent<ModSettingButtonHandler>().modSettingButton = modSettingsButton;
            modSettingsButton.SetActive(false);
            modUpdateButton.SetActive(false);
        }

        public class ModSettingButtonHandler : MonoBehaviour
        {
            public GameObject modSettingButton;

            void OnEnable()
            {
                modSettingButton.SetActive((bool)modSetButton.GetValue());
            }

            void OnDisable()
            {
                modSettingButton.SetActive(false);
            }
        }
    }
}