using System;
using System.Collections.Generic;

// GNU GPL 3.0
namespace MSCLoader
{
    public class SettingsList
    {
        public bool isDisabled { get; set; }
        public List<Setting> settings = new List<Setting>();
    }

    public class Setting
    {
        public string ID { get; set; }
        public object Value { get; set; }
    }

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

    public class Settings
    {
        public static List<Settings> modSettings = new List<Settings>();
        public static List<Settings> modSettingsDefault = new List<Settings>();
        public string ID { get; set; }
        public string Name { get; set; }
        public Mod Mod { get; set; }
        public object Value { get; set; }
        public Action DoAction { get; set; }
        public SettingsType type { get; set; }
        public object[] Vals { get; set; }

        public Settings(string id, string name, object value)
        {
            ID = id;
            Name = name;
            Value = value;
            DoAction = null;
        }

        public Settings(string id, string name, Action doAction)
        {
            ID = id;
            Name = name;
            Value = "DoAction";
            DoAction = doAction;
        }

        public Settings(string id, string name, object value, Action doAction)
        {
            ID = id;
            Name = name;
            Value = value;
            DoAction = doAction;
        }

        public object GetValue() => Value; //Return whatever is there

        public static void HideResetAllButton(Mod mod) => 
            modSettingsDefault.Add(new Settings("MSCL_HideResetAllButton", null, null) { Mod = mod });

        public static List<Settings> GetDefault(Mod mod) => 
            modSettingsDefault.FindAll(x => x.Mod == mod);

        public static List<Settings> Get(Mod mod) => modSettings.FindAll(x => x.Mod == mod);

        public static void AddCheckBox(Mod mod, Settings setting)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });

            if (setting.Value is bool)
            {
                setting.type = SettingsType.CheckBox;
                modSettings.Add(setting);
            }
            else ModConsole.Error($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        public static void AddCheckBox(Mod mod, Settings setting, string group)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[1];
            
            if (setting.Value is bool)
            {
                setting.type = SettingsType.CheckBoxGroup;
                setting.Vals[0] = group;
                modSettings.Add(setting);
            }
            else ModConsole.Error($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        public static void AddButton(Mod mod, Settings setting, string description = null) => 
            AddButton(mod, setting, new UnityEngine.Color32(0, 113, 166, 255), new UnityEngine.Color32(0, 153, 166, 255), new UnityEngine.Color32(0, 183, 166, 255), description);

        public static void AddButton(Mod mod, Settings setting, UnityEngine.Color normalColor, UnityEngine.Color highlightedColor, UnityEngine.Color pressedColor, string description = null) => 
            AddButton(mod, setting, normalColor, highlightedColor, pressedColor, UnityEngine.Color.white, description);
        
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
            }
            else ModConsole.Error($"[<b>{mod.ID}</b>] AddButton: Action cannot be null.");
        }

        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue) => 
            AddSlider(mod, setting, minValue, maxValue, null);

        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue, string[] textValues)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[4];

            //sometimes is double or Single (this should fix that, exclude types)
            if (setting.Value.GetType() != typeof(float) || setting.Value.GetType() != typeof(string))
            {
                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = true;
                if (textValues == null)
                    setting.Vals[3] = null;
                else
                {
                    setting.Vals[3] = textValues;
                    if (textValues.Length <= (maxValue - minValue))
                        ModConsole.Error($"[<b>{mod.ID}</b>] AddSlider: array of textValues is smaller than slider range (min to max).");
                }
                modSettings.Add(setting);
            }
            else ModConsole.Error($"[<b>{mod.ID}</b>] AddSlider: only int allowed here");
        }

        public static void AddSlider(Mod mod, Settings setting, float minValue, float maxValue)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[4];

            if (setting.Value is float || setting.Value is double)
            {
                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = false;
                setting.Vals[3] = null;
                modSettings.Add(setting);
            }
            else ModConsole.Error($"[<b>{mod.ID}</b>] AddSlider: only float allowed here");
        }

        public static void AddTextBox(Mod mod, Settings setting, string placeholderText) => 
            AddTextBox(mod, setting, placeholderText, UnityEngine.Color.white);

        public static void AddTextBox(Mod mod, Settings setting, string placeholderText, UnityEngine.Color titleTextColor)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[2];
            setting.type = SettingsType.TextBox;
            setting.Vals[0] = placeholderText;
            setting.Vals[1] = titleTextColor;
            modSettings.Add(setting);
        }

        public static void AddHeader(Mod mod, string HeaderTitle) => 
            AddHeader(mod, HeaderTitle, UnityEngine.Color.blue, UnityEngine.Color.white);

        public static void AddHeader(Mod mod, string HeaderTitle, UnityEngine.Color backgroundColor) => 
            AddHeader(mod, HeaderTitle, backgroundColor, UnityEngine.Color.white);

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
        }

        public static void AddText(Mod mod, string text)
        {
            Settings setting = new Settings(null, text, null)
            {
                Mod = mod,
                type = SettingsType.Text
            };
            modSettings.Add(setting);
        }

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
                ModConsole.Error($"[<b>{mod.ID}</b>] AddResetButton: provide at least one setting to reset.");
            }
        }
    }
}