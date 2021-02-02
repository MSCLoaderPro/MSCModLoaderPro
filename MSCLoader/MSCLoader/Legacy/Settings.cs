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

    [Obsolete("Old Settings is obsolete")]
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

        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, object value)
        {
            ID = id;
            Name = name;
            Value = value;
            DoAction = null;
        }

        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, Action doAction)
        {
            ID = id;
            Name = name;
            Value = "DoAction";
            DoAction = doAction;
        }

        [Obsolete("Old Settings is obsolete")]
        public Settings(string id, string name, object value, Action doAction)
        {
            ID = id;
            Name = name;
            Value = value;
            DoAction = doAction;
        }

        [Obsolete("Old Settings is obsolete")]
        public object GetValue() => Value; //Return whatever is there

        [Obsolete("Old Settings is obsolete")]
        public static void HideResetAllButton(Mod mod) { }

        [Obsolete("Old Settings is obsolete")]
        public static List<Settings> GetDefault(Mod mod) => modSettingsDefault.FindAll(x => x.Mod == mod);

        [Obsolete("Old Settings is obsolete")]
        public static List<Settings> Get(Mod mod) => modSettings.FindAll(x => x.Mod == mod);

        [Obsolete("Old Settings is obsolete")]
        public static void AddCheckBox(Mod mod, Settings setting)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });

            if (setting.Value is bool)
            {
                setting.type = SettingsType.CheckBox;
                modSettings.Add(setting);

                SettingToggle toggle = mod.modSettings.AddToggle(setting.ID, setting.Name, (bool)setting.Value, (value) => { setting.Value = value; });
                if (setting.DoAction != null) toggle.AddAction((value) => setting.DoAction());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        [Obsolete("Old Settings is obsolete")]
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

                SettingToggle toggle = mod.modSettings.AddToggle(setting.ID, setting.Name, (bool)setting.Value, (value) => { setting.Value = value; });
                if (setting.DoAction != null) toggle.AddAction((value) => setting.DoAction());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddCheckBox: Non-bool value.");
        }

        [Obsolete("Old Settings is obsolete")]
        public static void AddButton(Mod mod, Settings setting, string description = null) => 
            AddButton(mod, setting, new UnityEngine.Color32(0, 113, 166, 255), new UnityEngine.Color32(0, 153, 166, 255), new UnityEngine.Color32(0, 183, 166, 255), description);

        [Obsolete("Old Settings is obsolete")]
        public static void AddButton(Mod mod, Settings setting, UnityEngine.Color normalColor, UnityEngine.Color highlightedColor, UnityEngine.Color pressedColor, string description = null) => 
            AddButton(mod, setting, normalColor, highlightedColor, pressedColor, UnityEngine.Color.white, description);

        [Obsolete("Old Settings is obsolete")]
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

                mod.modSettings.AddButton(setting.ID, description, setting.Name, () => setting.DoAction.Invoke());
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddButton: Action cannot be null.");
        }

        [Obsolete("Old Settings is obsolete")]
        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue) => 
            AddSlider(mod, setting, minValue, maxValue, null);

        [Obsolete("Old Settings is obsolete")]
        public static void AddSlider(Mod mod, Settings setting, int minValue, int maxValue, string[] textValues)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[4];

            //sometimes is double or Single (this should fix that, exclude types)
            if (setting.Value.GetType() != typeof(float) || setting.Value.GetType() != typeof(string))
            {
                SettingSlider slider = mod.modSettings.AddSlider(setting.ID, setting.Name, int.Parse(setting.Value.ToString()), minValue, maxValue);
                if (setting.DoAction != null) slider.AddAction((value) => setting.DoAction());

                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = true;
                if (textValues == null)
                    setting.Vals[3] = null;
                else
                {
                    slider.textValues = textValues;
                    setting.Vals[3] = textValues;
                    if (textValues.Length <= (maxValue - minValue))
                        ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: array of textValues is smaller than slider range (min to max).");
                }
                modSettings.Add(setting);
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: only int allowed here");
        }

        [Obsolete("Old Settings is obsolete")]
        public static void AddSlider(Mod mod, Settings setting, float minValue, float maxValue, int decimalPoints = 2)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[4];

            if (setting.Value is float || setting.Value is double)
            {
                SettingSlider slider = mod.modSettings.AddSlider(setting.ID, setting.Name, float.Parse(setting.Value.ToString()), minValue, maxValue, decimalPoints);
                if (setting.DoAction != null) slider.AddAction((value) => setting.DoAction());

                setting.type = SettingsType.Slider;
                setting.Vals[0] = minValue;
                setting.Vals[1] = maxValue;
                setting.Vals[2] = false;
                setting.Vals[3] = null;
                modSettings.Add(setting);
            }
            else ModConsole.LogError($"[<b>{mod.ID}</b>] AddSlider: only float allowed here");
        }

        [Obsolete("Old Settings is obsolete")]
        public static void AddSlider(Mod mod, Settings setting, float minValue, float maxValue) => 
            AddSlider(mod, setting, minValue, maxValue, 2);

        [Obsolete("Old Settings is obsolete")]
        public static void AddTextBox(Mod mod, Settings setting, string placeholderText) => 
            AddTextBox(mod, setting, placeholderText, UnityEngine.Color.white);

        [Obsolete("Old Settings is obsolete")]
        public static void AddTextBox(Mod mod, Settings setting, string placeholderText, UnityEngine.Color titleTextColor)
        {
            setting.Mod = mod;
            modSettingsDefault.Add(new Settings(setting.ID, setting.Name, setting.Value) { Mod = mod });
            setting.Vals = new object[2];
            setting.type = SettingsType.TextBox;
            setting.Vals[0] = placeholderText;
            setting.Vals[1] = titleTextColor;
            modSettings.Add(setting);

            SettingTextBox textBox = mod.modSettings.AddTextBox(setting.ID, setting.Name, setting.Value.ToString(), placeholderText);
            if (setting.DoAction != null) textBox.AddOnValueChangeAction((value) => setting.DoAction());
        }

        [Obsolete("Old Settings is obsolete")]
        public static void AddHeader(Mod mod, string HeaderTitle) => 
            AddHeader(mod, HeaderTitle, UnityEngine.Color.blue, UnityEngine.Color.white);

        [Obsolete("Old Settings is obsolete")]
        public static void AddHeader(Mod mod, string HeaderTitle, UnityEngine.Color backgroundColor) => 
            AddHeader(mod, HeaderTitle, backgroundColor, UnityEngine.Color.white);

        [Obsolete("Old Settings is obsolete")]
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

        [Obsolete("Old Settings is obsolete")]
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

        [Obsolete("Old Settings is obsolete")]
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