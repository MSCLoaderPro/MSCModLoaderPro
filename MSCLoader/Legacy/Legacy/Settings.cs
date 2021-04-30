using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SettingsList
    {
        public bool isDisabled { get; set; }
        public List<Setting> settings = new List<Setting>();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Setting
    {
        public string ID { get; set; }
        public object Value { get; set; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public enum SettingsType
    {
        CheckBoxGroup,
        CheckBox,
        Button,
        RButton,
        Slider,
        TextBox,
        Header,
        Text
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Old Settings is obsolete")]
    public class Settings
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public static List<Settings> modSettings = new List<Settings>();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public static List<Settings> modSettingsDefault = new List<Settings>();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public string ID { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public string Name { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public Mod Mod { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public object Value { get 
            { 
                if (settingType > 0) 
                {
                    if (settingType == 1) return (Mod.modSettings.settings.FirstOrDefault(x => x is SettingToggle && (x as SettingToggle).ID == ID) as SettingToggle).Value;
                    if (settingType == 2) return (Mod.modSettings.settings.FirstOrDefault(x => x is SettingSlider && (x as SettingSlider).ID == ID) as SettingSlider).Value;
                    if (settingType == 3) return (Mod.modSettings.settings.FirstOrDefault(x => x is SettingTextBox && (x as SettingTextBox).ID == ID) as SettingTextBox).Value;
                }
                return settingValue;
            }
            set
            {
                if (settingType > 0)
                {
                    if (settingType == 1) (Mod.modSettings.settings.FirstOrDefault(x => x is SettingToggle && (x as SettingToggle).ID == ID) as SettingToggle).Value = (bool)value;
                    if (settingType == 2) (Mod.modSettings.settings.FirstOrDefault(x => x is SettingSlider && (x as SettingSlider).ID == ID) as SettingSlider).Value = (float)value;
                    if (settingType == 3) (Mod.modSettings.settings.FirstOrDefault(x => x is SettingTextBox && (x as SettingTextBox).ID == ID) as SettingTextBox).Value = (string)value;
                }
                settingValue = value;
            }
            }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public Action DoAction { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public SettingsType type { get; set; }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public object[] Vals { get; set; }

        internal int settingType = 0;
        object settingValue = null;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, object value)
        {
            ID = id;
            Name = name;
            settingValue = value;
            DoAction = null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, Action doAction)
        {
            ID = id;
            Name = name;
            settingValue = "DoAction";
            DoAction = doAction;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, object value, Action doAction)
        {
            ID = id;
            Name = name;
            settingValue = value;
            DoAction = doAction;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public object GetValue() => Value; //Return whatever is there

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public static void HideResetAllButton(Mod mod) { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete")]
        public static List<Settings> GetDefault(Mod mod) => modSettingsDefault.FindAll(x => x.Mod == mod);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.settings instead.")]
        public static List<Settings> Get(Mod mod) => modSettings.FindAll(x => x.Mod == mod);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddToggle() instead.")]
        public static void AddCheckBox(Mod mod, Settings setting)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.settingValue) { Mod = mod });

            if (setting.settingValue is bool boolean)
            {
                setting.type = SettingsType.CheckBox;
                modSettings.Add(setting);

                setting.settingType = 1;
                SettingToggle toggle = mod.modSettings.AddToggle(setting.ID, setting.Name, boolean, (value) => { setting.settingValue = value; });
                if (setting.DoAction != null) toggle.AddAction((value) => setting.DoAction());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddToggle() instead.")]
        public static void AddCheckBox(Mod mod, Settings setting, string group)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.settingValue) { Mod = mod });
            setting.Vals = new object[1];
            
            if (setting.settingValue is bool boolean)
            {
                setting.type = SettingsType.CheckBoxGroup;
                setting.Vals[0] = group;
                modSettings.Add(setting);

                setting.settingType = 1;
                SettingToggle toggle = mod.modSettings.AddToggle(setting.ID, setting.Name, boolean, (value) => { setting.settingValue = value; });
                if (setting.DoAction != null) toggle.AddAction((value) => setting.DoAction());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddButton() instead.")]
        public static void AddButton(Mod mod, Settings setting, string description = null) => 
            AddButton(mod, setting, new UnityEngine.Color32(0, 113, 166, 255), new UnityEngine.Color32(0, 153, 166, 255), new UnityEngine.Color32(0, 183, 166, 255), description);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddButton() instead.")]
        public static void AddButton(Mod mod, Settings setting, UnityEngine.Color normalColor, UnityEngine.Color highlightedColor, UnityEngine.Color pressedColor, string description = null) => 
            AddButton(mod, setting, normalColor, highlightedColor, pressedColor, UnityEngine.Color.white, description);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddButton() instead.")]
        public static void AddButton(Mod mod, Settings setting, UnityEngine.Color normalColor, UnityEngine.Color highlightedColor, UnityEngine.Color pressedColor, UnityEngine.Color buttonTextColor, string description = null)
        {
            setting.Mod = mod;
            setting.Vals = new object[5];
            if (description == null) description = string.Empty;

            if (setting.DoAction != null)
            {
                setting.type = SettingsType.Button;
                setting.Vals[0] = description;
                setting.Vals[1] = normalColor;
                setting.Vals[2] = highlightedColor;
                setting.Vals[3] = pressedColor;
                setting.Vals[4] = buttonTextColor;
                modSettings.Add(setting);

                mod.modSettings.AddButton(setting.ID, setting.Name, description, () => setting.DoAction.Invoke());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddButton: Action cannot be null.");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddSlider() instead.")]
        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue) => 
            AddSlider(mod, setting, minValue, maxValue, null);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddSlider() instead.")]
        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue, string[] textValues)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.settingValue) { Mod = mod });
            setting.Vals = new object[4];

            //sometimes is double or Single (this should fix that, exclude types)
            if (setting.settingValue.GetType() != typeof(float) || setting.settingValue.GetType() != typeof(string))
            {
                setting.settingType = 2;
                SettingSlider slider = mod.modSettings.AddSlider(setting.ID, setting.Name, int.Parse(setting.settingValue.ToString()), minValue, maxValue);
                if (setting.DoAction != null) slider.AddAction((value) => { setting.DoAction.Invoke(); });

                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = true;
                if (textValues == null)
                    setting.Vals[3] = null;
                else
                {
                    slider.TextValues = textValues;
                    setting.Vals[3] = textValues;
                    if (textValues.Length <= (maxValue - minValue))
                        ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: array of textValues is smaller than slider range (min to max).");
                }
                modSettings.Add(setting);
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: only int allowed here");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddSlider() instead.")]
        public static void AddSlider(Mod mod, Settings setting, float minValue, float maxValue, int decimalPoints = 2)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.settingValue) { Mod = mod });
            setting.Vals = new object[4];

            if (setting.settingValue is float || setting.settingValue is double)
            {
                setting.settingType = 2;
                SettingSlider slider = mod.modSettings.AddSlider(setting.ID, setting.Name, float.Parse(setting.settingValue.ToString()), minValue, maxValue, decimalPoints);
                if (setting.DoAction != null) slider.AddAction((value) => { setting.settingValue = slider.Value; setting.DoAction.Invoke(); });

                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = false;
                setting.Vals[3] = null;
                modSettings.Add(setting);
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: only float allowed here");
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddSlider() instead.")]
        public static void AddSlider(Mod mod, Settings setting, float minValue, float maxValue) => 
            AddSlider(mod, setting, minValue, maxValue, 2);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddTextBox() instead.")]
        public static void AddTextBox(Mod mod, Settings setting, string placeholderText) => 
            AddTextBox(mod, setting, placeholderText, UnityEngine.Color.white);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddTextBox() instead.")]
        public static void AddTextBox(Mod mod, Settings setting, string placeholderText, UnityEngine.Color titleTextColor)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.settingValue) { Mod = mod });
            setting.Vals = new object[2];
            setting.type = SettingsType.TextBox;
            setting.Vals[0] = placeholderText;
            setting.Vals[1] = titleTextColor;
            modSettings.Add(setting);

            setting.settingType = 3;
            SettingTextBox textBox = mod.modSettings.AddTextBox(setting.ID, setting.Name, setting.settingValue.ToString(), placeholderText);
            if (setting.DoAction != null) textBox.AddOnValueChangeAction((value) => setting.DoAction());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle) => 
            AddHeader(mod, HeaderTitle, UnityEngine.Color.blue, UnityEngine.Color.white);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle, UnityEngine.Color backgroundColor) => 
            AddHeader(mod, HeaderTitle, backgroundColor, UnityEngine.Color.white);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle, UnityEngine.Color backgroundColor, UnityEngine.Color textColor)
        {
            Settings setting = new Settings(null, HeaderTitle, null)
            {
                Mod = mod,
                Vals = new object[3],
                type = SettingsType.Header
            };
            setting.Vals[0] = HeaderTitle;
            setting.Vals[1] = backgroundColor;
            setting.Vals[2] = textColor;
            modSettings.Add(setting);

            mod.modSettings.AddHeader(HeaderTitle);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Use modSettings.AddText() instead.")]
        public static void AddText(Mod mod, string text)
        {
            Settings setting = new Settings(null, text, null)
            {
                Mod = mod,
                type = SettingsType.Text
            };
            modSettings.Add(setting);

            mod.modSettings.AddText(text);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Settings is obsolete. Does not do anything.")]
        public static void AddResetButton(Mod mod, string name, Settings[] sets)
        {
            if (sets != null)
            {
                Settings setting = new Settings("MSCL_ResetSpecificMod", name, null)
                {
                    Mod = mod,
                    Vals = new object[5],
                    type = SettingsType.RButton
                };
                setting.Vals[0] = sets;
                modSettings.Add(setting);
            }
            else
            {
                ModConsole.LogError($"[<b>{mod.ID}</b>] AddResetButton: provide at least one setting to reset.");
            }
        }
    }
}