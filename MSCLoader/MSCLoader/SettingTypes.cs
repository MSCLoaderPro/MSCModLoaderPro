using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MSCLoader
{
    public class ModConfig
    {
        public bool Disabled;
        public ConfigKeybind[] KeyBinds;
        public ConfigNumber[] Numbers;
        public ConfigBool[] Booleans;
        public ConfigString[] Strings;
    }

    public class ConfigKeybind
    {
        public string id;
        public KeyCode[] keybind;
        public ConfigKeybind(string ID, KeyCode[] keys)
        {
            id = ID;
            keybind = keys;
        }
    }
    public class ConfigNumber
    {
        public string id;
        public float value;
        public ConfigNumber(string ID, float number)
        {
            id = ID;
            value = number;
        }
    }
    public class ConfigBool
    {
        public string id;
        public bool value;
        public ConfigBool(string ID, bool boolean)
        {
            id = ID;
            value = boolean;
        }
    }
    public class ConfigString
    {
        public string id;
        public string value;
        public ConfigString(string ID, string text)
        {
            id = ID;
            value = text;
        }
    }

    public class ModSetting : MonoBehaviour { }

    public class SettingHeader : ModSetting
    {
        public ModSettings modSettings;

        public LayoutElement layoutElement;
        public Image background;
        //public Outline outline;
        public Text text;
        public Shadow textShadow;

        public float Height { get => layoutElement.preferredHeight; set => layoutElement.preferredHeight = value; }
        public Color BackgroundColor { get => background.color; set => background.color = value; }
        //public Color OutlineColor { get => outline.effectColor; set => outline.effectColor = value; }
        public string Text { get => text.text; set => text.text = value; }
    }

    public class SettingKeybind : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Text keyText;
        public Button keyButton;
        public Image backgroundImage;

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public string KeyText { get => keyText.text; set => keyText.text = value; }

        public KeyCode keybind;
        public KeyCode[] modifiers;

        public KeyCode defaultKeybind;
        public KeyCode[] defaultModifiers;

        [HideInInspector]
        public Action bindPrefix, bindPostfix;

        public void Start()
        {
            keyText.text = AdjustKeyNames();
        }

        public void ReassignKey()
        {
            StartCoroutine(BindKey());
        }

        IEnumerator BindKey()
        {
            if (bindPrefix != null) bindPrefix.Invoke();

            List<KeyCode> keyCodes = new List<KeyCode>();

            keyText.text = "PRESS KEY(S)";

            while (true)
            {
                if (Input.anyKeyDown)
                {
                    if (Input.GetKeyDown(KeyCode.Mouse0)) break;
                    if (Input.GetKeyDown(KeyCode.Mouse1))
                    {
                        keybind = KeyCode.None;
                        modifiers = new KeyCode[0];
                        break;
                    }
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(key)) keyCodes.Add(key);
                    }
                }

                if (keyCodes.Any(x => Input.GetKeyUp(x)))
                {
                    keybind = keyCodes.FirstOrDefault(x => Input.GetKeyUp(x));
                    keyCodes.Remove(keybind);
                    modifiers = keyCodes.ToArray();
                    break;
                }

                yield return null;
            }

            keyText.text = AdjustKeyNames();

            if (bindPostfix != null) bindPostfix.Invoke();
        }

        public string AdjustKeyNames()
        {
            string text = "";
            foreach (KeyCode key in modifiers)
                text += $"{key}+";
            text += $"{keybind}";

            StringBuilder stringBuilder = new StringBuilder(text);

            stringBuilder.Replace("Left", "L-");
            stringBuilder.Replace("Right", "R-");
            stringBuilder.Replace("Control", "CTRL");
            stringBuilder.Replace("Keypad", "Num-");
            stringBuilder.Replace("Mouse4", "Mb5");
            stringBuilder.Replace("Mouse3", "Mb4");
            stringBuilder.Replace("Mouse2", "Mb3");

            return stringBuilder.ToString().ToUpper();
        }

        public bool GetKey() => GetModifiers() && Input.GetKey(keybind);
        public bool GetKeyDown() => GetModifiers() && Input.GetKeyDown(keybind);
        public bool GetKeyUp() => (Input.GetKeyUp(keybind) && modifiers.All(x => Input.GetKeyUp(x) || Input.GetKey(x))) ||
            (Input.GetKey(keybind) && modifiers.Any(x => Input.GetKeyUp(x)) && modifiers.All(x => Input.GetKeyUp(x) || Input.GetKey(x)));

        public bool GetModifiers() => modifiers.All(x => Input.GetKey(x));

        public void ResetToDefaults()
        {
            keybind = defaultKeybind;
            modifiers = defaultModifiers;
            KeyText = AdjustKeyNames();
        }
    }

    public class SettingToggle : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Toggle toggle;
        public Image offImage, onImage;

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public bool Value { get => toggle.isOn; set => toggle.isOn = value; }
        public Toggle.ToggleEvent OnValueChanged { get => toggle.onValueChanged; }

        public bool suspendOnValueChangedActions = false;

        public bool defaultValue;

        bool actionDelegate = false;
        List<Action> actions = new List<Action>();
        public void AddAction(Action action)
        {
            // INSERT LOGGING?
            actions.Add(action);

            if (!actionDelegate)
            {
                actionDelegate = true;
                toggle.onValueChanged.AddListener(delegate
                {
                    if (!suspendOnValueChangedActions)
                        for (int i = 0; i < actions.Count; i++)
                            actions[i].Invoke();
                });
            }
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }
    }

    public class SettingSlider : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Text valueText;
        public Shadow valueShadow;

        public Slider slider;
        public Image backgroundImage, handleImage;

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public float Value { get => slider.value; set => slider.value = value; }

        public float MinValue { get => slider.minValue; set => slider.minValue = value; }
        public float MaxValue { get => slider.maxValue; set => slider.maxValue = value; }
        public bool WholeNumbers { get => slider.wholeNumbers; set => slider.wholeNumbers = value; }
        public bool RoundValue { get => roundValue; set => roundValue = value; }
        public int RoundDigits { get => roundDigits; set => roundDigits = Math.Abs(value) % 16; }
        public Slider.SliderEvent OnValueChanged { get => slider.onValueChanged; }

        public string valuePrefix = "", valueSuffix = "";
        public string[] textValues = new string[0];

        public bool suspendOnValueChangedActions = false;

        public float defaultValue;

        public void Start() => ChangeValueText();
        public void ChangeValueText()
        {
            valueText.text = (textValues.Length > 0) ?
                $"{valuePrefix}{textValues[(int)Value]}{valueSuffix}" :
                $"{valuePrefix}{Value}{valueSuffix}";
        }

        public bool roundValue = false;
        public int roundDigits = 0;
        public void SetRoundValue()
        {
            if (roundValue && !suspendOnValueChangedActions)
                slider.value = (float)Math.Round(slider.value, roundDigits);
        }

        bool actionDelegate = false;
        List<Action> actions = new List<Action>();
        public void AddAction(Action action)
        {
            // INSERT LOGGING?
            actions.Add(action);

            if (!actionDelegate)
            {
                actionDelegate = true;
                slider.onValueChanged.AddListener(delegate
                {
                    if (!suspendOnValueChangedActions)
                        for (int i = 0; i < actions.Count; i++)
                            actions[i].Invoke();
                });
            }
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }
    }

    public class SettingTextBox : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public InputField inputField;
        public Image inputImage;
        public Text inputPlaceholderText;

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public string Value { get => inputField.text; set => inputField.text = value; }

        public string defaultValue;

        public InputField.OnChangeEvent OnValueChange { get => inputField.onValueChange; }
        public InputField.SubmitEvent OnEndEdit { get => inputField.onEndEdit; }

        public bool suspendOnEndEditActions = false;
        bool onEndEditActionDelegate = false;
        List<Action> onEndEditActions = new List<Action>();
        public void AddOnEndEditAction(Action action)
        {
            onEndEditActions.Add(action);

            if (!onEndEditActionDelegate)
            {
                inputField.onEndEdit.AddListener(delegate
                {
                    if (!suspendOnEndEditActions)
                        for (int i = 0; i < onEndEditActions.Count; i++)
                            onEndEditActions[i].Invoke();
                });
            }
        }

        public bool suspendOnValueChangeActions = false;
        bool onValueChangeActionDelegate = false;
        List<Action> onValueChangeActions = new List<Action>();
        public void AddOnValueChangeAction(Action action)
        {
            onValueChangeActions.Add(action);

            if (!onValueChangeActionDelegate)
            {
                inputField.onValueChange.AddListener(delegate
                {
                    if (!suspendOnValueChangeActions)
                        for (int i = 0; i < onValueChangeActions.Count; i++)
                            onValueChangeActions[i].Invoke();
                });
            }
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }
    }

    public class SettingText : ModSetting
    {
        public Text text;
        public Shadow textShadow;

        public Image background;
        //public Outline outline;

        public string Text { get => text.text; set => text.text = value; }
        public Color TextColor { get => text.color; set => text.color = value; }
        public Color BackgroundColor { get => background.color; set => background.color = value; }
        //public Color OutlineColor { get => outline.effectColor; set => outline.effectColor = value; }
    }

    public class SettingButton : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Button button;
        public Image buttonImage;
        public Text buttonText;
        public Shadow buttonTextShadow;

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public Button.ButtonClickedEvent OnClick { get => button.onClick; set => button.onClick = value; }

        public bool suspendOnClickActions = false;

        bool actionDelegate = false;
        List<Action> actions = new List<Action>();
        public void AddAction(Action action)
        {
            // INSERT LOGGING?
            actions.Add(action);

            if (!actionDelegate)
            {
                actionDelegate = true;
                button.onClick.AddListener(delegate
                {
                    if (!suspendOnClickActions)
                        for (int i = 0; i < actions.Count; i++)
                            actions[i].Invoke();
                });
            }
        }
    }

    public class SettingRadioButtons : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public int radioValue = -1;
        public List<RadioButton> buttons = new List<RadioButton>();

        public string ID { get => gameObject.name; set => gameObject.name = value; }
        public string Name { get => nameText.text; set => nameText.text = value; }
        public int Value
        {
            get => radioValue; set
            {
                radioValue = value;
                for (int i = 0; i < buttons.Count; i++)
                {
                    buttons[i].suspendSetValue = true;
                    buttons[i].toggle.isOn = (i == value);
                    buttons[i].suspendSetValue = false;
                }
            }
        }

        public int defaultValue;

        public void SetButtonLabelText(int id, string text)
        {
            if (id >= buttons.Count || id < 0) throw new Exception("ID, out of bounds.");
            buttons[id].labelText.text = text;
        }
        public string GetButtonLabelText(int id)
        {
            if (id >= buttons.Count || id < 0) throw new Exception("ID, out of bounds.");
            return buttons[id].labelText.text;
        }

        public GameObject buttonPrefab;
        public RectTransform toggleGroup;
        public RadioButton AddButton(string labelText)
        {
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(toggleGroup, false);

            RadioButton radioButton = newButton.GetComponent<RadioButton>();
            radioButton.radioID = buttons.Count;
            radioButton.name = $"Radio{radioButton.radioID}";
            radioButton.labelText.text = labelText;
            buttons.Add(radioButton);

            radioButton.suspendSetValue = true;
            radioButton.toggle.isOn = (Value == radioButton.radioID);
            radioButton.suspendSetValue = false;

            return radioButton;
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }
    }

    public class RadioButton : MonoBehaviour
    {
        public SettingRadioButtons settingRadioButtons;
        public int radioID = 0;

        public Toggle toggle;
        public Image offImage, onImage;

        public Text labelText;
        public Shadow labelShadow;

        public bool suspendSetValue = false;
        public void SetSettingValue()
        {
            if (toggle.isOn && !suspendSetValue)
                settingRadioButtons.radioValue = radioID;
        }
    }
}
