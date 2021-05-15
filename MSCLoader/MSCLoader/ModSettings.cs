using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary>Handler for all the mod windows.</summary>
    public class ModContainer : MonoBehaviour
    {
        /// <summary>ModLoader class instance.</summary>
        [SerializeField] internal ModLoader modLoader;

        /// <summary>Dictionary containing all mods' ModSettings Components.</summary>
        public Dictionary<Mod, ModSettings> settingsDictionary = new Dictionary<Mod, ModSettings>();
        /// <summary>Dictionary containing all mods' ModListElement Components.</summary>
        public Dictionary<Mod, ModListElement> modListDictionary = new Dictionary<Mod, ModListElement>();

        /// <summary>UI Text for the mod counter.</summary>
        public Text modCountText;

        /// <summary>Container for the mod loader settings.</summary>
        [SerializeField] internal ModLoaderSettings modLoaderSettings;

        /// <summary>Parent Transform for all mods' list elements.</summary>
        public Transform modList;
        /// <summary>Prefab GameObject for a mod list elements.</summary>
        public GameObject modListElementPrefab;

        /// <summary>Parent Transform for all mods' setting lists.</summary>
        public Transform settingsList;
        /// <summary>Prefab GameObject for a mod settings window.</summary>
        public GameObject settingsWindowPrefab;
        /// <summary>Component responsible for updating.</summary>
        public ModUpdater modUpdater;

        public List<SettingKeybind> keybinds = new List<SettingKeybind>();

        void OnApplicationQuit()
        {
            foreach (ModSettings modSettings in settingsDictionary.Values)
                if (modSettings.loadedSettings != null) modSettings.SaveSettings();
        }
        /// <summary>Creates a ModListElement for the specified mod and adds it to the mod list.</summary>
        /// <param name="mod">Mod to create the ModListElement for.</param>
        /// <returns>Created ModListElement.</returns>
        public ModListElement CreateModListElement(Mod mod)
        {
            ModListElement modListElement = Instantiate(modListElementPrefab).GetComponent<ModListElement>();
            modListElement.modContainer = this;
            modListElement.mod = mod;
            modListElement.ID = mod.ID;
            modListElement.Name = mod.Name;
            modListElement.Author = mod.Author;
            modListElement.Version = mod.Version;

            if (mod.Icon != null)
            {
                Texture2D iconTexture = new Texture2D(1, 1);
                iconTexture.LoadImage(mod.Icon);

                modListElement.SetModIcon(iconTexture);
            }
            else
            {
                string modInfoPath = Path.Combine(NexusMods.NexusSSO.NexusDataFolder, mod.ID);
                if (Directory.Exists(modInfoPath))
                {
                    string infoFile = Path.Combine(modInfoPath, "ModInfo.json");
                    string icon = Path.Combine(modInfoPath, "icon.png");
                    try
                    {
                        if (File.Exists(infoFile))
                        {
                            var info = Newtonsoft.Json.JsonConvert.DeserializeObject<NexusMods.JSONClasses.NexusMods.ModInfo>(File.ReadAllText(infoFile));
                            mod.Description = info.summary;
                        }

                        if (File.Exists(icon))
                        {
                            Texture2D iconTexture = new Texture2D(1, 1);
                            byte[] array = File.ReadAllBytes(icon);
                            iconTexture.LoadImage(array);
                            modListElement.SetModIcon(iconTexture);
                        }
                    }
                    catch
                    {
                        ModConsole.LogError($"Unable to load info of mod {mod.ID}.");
                        if (File.Exists(infoFile))
                            File.Delete(infoFile);
                    }
                }
            }

            modListElement.transform.SetParent(modList, false);
            modListDictionary.Add(mod, modListElement);

            modListElement.gameObject.SetActive(true);

            return modListElement;
        }
        /// <summary>Creates a ModSettings window for the specified mod.</summary>
        /// <param name="mod">Mod to create the ModSettings window for.</param>
        /// <returns>Created ModSettings.</returns>
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
            mod.modListElement.SetSettingsOpen(false);

            settingsDictionary.Add(mod, modSettings);

            return modSettings;
        }
        /// <summary>Updates the mod count text to reflect the current number of mods and how many of them are disabled or has an update available.</summary>
        public void UpdateModCountText()
        {
            modCountText.text = $"{ModLoader.LoadedMods.Count} MODS";

            int disabledMods = ModLoader.LoadedMods.Count(mod => !mod.Enabled);
            if (disabledMods > 0) modCountText.text += $", {disabledMods} DISABLED";

            int updatesAvailable = ModLoader.LoadedMods.Count(x => x.ModUpdateData.UpdateStatus == UpdateStatus.Available);
            if (updatesAvailable > 0) modCountText.text += $", <color=yellow>{updatesAvailable} UPDATE{(updatesAvailable > 1 ? "S" : "")} AVAILABLE</color>";
        }

        internal void DisableModToggle()
        {
            foreach (ModListElement mod in modListDictionary.Values)
                mod.LockModEnabled();
        }

        // CHECK ALL KEYBINDS
        void Update()
        {
            for (int i = 0; i < keybinds.Count; i++)
            {
                if (keybinds[i].GetKeyDown()) keybinds[i].OnKeyDown.Invoke();
                else if (keybinds[i].GetKeyUp()) keybinds[i].OnKeyUp.Invoke();
                else if (keybinds[i].GetKey()) keybinds[i].OnKey.Invoke();
            }
        }
    }

    /// <summary>Control Component for the mod list elements.</summary>
    public class ModListElement : MonoBehaviour
    {
        public ModContainer modContainer;

        /// <summary>The mod this ModListElement belongs to.</summary>
        public Mod mod;
        /// <summary>The ModSettings this ModListElement is linked to.</summary>
        public ModSettings modSettings;

        public Toggle modSettingsToggle;
        public Text nameText, authorText, versionText;
        public RawImage iconImage;
        public GameObject updateButton;

        public Button buttonEnabled;
        public Image imageEnabled;
        public Sprite spriteEnabled, spriteDisabled, spriteLockedEnabled, spriteLockedDisabled;

        public RectTransform openArrowTransform;
        Vector3 openArrowOpen = new Vector3(-1, 1, 1);
        public Shadow openArrowShadow;
        Vector2 openArrowShadowOpen = new Vector2(-1f, -1f);
        Vector2 openArrowShadowClosed = new Vector2(1f, -1f);

        /// <summary>ID for the ModListElement, gets/sets the name of the GameObject the list is on.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Name of the mod, gets/sets the Name UI Text</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Author of the mod, gets/sets the Author UI Text</summary>
        public string Author { get => authorText.text; set => authorText.text = $"AUTHOR{(value.Contains(",") || value.Contains("&") ? "S" : "")}: {value}"; }
        /// <summary>Version of the mod, gets/sets the Version UI Text</summary>
        public string Version { get => versionText.text; set => versionText.text = $"VERSION: {value}"; }

        /// <summary>Set the mod's icon to the specified texture.</summary>
        /// <param name="icon">Texture to use as the icon for the mod.</param>
        public void SetModIcon(Texture2D icon) => iconImage.texture = icon;

        /// <summary>Opens the mod's settings window while closing all others.</summary>
        public void ToggleSettingsOpen() => SetSettingsOpen(!modSettings.gameObject.activeSelf);
        /// <summary>Open/Close the mod's settings window</summary>
        public void SetSettingsOpen(bool open, bool ignoreOthers = false)
        {
            if (!ignoreOthers)
            {
                modContainer.modLoaderSettings.SetSettingsOpen(false, true);
                foreach (ModListElement otherMod in modContainer.modListDictionary.Values.Where(x => x != this))
                    otherMod.SetSettingsOpen(false, true);
            }
            
            modSettings.gameObject.SetActive(open);

            openArrowTransform.localScale = open ? openArrowOpen : Vector3.one;
            openArrowShadow.effectDistance = open ? openArrowShadowOpen : openArrowShadowClosed;
        }

        /// <summary>Toggle the mod's enabled state.</summary>
        public void ToggleModEnabled() => SetModEnabled(!mod.enabled);
        /// <summary>Set the enabled status of the mod.</summary>
        /// <param name="enabled">Enabled/disabled</param>
        /// <param name="callMethods">Call OnModEnabled/Disabled</param>
        public void SetModEnabled(bool enabled, bool callMethods = true)
        {
            mod.enabled = enabled;
            modSettings.SaveSettings();
            modContainer.UpdateModCountText();

            nameText.color = mod.enabled ? ModLoader.MSCYellow : ModLoader.ModDisabledRed;
            imageEnabled.sprite = mod.enabled ? spriteEnabled : spriteDisabled;

            if (mod.enabled) ModLoader.AddToMethodLists(mod); else ModLoader.RemoveFromMethodLists(mod);

            if (callMethods)
            {
                if (mod.enabled) mod.OnModEnabled(); else mod.OnModDisabled();
                ModConsole.Log($"<b>{mod.ID}:</b> {(mod.Enabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
            }
        }
        public void LockModEnabled()
        {
            buttonEnabled.interactable = false;
            imageEnabled.sprite = mod.enabled ? spriteLockedEnabled : spriteLockedDisabled;

        }

        public void ToggleUpdateButton(bool enabled)
        {
            updateButton.SetActive(enabled);
        }

        public void DownloadUpdate()
        {
            ToggleUpdateButton(false);
            if (mod.ModUpdateData.UpdateStatus == UpdateStatus.Available)
                modContainer.modUpdater.DownloadModUpdate(mod);
        }
    }

    /// <summary>Control Component for the mod settings.</summary>
    public class ModSettings : MonoBehaviour
    {
        public ModContainer modContainer;
        /// <summary>The mod this ModListElement belongs to.</summary>
        public Mod mod;

        /// <summary>Parent Transform for all added settings.</summary>
        public Transform settingsList;
        /// <summary>List of added settings.</summary>
        public List<ModSetting> settings = new List<ModSetting>();

        /// <summary>Setting values loaded from file.</summary>
        public ModConfig loadedSettings;

        public GameObject prefabDefaultText, defaultText, resetButton, headerSettings;
        public GameObject prefabButton, prefabHeader, prefabKeybind, prefabRadioButtons, prefabSlider, 
            prefabSpacer, prefabText, prefabTextBox, prefabToggle, prefabBoolean, prefabNumber, prefabString;

        public GameObject descriptionHeader;
        public Text nameText, idText, descriptionText;
        /// <summary>Get/Set the Name UI Text</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Get/Set the ID UI Text, also sets the GameObject name to the ID value.</summary>
        public string ID
        {
            get => idText.text; set
            {
                gameObject.name = value;
                idText.text = $"ID: {value}";
            }
        }
        /// <summary>Get/Set the description UI Text, if set to an empty string it hides the UI element.</summary>
        public string Description
        {
            get => descriptionText.text;
            set
            {
                descriptionText.text = value;
                descriptionHeader.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        void OnEnable()
        {
            CheckForSettings();
            mod?.ModSettingsOpen();
        }

        void OnDisable()
        {
            try { if (loadedSettings != null) SaveSettings(); } catch { }
            mod?.ModSettingsClose();
        }

        public void LoadSettings()
        {
            string path = Path.Combine(ModLoader.GetModSettingsFolder(mod, true), $"{mod.ID}.json");
            if (!File.Exists(path)) SaveSettings();

            loadedSettings = Newtonsoft.Json.JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(path));

            mod.modListElement.SetModEnabled(loadedSettings.Enabled, false);
        }

        public void SaveSettings()
        {
            ModConfig modConfig = loadedSettings ?? new ModConfig();
            modConfig.Enabled = mod.Enabled;

            if (settings.Count > 0) foreach (ModSetting setting in settings) setting.SaveSetting(modConfig);

            string path = $@"{ModLoader.GetModSettingsFolder(mod, true)}\{mod.ID}.json";
            string data = Newtonsoft.Json.JsonConvert.SerializeObject(modConfig, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(path, data);
        }

        void AddSettingToList(ModSetting setting)
        {
            settings.Add(setting);
            setting.transform.SetParent(settingsList, false);

            CheckForSettings();
        }

        void CheckForSettings()
        {
            if ((settings.Count == 0 || settings.All(x => x is SettingBoolean || x is SettingNumber || x is SettingString)) && defaultText == null)
            {
                defaultText = Instantiate(prefabDefaultText);
                defaultText.transform.SetParent(settingsList, false);
                resetButton.SetActive(false);
                headerSettings.SetActive(false);
            }
            else if (settings.Count > 0 && !settings.All(x => x is SettingBoolean || x is SettingNumber || x is SettingString))
            {
                if (defaultText != null) Destroy(defaultText);
                resetButton.SetActive(true);
                headerSettings.SetActive(true);
            }
        }
        /// <summary>Adds a button to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="buttonText">Text to display on the button.</param>
        /// <param name="name">(Optional) Name of the setting, if empty the button will take up the whole space available.</param>
        /// <param name="action">(Optional) UnityAction to execute when clicking the button.</param>
        /// <param name="blockSuspension">(Optional) Should the provided action be disabled if/when actions are disabled on the setting?</param>
        /// <returns>Added SettingButton</returns>
        public SettingButton AddButton(string id, string buttonText, string name = "", UnityAction action = null, bool blockSuspension = false)
        {
            SettingButton button = Instantiate(prefabButton).GetComponent<SettingButton>();
            button.ID = id;
            button.Name = name;
            button.ButtonText = buttonText;

            if (action != null) button.AddAction(action, blockSuspension);

            AddSettingToList(button);

            return button;
        }
        /// <summary>Adds a button to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="buttonText">Text to display on the button.</param>
        /// <param name="action">UnityAction to execute when clicking the button.</param>
        /// <param name="blockSuspension">(Optional) Should the provided action be disabled if/when actions are disabled on the setting?</param>
        /// <returns>Added SettingButton</returns>
        public SettingButton AddButton(string id, string buttonText, UnityAction action = null, bool blockSuspension = false) =>
            AddButton(id, buttonText, "", action, blockSuspension);
        /// <summary>Adds a header to the settings list.</summary>
        /// <param name="text">Text to display on the header.</param>
        /// <returns>Added SettingHeader.</returns>
        public SettingHeader AddHeader(string text)
        {
            SettingHeader header = Instantiate(prefabHeader).GetComponent<SettingHeader>();
            header.Text = text;

            AddSettingToList(header);

            return header;
        }
        /// <summary>Adds a header to the settings list.</summary>
        /// <param name="text">Text to display on the header.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <returns>Added SettingHeader.</returns>
        public SettingHeader AddHeader(string text, Color backgroundColor)
        {
            SettingHeader header = AddHeader(text);
            header.BackgroundColor = backgroundColor;

            return header;
        }
        /// <summary>Adds a header to the settings list.</summary>
        /// <param name="text">Text to display on the header.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <returns>Added SettingHeader.</returns>
        public SettingHeader AddHeader(string text, Color backgroundColor, Color textColor)
        {
            SettingHeader header = AddHeader(text, backgroundColor);
            header.text.color = textColor;

            return header;
        }
        /// <summary>Adds a header to the settings list.</summary>
        /// <param name="text">Text to display on the header.</param>
        /// <param name="backgroundColor">Color of the background.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="outlineColor">Color of the Outline.</param>
        /// <returns>Added SettingHeader.</returns>
        public SettingHeader AddHeader(string text, Color backgroundColor, Color textColor, Color outlineColor)
        {
            SettingHeader header = AddHeader(text, backgroundColor, textColor);
            header.OutlineColor = outlineColor;

            return header;
        }
        /// <summary>Adds a keybind to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="key">Default keycode of the main key.</param>
        /// <param name="modifiers">Keycodes for the default modifiers required to be pressed along with the main key.</param>
        /// <returns>Added SettingKeybind.</returns>
        public SettingKeybind AddKeybind(string id, string name, KeyCode key, params KeyCode[] modifiers)
        {
            SettingKeybind keybind = Instantiate(prefabKeybind).GetComponent<SettingKeybind>();
            keybind.ID = id;
            keybind.Name = name;
            keybind.keybind = key;
            keybind.modifiers = (modifiers.Length > 0 ? modifiers : new KeyCode[0]).Where(x => x != KeyCode.None).ToArray();
            keybind.defaultKeybind = key;
            keybind.defaultModifiers = (modifiers.Length > 0 ? modifiers : new KeyCode[0]).Where(x => x != KeyCode.None).ToArray();
            keybind.keyText.text = keybind.AdjustKeyNames();
            modContainer.keybinds.Add(keybind);

            AddSettingToList(keybind);

            ModConfigKeybind configKeybind = loadedSettings.Keybinds.FirstOrDefault(x => x.id == id);
            if (configKeybind != null)
            {
                keybind.keybind = configKeybind.keybind;
                keybind.modifiers = configKeybind.modifiers.Where(x => x != KeyCode.None).ToArray();
            }

            return keybind;
        }
        /// <summary>Adds radio buttons to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value</param>
        /// <param name="options">Array of buttons, by name, available.</param>
        /// <returns>Added SettingRadioButtons.</returns>
        public SettingRadioButtons AddRadioButtons(string id, string name, int value, params string[] options)
        {
            SettingRadioButtons radioButtons = Instantiate(prefabRadioButtons).GetComponent<SettingRadioButtons>();
            radioButtons.ID = id;
            radioButtons.Name = name;
            for (int i = 0; i < options.Length; i++) radioButtons.AddButton(options[i]);
            radioButtons.Value = value;
            radioButtons.defaultValue = value;

            AddSettingToList(radioButtons);

            ModConfigNumber configNumber = loadedSettings.Numbers.FirstOrDefault(x => x.id == id);
            if (configNumber != null) radioButtons.Value = (int)configNumber.value;

            return radioButtons;
        }
        /// <summary>Adds radio buttons to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action called whenever the value changes. Parameter passes the changed value of the setting.</param>
        /// <param name="options">Array of buttons, by name, available.</param>
        /// <returns>Added SettingRadioButtons.</returns>
        public SettingRadioButtons AddRadioButtons(string id, string name, int value, UnityAction<int> action, params string[] options)
        {
            SettingRadioButtons radioButtons = AddRadioButtons(id, name, value, options);
            if (action != null) radioButtons.AddAction(action);

            return radioButtons;
        }
        /// <summary>Adds radio buttons to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action called whenever the value changes.</param>
        /// <param name="options">Array of buttons, by name, available.</param>
        /// <returns>Added SettingRadioButtons.</returns>
        public SettingRadioButtons AddRadioButtons(string id, string name, int value, UnityAction action, params string[] options)
        {
            SettingRadioButtons radioButtons = AddRadioButtons(id, name, value, options);
            if (action != null) radioButtons.AddAction((x) => action());

            return radioButtons;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="roundDigits">(Optional) The amount of decimals to round to, -1 disables rounding.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, int roundDigits = 2)
        {
            SettingSlider slider = Instantiate(prefabSlider).GetComponent<SettingSlider>();
            slider.ID = id;
            slider.Name = name;
            slider.MinValue = minValue;
            slider.MaxValue = maxValue;
            slider.Value = value;
            slider.defaultValue = value;

            if (roundDigits >= 0) slider.roundDigits = roundDigits;
            slider.SetRoundValue();

            AddSettingToList(slider);

            ModConfigNumber configNumber = loadedSettings.Numbers.FirstOrDefault(x => x.id == id);
            if (configNumber != null) slider.Value = configNumber.value;

            slider.ChangeValueText();

            return slider;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="roundDigits">(Optional) The amount of decimals to round to, -1 disables rounding.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, int roundDigits = 2, UnityAction<float> action = null)
        {
            SettingSlider slider = AddSlider(id, name, value, minValue, maxValue, roundDigits);
            if (action != null) slider.AddAction(action);

            return slider;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="roundDigits">(Optional) The amount of decimals to round to, -1 disables rounding.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, int roundDigits = 2, UnityAction action = null)
        {
            SettingSlider slider = AddSlider(id, name, value, minValue, maxValue, roundDigits);
            if (action != null) slider.AddAction((x) => action());

            return slider;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, UnityAction<float> action = null) => 
            AddSlider(id, name, value, minValue, maxValue, 2, action);
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, UnityAction action = null) => 
            AddSlider(id, name, value, minValue, maxValue, 2, action);
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, int value, int minValue, int maxValue)
        {
            SettingSlider slider = AddSlider(id, name, value, minValue, maxValue, -1);
            slider.WholeNumbers = true;

            return slider;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, int value, int minValue, int maxValue, UnityAction<float> action)
        {
            SettingSlider slider = AddSlider(id, name, value, minValue, maxValue);
            if (action != null) slider.AddAction(action);

            return slider;
        }
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, int value, int minValue, int maxValue, UnityAction action)
        {
            SettingSlider slider = AddSlider(id, name, value, minValue, maxValue);
            if (action != null) slider.AddAction((x) => action());

            return slider;
        }
        /// <summary>Adds an empty space of configurable height to the settings list.</summary>
        /// <param name="height">Height of the spacer.</param>
        /// <returns>Added SettingSpacer.</returns>
        public SettingSpacer AddSpacer(float height)
        {
            SettingSpacer spacer = Instantiate(prefabSpacer).GetComponent<SettingSpacer>();
            spacer.Height = height;

            AddSettingToList(spacer);

            return spacer;
        }
        /// <summary>Adds a non-interactable text to the settings list.</summary>
        /// <param name="text">Text to display.</param>
        /// <returns>Added SettingText.</returns>
        public SettingText AddText(string text)
        {
            SettingText settingText = Instantiate(prefabText).GetComponent<SettingText>();
            settingText.Text = text;

            AddSettingToList(settingText);

            return settingText;
        }
        /// <summary>Adds a non-interactable text to the settings list.</summary>
        /// <param name="text">Text to display.</param>
        /// <param name="backgroundColor">Color of the text background.</param>
        /// <returns>Added SettingText.</returns>
        public SettingText AddText(string text, Color backgroundColor)
        {
            SettingText settingText = AddText(text);
            settingText.BackgroundColor = backgroundColor;

            return settingText;
        }
        /// <summary>Adds a non-interactable text to the settings list.</summary>
        /// <param name="text">Text to display.</param>
        /// <param name="backgroundColor">Color of the text background.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <returns>Added SettingText.</returns>
        public SettingText AddText(string text, Color backgroundColor, Color textColor)
        {
            SettingText settingText = AddText(text);
            settingText.BackgroundColor = backgroundColor;
            settingText.TextColor = textColor;

            return settingText;
        }
        /// <summary>Adds a non-interactable text to the settings list.</summary>
        /// <param name="text">Text to display.</param>
        /// <param name="backgroundColor">Color of the text background.</param>
        /// <param name="textColor">Color of the text.</param>
        /// <param name="outlineColor">Color of the text background's outline.</param>
        /// <returns>Added SettingText.</returns>
        public SettingText AddText(string text, Color backgroundColor, Color textColor, Color outlineColor)
        {
            SettingText settingText = AddText(text);
            settingText.BackgroundColor = backgroundColor;
            settingText.OutlineColor = outlineColor;

            return settingText;
        }
        /// <summary>Adds a text box to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="placeholder">(Optional) Text to add as a placeholder.</param>
        /// <param name="inputType">(Optional) The input type the text box should have, eg. integers, alphanumeric, names etc.</param>
        /// <returns>Added SettingTextBox.</returns>
        public SettingTextBox AddTextBox(string id, string name, string value, string placeholder = "ENTER TEXT...", InputField.CharacterValidation inputType = InputField.CharacterValidation.None)
        {
            SettingTextBox textBox = Instantiate(prefabTextBox).GetComponent<SettingTextBox>();
            textBox.ID = id;
            textBox.Name = name;
            textBox.Value = value;
            textBox.defaultValue = value;
            textBox.Placeholder = placeholder;
            textBox.InputType = inputType;

            AddSettingToList(textBox);

            ModConfigString configString = loadedSettings.Strings.FirstOrDefault(x => x.id == id);
            if (configString != null) textBox.Value = configString.value;


            return textBox;
        }
        /// <summary>Adds a text box to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action to be called whenever the text input changes.</param>
        /// <param name="placeholder">(Optional) Text to add as a placeholder.</param>
        /// <param name="inputType">(Optional) The input type the text box should have, eg. integers, alphanumeric, names etc.</param>
        /// <returns>Added SettingTextBox.</returns>
        public SettingTextBox AddTextBox(string id, string name, string value, UnityAction<string> action, string placeholder = "ENTER TEXT...", InputField.CharacterValidation inputType = InputField.CharacterValidation.None)
        {
            SettingTextBox textBox = AddTextBox(id, name, value, placeholder, inputType);
            if (action != null) textBox.AddOnValueChangeAction(action);

            return textBox;
        }
        /// <summary>Adds a text box to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action to be called whenever the text input changes.</param>
        /// <param name="placeholder">(Optional) Text to add as a placeholder.</param>
        /// <param name="inputType">(Optional) The input type the text box should have, eg. integers, alphanumeric, names etc.</param>
        /// <returns>Added SettingTextBox.</returns>
        public SettingTextBox AddTextBox(string id, string name, string value, UnityAction action, string placeholder = "ENTER TEXT...", InputField.CharacterValidation inputType = InputField.CharacterValidation.None)
        {
            SettingTextBox textBox = AddTextBox(id, name, value, placeholder, inputType);
            if (action != null) textBox.AddOnValueChangeAction((x) => action());

            return textBox;
        }
        /// <summary>Adds a toggle to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <returns>Added SettingToggle.</returns>
        public SettingToggle AddToggle(string id, string name, bool value)
        {
            SettingToggle toggle = Instantiate(prefabToggle).GetComponent<SettingToggle>();
            toggle.ID = id;
            toggle.Name = name;
            toggle.Value = value;
            toggle.defaultValue = value;

            AddSettingToList(toggle);

            ModConfigBool configBool = loadedSettings.Booleans.FirstOrDefault(x => x.id == id);
            if (configBool != null) toggle.Value = configBool.value;

            return toggle;
        }
        /// <summary>Adds a toggle to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action called whenever the value changes.</param>
        /// <returns>Added SettingToggle.</returns>
        public SettingToggle AddToggle(string id, string name, bool value, UnityAction<bool> action)
        {
            SettingToggle toggle = AddToggle(id, name, value);
            toggle.OnValueChanged.AddListener(action);

            return toggle;
        }
        /// <summary>Adds a toggle to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="action">Action called whenever the value changes.</param>
        /// <returns>Added SettingToggle.</returns>
        public SettingToggle AddToggle(string id, string name, bool value, UnityAction action)
        {
            SettingToggle toggle = AddToggle(id, name, value);
            toggle.OnValueChanged.AddListener((x) => action());

            return toggle;
        }
        /// <summary>Adds a hidden Boolean setting.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="value">Default value of the setting.</param>
        /// <returns>Added SettingBoolean</returns>
        public SettingBoolean AddBoolean(string id, bool value)
        {
            SettingBoolean boolean = Instantiate(prefabBoolean).GetComponent<SettingBoolean>();
            boolean.ID = id;
            boolean.Value = value;

            AddSettingToList(boolean);

            ModConfigBool configString = loadedSettings.Booleans.FirstOrDefault(x => x.id == id);
            if (configString != null) boolean.Value = configString.value;

            return boolean;
        }
        /// <summary>Adds a hidden Number setting.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="value">Default value of the setting.</param>
        /// <returns>Added SettingNumber</returns>
        public SettingNumber AddNumber(string id, float value)
        {
            SettingNumber number = Instantiate(prefabNumber).GetComponent<SettingNumber>();
            number.ID = id;
            number.Value = value;

            AddSettingToList(number);

            ModConfigNumber configString = loadedSettings.Numbers.FirstOrDefault(x => x.id == id);
            if (configString != null) number.Value = configString.value;

            return number;
        }
        /// <summary>Adds a hidden Number setting.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="value">Default value of the setting.</param>
        /// <returns>Added SettingNumber</returns>
        public SettingNumber AddNumber(string id, int value)
        {
            SettingNumber number = Instantiate(prefabNumber).GetComponent<SettingNumber>();
            number.ID = id;
            number.ValueInt = value;

            AddSettingToList(number);

            ModConfigNumber configString = loadedSettings.Numbers.FirstOrDefault(x => x.id == id);
            if (configString != null) number.Value = configString.value;

            return number;
        }
        /// <summary>Adds a hidden String setting.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="value">Default value of the setting.</param>
        /// <returns>Added SettingNumber</returns>
        public SettingString AddString(string id, string value)
        {
            SettingString text = Instantiate(prefabString).GetComponent<SettingString>();
            text.ID = id;
            text.Value = value;

            AddSettingToList(text);

            ModConfigString configString = loadedSettings.Strings.FirstOrDefault(x => x.id == id);
            if (configString != null) text.Value = configString.value;

            return text;
        }
    }
}
