using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// GNU GPL 3.0
namespace MSCLoader
{
    public class SettingsView : MonoBehaviour
    {
        public ModSettings_menu modSettingsMenu;
        public GameObject settingViewContainer, backButton, modInfo, modKeybinds, keybindsList, modSettings, settingsList, modList, modView;
        public Toggle disableMod, coreModCheckbox;
        public Text modCount, infoText, descriptionText;
        public Button nexusLink, rdLink, ghLink;
        
        int page = 0;

        Mod selectedMod;

        public SettingsView Setup(ModSettings_menu ms)
        {
            modSettingsMenu = ms;

            settingViewContainer = transform.GetChild(0).gameObject;
            backButton = settingViewContainer.transform.GetChild(0).GetChild(1).gameObject;
            modInfo = settingViewContainer.transform.GetChild(2).gameObject;
            modKeybinds = settingViewContainer.transform.GetChild(1).gameObject;
            keybindsList = modKeybinds.transform.GetChild(0).GetChild(4).gameObject;
            modSettings = settingViewContainer.transform.GetChild(4).gameObject;
            settingsList = modSettings.transform.GetChild(0).GetChild(4).gameObject;
            modList = settingViewContainer.transform.GetChild(3).gameObject;
            modView = modList.transform.GetChild(0).gameObject;

            GameObject modSettingsView = modInfo.transform.GetChild(0).gameObject;
            disableMod = modSettingsView.transform.GetChild(2).GetComponent<Toggle>();
            coreModCheckbox = settingViewContainer.transform.GetChild(6).GetChild(0).GetComponent<Toggle>();

            modCount = settingViewContainer.transform.GetChild(6).GetChild(1).GetComponent<Text>();
            infoText = modSettingsView.transform.GetChild(0).GetComponent<Text>();
            descriptionText = modSettingsView.transform.GetChild(8).GetComponent<Text>();

            nexusLink = modSettingsView.transform.GetChild(4).GetComponent<Button>();
            rdLink = modSettingsView.transform.GetChild(5).GetComponent<Button>();
            ghLink = modSettingsView.transform.GetChild(6).GetComponent<Button>();

            backButton.GetComponent<Button>().onClick.AddListener(() => GoBack());
            disableMod.onValueChanged.AddListener(DisableMod);
            coreModCheckbox.onValueChanged.AddListener(delegate { ToggleCoreCheckbox(); });
            settingViewContainer.transform.GetChild(0).GetChild(2).GetComponent<Button>().onClick.AddListener(() => ToggleVisibility());

            transform.SetParent(ModUI.GetCanvas().transform, false);
            SetVisibility(false);

            return this;
        }

        public void ModButton(string name, string version, string author, Mod mod)
        {
            GameObject modButton;
            if (!mod.LoadInMenu && Application.loadedLevelName == "MainMenu")
            {
                modButton = Instantiate(modSettingsMenu.ModButton);
                modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().color = mod.isDisabled ? Color.red : Color.yellow;
                modButton.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = mod.isDisabled ? "<color=red>Mod is disabled!</color>" : "<color=yellow>Ready to load</color>";
            }
            else
            {
                if (mod.isDisabled)
                {
                    modButton = Instantiate(modSettingsMenu.ModButton);
                    modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.red;
                    modButton.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "<color=red>Mod is disabled!</color>";
                }
                else
                {
                    if (mod.ID.StartsWith("MSCLoader_"))
                    {
                        if (coreModCheckbox.isOn)
                        {
                            modButton = Instantiate(modSettingsMenu.ModButton);
                            modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.cyan;
                            modButton.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "<color=cyan>Core Module!</color>";

                        }
                        else return;
                    }
                    else
                    {
                        modButton = Instantiate(modSettingsMenu.ModButton);
                        modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().color = Color.green;
                    }
                }
            }

            if (mod.ID.StartsWith("MSCLoader_")) modButton.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<Button>().interactable = false;

            modButton.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<Button>().onClick.AddListener(() => ModDetailsShow(mod));

            if (Settings.modSettings.Exists(set => set.Mod == mod))
            {
                Button button = modButton.transform.GetChild(1).GetChild(4).GetChild(1).GetComponent<Button>();
                button.interactable = true;
                button.onClick.AddListener(() => ModSettingsShow(mod));
            }

            if (Keybind.Keybinds.Exists(key => key.Mod == mod))
            {
                Button button = modButton.transform.GetChild(1).GetChild(4).GetChild(2).GetComponent<Button>();
                button.interactable = true;
                button.onClick.AddListener(() => ModKeybindsShow(mod));
            }

            if (name.Length > 24)
                modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = string.Format("{0}...", name.Substring(0, 22));
            else
                modButton.transform.GetChild(1).GetChild(0).GetComponent<Text>().text = name;

            modButton.transform.GetChild(1).GetChild(1).GetComponent<Text>().text = string.Format("by <color=orange>{0}</color>", author);
            modButton.transform.GetChild(1).GetChild(2).GetComponent<Text>().text = version;

            modButton.transform.SetParent(modView.transform, false);
            /*
            if (mod.metadata != null && mod.metadata.icon.iconFileName != null && mod.metadata.icon.iconFileName != string.Empty)
            {
                try
                {
                    Texture2D t2d = new Texture2D(1, 1);

                    if (mod.metadata.icon.isIconRemote)
                    {/*
                        if (File.Exists(Path.Combine(ModLoader.ManifestsFolder, @"Mod Icons\" + mod.metadata.icon.iconFileName)))
                        {
                            t2d.LoadImage(File.ReadAllBytes(Path.Combine(ModLoader.ManifestsFolder, @"Mod Icons\" + mod.metadata.icon.iconFileName)));
                            modButton.transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = t2d;
                        }
                    }
                    else if (mod.metadata.icon.isIconUrl)
                        modButton.transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = new WWW(mod.metadata.icon.iconFileName).texture;
                    else
                    {
                        if (File.Exists(Path.Combine(ModLoader.GetModAssetsFolder(mod), mod.metadata.icon.iconFileName)))
                        {
                            t2d.LoadImage(File.ReadAllBytes(Path.Combine(ModLoader.GetModAssetsFolder(mod), mod.metadata.icon.iconFileName)));
                            modButton.transform.GetChild(0).GetChild(0).GetComponent<RawImage>().texture = t2d;
                        }
                    }
                }
                catch (Exception e)
                {
                    ModConsole.Error(e.Message);
                    System.Console.WriteLine(e);
                }
            }*/

            if (mod.hasUpdate) modButton.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "<color=lime>UPDATE AVAILABLE!</color>";

            if (mod.UseAssetsFolder) modButton.transform.GetChild(2).GetChild(2).gameObject.SetActive(true);

            modButton.transform.GetChild(2).GetChild(1).gameObject.SetActive(true);
            if (mod.isDisabled) modButton.transform.GetChild(2).GetChild(1).GetComponent<Image>().color = Color.red;

            if (mod.LoadInMenu) modButton.transform.GetChild(2).GetChild(0).gameObject.SetActive(true);
        }

        public void ToggleCoreCheckbox()
        {
            if (page == 0) CreateList();
        }

        public void SettingsList(Settings setting)
        {
            switch (setting.type)
            {
                case SettingsType.CheckBox:

                    GameObject checkbox = Instantiate(modSettingsMenu.Checkbox);
                    checkbox.transform.GetChild(1).GetComponent<Text>().text = setting.Name;

                    Toggle toggle = checkbox.GetComponent<Toggle>();
                    toggle.isOn = (bool)setting.Value;
                    toggle.onValueChanged.AddListener(delegate
                    {
                        setting.Value = toggle.isOn;
                        setting.DoAction?.Invoke();
                    });

                    checkbox.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.CheckBoxGroup:

                    GameObject group;
                    if (settingsList.transform.FindChild(setting.Vals[0].ToString()) == null)
                    {
                        group = new GameObject { name = setting.Vals[0].ToString() };
                        group.AddComponent<ToggleGroup>();
                        group.transform.SetParent(settingsList.transform, false);
                    }
                    else
                        group = settingsList.transform.FindChild(setting.Vals[0].ToString()).gameObject;

                    GameObject checkboxG = Instantiate(modSettingsMenu.Checkbox);
                    checkboxG.transform.GetChild(1).GetComponent<Text>().text = setting.Name;

                    Toggle toggleG = checkboxG.GetComponent<Toggle>();
                    toggleG.group = group.GetComponent<ToggleGroup>();
                    toggleG.isOn = (bool)setting.Value;

                    if((bool)setting.Value) toggleG.group.NotifyToggleOn(toggleG);

                    toggleG.onValueChanged.AddListener(delegate
                    {
                        setting.Value = toggleG.isOn;
                        setting.DoAction?.Invoke();
                    });

                    checkboxG.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.Button:

                    GameObject button = Instantiate(modSettingsMenu.setBtn);
                    button.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = setting.Name;
                    button.transform.GetChild(1).GetComponent<Text>().text = setting.Vals[0].ToString();
                    button.transform.GetChild(1).GetComponent<Text>().color = (Color)setting.Vals[4];

                    if (setting.Vals[0].ToString() == null || setting.Vals[0].ToString() == string.Empty)
                        button.transform.GetChild(1).gameObject.SetActive(false);

                    button.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(setting.DoAction.Invoke);

                    ColorBlock cb = button.transform.GetChild(0).GetComponent<Button>().colors;
                    cb.normalColor = (Color)setting.Vals[1];
                    cb.highlightedColor = (Color)setting.Vals[2];
                    cb.pressedColor = (Color)setting.Vals[3];

                    button.transform.GetChild(0).GetComponent<Button>().colors = cb; 
                    button.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.RButton:

                    GameObject rbtn = Instantiate(modSettingsMenu.setBtn);
                    rbtn.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = setting.Name;
                    rbtn.transform.GetChild(0).GetChild(0).GetComponent<Text>().color = Color.black;
                    rbtn.transform.GetChild(1).gameObject.SetActive(false);
                    rbtn.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate
                    {
                        ModSettings_menu.ResetSpecificSettings(setting.Mod, (Settings[])setting.Vals[0]);
                        ModSettingsShow(setting.Mod);
                        setting.Mod.ModSettingsLoaded();
                    });
                    ColorBlock rcb = rbtn.transform.GetChild(0).GetComponent<Button>().colors;
                    rcb.normalColor = new Color32(255, 187, 5, 255);
                    rcb.highlightedColor = new Color32(255, 230, 5, 255);
                    rcb.pressedColor = new Color32(255, 230, 5, 255);
                    rbtn.transform.GetChild(0).GetComponent<Button>().colors = rcb;
                    rbtn.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.Slider:

                    GameObject modViewLabel = Instantiate(modSettingsMenu.ModLabel);
                    modViewLabel.GetComponent<Text>().text = setting.Name;
                    modViewLabel.transform.SetParent(settingsList.transform, false);

                    GameObject slidr = Instantiate(modSettingsMenu.slider);
                    slidr.transform.GetChild(1).GetComponent<Text>().text = setting.Value.ToString();

                    Slider slider = slidr.transform.GetChild(0).GetComponent<Slider>();
                    slider.minValue = float.Parse(setting.Vals[0].ToString());
                    slider.maxValue = float.Parse(setting.Vals[1].ToString());
                    slider.value = float.Parse(setting.Value.ToString());
                    slider.wholeNumbers = (bool)setting.Vals[2];
                    if (setting.Vals[3] != null)
                        slidr.transform.GetChild(1).GetComponent<Text>().text = ((string[])setting.Vals[3])[int.Parse(setting.Value.ToString())];

                    slider.onValueChanged.AddListener(delegate
                    {
                        if ((bool)setting.Vals[2]) setting.Value = slider.value;
                        else setting.Value = Math.Round(slider.value, 1);
                        if (setting.Vals[3] == null)
                            slidr.transform.GetChild(1).GetComponent<Text>().text = setting.Value.ToString();
                        else
                            slidr.transform.GetChild(1).GetComponent<Text>().text = ((string[])setting.Vals[3])[int.Parse(setting.Value.ToString())];

                        setting.DoAction?.Invoke();
                    });

                    slidr.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.TextBox:

                    GameObject modViewLabels = Instantiate(modSettingsMenu.ModLabel);
                    modViewLabels.GetComponent<Text>().text = setting.Name;
                    modViewLabels.GetComponent<Text>().color = (Color)setting.Vals[1];
                    modViewLabels.transform.SetParent(settingsList.transform, false);

                    GameObject txt = Instantiate(modSettingsMenu.textBox);
                    txt.transform.GetChild(0).GetComponent<Text>().text = setting.Vals[0].ToString();
                    InputField inputField = txt.GetComponent<InputField>();
                    inputField.text = setting.Value.ToString();
                    inputField.onValueChange.AddListener( delegate { setting.Value = inputField.text; });

                    txt.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.Header:

                    GameObject hdr = Instantiate(modSettingsMenu.header);
                    hdr.transform.GetChild(0).GetComponent<Text>().text = setting.Name;
                    hdr.GetComponent<Image>().color = (Color)setting.Vals[1];
                    hdr.transform.GetChild(0).GetComponent<Text>().color = (Color)setting.Vals[2];
                    hdr.transform.SetParent(settingsList.transform, false);

                    break;
                case SettingsType.Text:

                    GameObject tx = Instantiate(modSettingsMenu.ModLabel);
                    tx.GetComponent<Text>().text = setting.Name;
                    tx.transform.SetParent(settingsList.transform, false);

                    break;
            }
        }

        public void KeyBindHeader(Keybind key)
        {
            Transform hdr = Instantiate(modSettingsMenu.header).transform;
            hdr.GetChild(0).GetComponent<Text>().text = key.Name;
            hdr.GetComponent<Image>().color = (Color)key.Vals[1];
            hdr.GetChild(0).GetComponent<Text>().color = (Color)key.Vals[2];
            hdr.SetParent(keybindsList.transform, false);
        }

        public void KeyBindsList(Keybind key)
        {
            GameObject keyBind = Instantiate(modSettingsMenu.KeyBind);
            keyBind.transform.GetChild(0).GetComponent<Text>().text = key.Name;

            KeyBinding keyBinding = keyBind.AddComponent<KeyBinding>();
            keyBinding.modifierButton = keyBind.transform.GetChild(1).gameObject;
            keyBinding.modifierDisplay = keyBind.transform.GetChild(1).GetChild(0).GetComponent<Text>();
            keyBinding.keyButton = keyBind.transform.GetChild(3).gameObject;
            keyBinding.keyDisplay = keyBind.transform.GetChild(3).GetChild(0).GetComponent<Text>();
            keyBinding.key = key.Key;
            keyBinding.modifierKey = key.Modifier;
            keyBinding.mod = selectedMod;
            keyBinding.id = key.ID;
            keyBinding.LoadBind();

            keyBind.transform.SetParent(keybindsList.transform, false);
        }

        public void RemoveChildren(Transform parent)
        {
            foreach (Transform child in parent) Destroy(child.gameObject);
        }

        public void GoBack()
        {
            Animator anim = settingViewContainer.GetComponent<Animator>();
            switch (page)
            {
                case 1:

                    page = 0;
                    SetScrollRect();
                    CreateList();
                    anim.SetBool("goDetails", false);
                    backButton.SetActive(false);

                    break;
                case 2:

                    page = 0;
                    SetScrollRect();
                    anim.SetBool("goKeybind", false);
                    backButton.SetActive(false);

                    break;
                case 3:

                    page = 0;
                    SetScrollRect();
                    ModSettings_menu.SaveSettings(selectedMod);
                    anim.SetBool("goModSetting", false);
                    backButton.SetActive(false);
                    RemoveChildren(settingsList.transform);

                    break;
                default: break;
            }

        }

        void SetScrollRect()
        {
            modSettings.GetComponent<ScrollRect>().enabled = (page == 3);
            modKeybinds.GetComponent<ScrollRect>().enabled = (page == 2);
            modInfo.GetComponent<ScrollRect>().enabled = (page == 1);
            modList.GetComponent<ScrollRect>().enabled = (page == 0);
        }

        public void GoToKeybinds()
        {
            settingViewContainer.GetComponent<Animator>().SetBool("goKeybind", true);
            page = 2;
            SetScrollRect();
        }

        public void GoToSettings()
        {
            settingViewContainer.GetComponent<Animator>().SetBool("goModSetting", true);
            page = 3;
            SetScrollRect();
        }

        public void DisableMod(bool ischecked)
        {
            if (selectedMod.isDisabled != ischecked)
            {
                selectedMod.isDisabled = ischecked;
                ModConsole.Print(string.Format("Mod <b><color=orange>{0}</color></b> is <color=red><b>{1}</b></color>", selectedMod.Name, ischecked ? "Disabled" : "Enabled"));
                ModSettings_menu.SaveSettings(selectedMod);
            }
        }

        public void ModDetailsShow(Mod mod)
        {/*
            selectedMod = mod;
            backButton.SetActive(true);

            infoText.text = string.Format(
                "<color=yellow>ID:</color> <b><color=lime>{0}</color></b>{1}" +
                "<color=yellow>Name:</color> <b><color=lime>{2}</color></b>{1}" +
                "<color=yellow>Version:</color> <b><color=orange>{3}</color></b>{4}" +
                " (designed for <b><color=lime>v{5}</color></b>){1}" +
                "<color=yellow>Author:</color> <b><color=lime>{6}</color></b>", 
                mod.ID, Environment.NewLine, mod.Name, mod.Version, mod.hasUpdate ? "(<color=lime>" + mod.RemMetadata.version + " available</color>)" : "", mod.compiledVersion, mod.Author
                );

            disableMod.interactable = Application.loadedLevelName == "MainMenu";
            disableMod.isOn = mod.isDisabled;

            nexusLink.gameObject.SetActive(false);
            rdLink.gameObject.SetActive(false);
            ghLink.gameObject.SetActive(false);

            descriptionText.text = "";

            if (mod.metadata != null)
            {
                if (!string.IsNullOrEmpty(mod.metadata.links.nexusLink))
                {
                    nexusLink.gameObject.SetActive(true);
                    nexusLink.onClick.RemoveAllListeners();
                    nexusLink.onClick.AddListener(() => OpenModLink(mod.metadata.links.nexusLink));
                }
                if (!string.IsNullOrEmpty(mod.metadata.links.rdLink))
                {
                    rdLink.gameObject.SetActive(true);
                    rdLink.onClick.RemoveAllListeners();
                    rdLink.onClick.AddListener(() => OpenModLink(mod.metadata.links.rdLink));
                }
                if (!string.IsNullOrEmpty(mod.metadata.links.githubLink))
                {
                    ghLink.gameObject.SetActive(true);
                    ghLink.onClick.RemoveAllListeners();
                    ghLink.onClick.AddListener(() => OpenModLink(mod.metadata.links.githubLink));
                }
                descriptionText.text = mod.metadata.description;
            }
            settingViewContainer.GetComponent<Animator>().SetBool("goDetails", true);
            
            page = 1;
            SetScrollRect();*/
        }
        
        void OpenModLink(string url)
        {
            Application.OpenURL(url);
            System.Console.WriteLine(url);
        }

        public void ModKeybindsShow(Mod selected)
        {
            backButton.SetActive(true);
            selectedMod = selected;
            RemoveChildren(keybindsList.transform);

            foreach (Keybind key in Keybind.Keybinds.Where(key => key.Mod == selected))
            {
                if (key.ID == null && key.Vals != null) KeyBindHeader(key);
                else KeyBindsList(key);
            }

            Button button = modKeybinds.transform.GetChild(0).GetChild(6).GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate 
            {
                ModSettings_menu.ResetBinds(selected);
                ModKeybindsShow(selected);
            });

            GoToKeybinds();
        }

        public void ModSettingsShow(Mod selected)
        {
            backButton.SetActive(true);
            selectedMod = selected;
            RemoveChildren(settingsList.transform);

            foreach (Settings set in Settings.modSettings.Where(set => set.Mod == selected))
                SettingsList(set);

            if (Settings.GetDefault(selected).Count == 0 || Settings.GetDefault(selected).Find(x => x.ID == "MSCL_HideResetAllButton") != null)
            {
                modSettings.transform.GetChild(0).GetChild(6).gameObject.SetActive(false);
            }
            else
            {
                modSettings.transform.GetChild(0).GetChild(6).gameObject.SetActive(true);
                modSettings.transform.GetChild(0).GetChild(6).GetComponent<Button>().onClick.RemoveAllListeners();
                modSettings.transform.GetChild(0).GetChild(6).GetComponent<Button>().onClick.AddListener(delegate
                {
                    ModSettings_menu.ResetSettings(selected);
                    ModSettingsShow(selected);
                    selected.ModSettingsLoaded();

                });
            }

            GoToSettings();
        }

        void ModListHeader(string header, Color background, Color text)
        {
            GameObject hdr = Instantiate(modSettingsMenu.header);
            hdr.transform.GetChild(0).GetComponent<Text>().text = header;
            hdr.GetComponent<Image>().color = background;
            hdr.transform.GetChild(0).GetComponent<Text>().color = text;
            hdr.transform.SetParent(modView.transform, false);
        }

        void CreateList()
        {
            RemoveChildren(modView.transform);

            if (coreModCheckbox.isOn) foreach (Mod mod in ModLoader.LoadedMods.Where(mod => mod.ID.StartsWith("MSCLoader_")))
                    ModButton(mod.Name, mod.Version, mod.Author, mod);

            foreach (Mod mod in ModLoader.LoadedMods.Where(mod => mod.hasUpdate))
                ModButton(mod.Name, mod.Version, mod.Author, mod);

            if (Application.loadedLevelName == "MainMenu")
            {
                foreach (Mod mod in ModLoader.LoadedMods.Where(mod => mod.LoadInMenu && !mod.ID.StartsWith("MSCLoader_") && mod.LoadInMenu))
                        ModButton(mod.Name, mod.Version, mod.Author, mod);

                foreach (Mod mod in ModLoader.LoadedMods.Where(mod => !mod.ID.StartsWith("MSCLoader_") && !mod.LoadInMenu && !mod.isDisabled))
                        ModButton(mod.Name, mod.Version, mod.Author, mod);
            }
            else
            {
                foreach (Mod mod in ModLoader.LoadedMods.Where(mod => !mod.ID.StartsWith("MSCLoader_") && !mod.isDisabled))
                        ModButton(mod.Name, mod.Version, mod.Author, mod);
            }

            foreach (Mod mod in ModLoader.LoadedMods.Where(mod => mod.isDisabled))
                ModButton(mod.Name, mod.Version, mod.Author, mod);
            /*
            foreach (string invalidMod in ModLoader.InvalidMods)
            {
                GameObject invMod = Instantiate(modSettingsMenu.ModButton_Invalid);
                invMod.transform.GetChild(0).GetComponent<Text>().text = invalidMod;
                invMod.transform.SetParent(modView.transform, false);
            }*/
        }

        public void ToggleVisibility()
        {
            if (!settingViewContainer.activeSelf)
            {
                modCount.text = string.Format("<color=orange><b>{0}</b></color> Mods", ModLoader.LoadedMods.Count - 2);
                CreateList();
                page = 0;
                SetScrollRect();
                SetVisibility(!settingViewContainer.activeSelf);
                backButton.SetActive(false);
            }
            else
            {
                if (page == 3)
                {
                    ModSettings_menu.SaveSettings(selectedMod);
                    RemoveChildren(settingsList.transform);
                }
                SetVisibility(!settingViewContainer.activeSelf);
            }
        }

        public void SetVisibility(bool visible)
        {
            settingViewContainer.SetActive(visible);
        }
    }
}