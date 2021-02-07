using Newtonsoft.Json;
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
        public ModLoader modLoader;

        /// <summary>Dictionary containing all mods' ModSettings Components.</summary>
        public Dictionary<Mod, ModSettings> settingsDictionary = new Dictionary<Mod, ModSettings>();
        /// <summary>Dictionary containing all mods' ModListElement Components.</summary>
        public Dictionary<Mod, ModListElement> modListDictionary = new Dictionary<Mod, ModListElement>();

        /// <summary>UI Text for the mod counter.</summary>
        public Text modCountText;

        /// <summary>Container for the mod loader settings.</summary>
        public ModLoaderSettings modLoaderSettings;

        /// <summary>Parent Transform for all mods' list elements.</summary>
        public Transform modList;
        /// <summary>Prefab GameObject for a mod list elements.</summary>
        public GameObject modListElementPrefab;

        /// <summary>Parent Transform for all mods' setting lists.</summary>
        public Transform settingsList;
        /// <summary>Prefab GameObject for a mod settings window.</summary>
        public GameObject settingsWindowPrefab;

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
            //else if (!string.IsNullOrEmpty(mod.IconName))
            //{
            //    Texture2D iconTexture = new Texture2D(1, 1);
            //    iconTexture.LoadImage(GetIcon(mod, mod.IconName));
            //
            //    modListElement.SetModIcon(iconTexture);
            //}

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
            mod.modListElement.ToggleSettingsOff();

            settingsDictionary.Add(mod, modSettings);

            return modSettings;
        }
        /// <summary>Updates the mod count text to reflect the current number of mods and how many of them are disabled.</summary>
        public void UpdateModCountText()
        {
            modCountText.text = $"{ModLoader.LoadedMods.Count} MODS";
            int disabledMods = ModLoader.LoadedMods.Count(mod => !mod.Enabled);
            if (disabledMods > 0) modCountText.text += $", {disabledMods} DISABLED.";
        }

        // NOT WORKING BECAUSE OF UNITY SYSTEM.DRAWING
        //public byte[] GetIcon(Mod mod, string name)
        //{
        //    //https://stackoverflow.com/a/9901769
        //    System.Reflection.Assembly assembly = mod.GetType().Assembly;
        //
        //    string resourceName = assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(name));
        //
        //    ModConsole.Log(resourceName);
        //
        //    using (var stream = assembly.GetManifestResourceStream(resourceName))
        //    {
        //        byte[] buffer = new byte[stream.Length];
        //        stream.Read(buffer, 0, (int)stream.Length);
        //        return buffer;
        //    }
        //}
    }

    /// <summary>Control Component for the mod list elements.</summary>
    public class ModListElement : MonoBehaviour
    {
        public ModContainer modContainer;

        /// <summary>The mod this ModListElement belongs to.</summary>
        public Mod mod;
        /// <summary>The ModSettings this ModListElement is linked to.</summary>
        public ModSettings modSettings;

        public Toggle modToggle, modSettingsToggle;
        public Text nameText, authorText, versionText;
        public RawImage iconImage;
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
        public void SetModIcon(Texture2D icon) =>
            iconImage.texture = icon;

        bool suspendAction = false;
        /// <summary>Opens the mod's settings window while closing all others.</summary>
        public void ToggleSettingsActive()
        {
            if (suspendAction) return;

            modContainer.modLoaderSettings.ToggleMenuOff();
            foreach (ModListElement otherMod in modContainer.modListDictionary.Values.Where(x => x != this))
                otherMod.ToggleSettingsOff();

            modSettings.gameObject.SetActive(modSettingsToggle.isOn);
        }
        /// <summary>Close the mod's settings window</summary>
        public void ToggleSettingsOff()
        {
            suspendAction = true;

            modSettingsToggle.isOn = false;
            modSettings.gameObject.SetActive(false);

            suspendAction = false;
        }
        /// <summary>Toggle the mod's enabled state.</summary>
        public void ToggleModEnabled()
        {
            mod.enabled = modToggle.isOn;
            modSettings.SaveSettings();

            nameText.color = modToggle.isOn ? ModUI.MSCYellow : ModUI.ModDisabledRed;
            modContainer.UpdateModCountText();

            ModConsole.Log($"<b>{mod.ID}:</b> {(mod.Enabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        }
        /// <summary>Set the enabled status of the mod.</summary>
        /// <param name="enabled">Enabled/disabled</param>
        public void SetModEnabled(bool enabled)
        {
            modToggle.isOn = enabled;
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
        public GameObject prefabButton, prefabHeader, prefabKeybind, prefabRadioButtons, prefabSlider, prefabSpacer, prefabText, prefabTextBox, prefabToggle;

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
        }

        void OnDisable()
        {
            try { if (loadedSettings != null) SaveSettings(); } catch { }
        }

        public void LoadSettings()
        {
            string path = Path.Combine(ModLoader.GetModSettingsFolder(mod, true), $"{mod.ID}.json");
            if (!File.Exists(path)) SaveSettings();

            loadedSettings = JsonConvert.DeserializeObject<ModConfig>(File.ReadAllText(path));

            mod.modListElement.SetModEnabled(loadedSettings.Enabled);
        }

        public void SaveSettings()
        {
            ModConfig modConfig = new ModConfig {
                Enabled = mod.Enabled,
                Keybinds = new List<ModConfigKeybind>(),
                Numbers = new List<ModConfigNumber>(),
                Booleans = new List<ModConfigBool>(),
                Strings = new List<ModConfigString>()
            };
            foreach (ModSetting setting in settings) setting.SaveSetting(modConfig);

            string path = $@"{ModLoader.GetModSettingsFolder(mod, true)}\{mod.ID}.json";
            string data = JsonConvert.SerializeObject(modConfig, Formatting.Indented);
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
            if (settings.Count == 0 && defaultText == null)
            {
                defaultText = Instantiate(prefabDefaultText);
                defaultText.transform.SetParent(settingsList, false);
                resetButton.SetActive(false);
                headerSettings.SetActive(false);
            }
            else if (settings.Count > 0)
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
        public SettingButton AddButton(string id, string buttonText, UnityAction action, bool blockSuspension = false) =>
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
            keybind.modifiers = (modifiers.Length > 0 ? modifiers : new KeyCode[0]);
            keybind.defaultKeybind = key;
            keybind.defaultModifiers = (modifiers.Length > 0 ? modifiers : new KeyCode[0]);
            keybind.keyText.text = keybind.AdjustKeyNames();

            AddSettingToList(keybind);

            ModConfigKeybind configKeybind = loadedSettings.Keybinds.FirstOrDefault(x => x.id == id);
            if (configKeybind != null)
            {
                keybind.keybind = configKeybind.keybind;
                keybind.modifiers = configKeybind.modifiers;
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
            slider.MaxValue = maxValue;
            slider.MinValue = minValue;
            slider.Value = value;
            slider.defaultValue = value;

            if (roundDigits >= 0) slider.roundDigits = roundDigits;
            slider.SetRoundValue();

            AddSettingToList(slider);

            ModConfigNumber configNumber = loadedSettings.Numbers.FirstOrDefault(x => x.id == id);
            if (configNumber != null) slider.Value = configNumber.value;

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
            AddSlider(id, name, value, maxValue, minValue, 2, action);
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <param name="action">(Optional) Action to call when the slider value is changed.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, float value, float minValue, float maxValue, UnityAction action = null) => 
            AddSlider(id, name, value, maxValue, minValue, 2, action);
        /// <summary>Adds a slider to the settings list.</summary>
        /// <param name="id">ID of the setting.</param>
        /// <param name="name">Name of the setting</param>
        /// <param name="value">Default value of the setting.</param>
        /// <param name="minValue">Minimum value of the slider.</param>
        /// <param name="maxValue">Maximum value of the slider.</param>
        /// <returns>Added SettingSlider.</returns>
        public SettingSlider AddSlider(string id, string name, int value, int minValue, int maxValue)
        {
            SettingSlider slider = AddSlider(id, name, value, maxValue, minValue, -1);
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
            SettingSlider slider = AddSlider(id, name, value, maxValue, minValue);
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
            SettingSlider slider = AddSlider(id, name, value, maxValue, minValue);
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
        /// <param name="outlineColor">Color of the text background's outline.</param>
        /// <returns>Added SettingText.</returns>
        public SettingText AddText(string text, Color backgroundColor, Color outlineColor)
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
    }
}
