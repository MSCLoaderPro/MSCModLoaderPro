extern alias unityUI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Outline = unityUI.UnityEngine.UI.Outline;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary>Parent class for settings.</summary>
    public class ModSetting : MonoBehaviour 
    { 
        /// <summary>Method to save settings into the provided ModConfig.</summary>
        /// <param name="modConfig">ModConfig to save settings to.</param>
        public virtual void SaveSetting(ModConfig modConfig) { } 
    }
    /// <summary>Main Component for the Button setting type.</summary>
    public class SettingButton : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Button button;
        public Image buttonImage;
        public Text buttonText;
        public Shadow buttonTextShadow;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window. An empty string disables the label.</summary>
        public string Name
        {
            get => nameText.text; set
            {
                nameText.text = value;
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
        /// <summary>Text on the button itself.</summary>
        public string ButtonText { get => buttonText.text; set => buttonText.text = value; }
        /// <summary>UI Button's OnClick Eventhandler.</summary>
        public Button.ButtonClickedEvent OnClick { get => button.onClick; set => button.onClick = value; }
        /// <summary>Suspends the action calling if true.</summary>
        public bool suspendActions = false;
        /// <summary>Adds an action that's called whenever the button is clicked.</summary>
        /// <param name="action">Action to call.</param>
        /// <param name="ignoreSuspendActions">(Optional) Should the action always be called regardless of other settings? (Not recommended for regular use)</param>
        public void AddAction(UnityAction action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                button.onClick.AddListener(() => { if (!suspendActions) action.Invoke(); });
            else
                button.onClick.AddListener(action);
        }
    }
    /// <summary>Main Component for the Header setting type.</summary>
    public class SettingHeader : ModSetting
    {
        public LayoutElement layoutElement;
        public Image background;
        public Outline outline;

        public Text text;
        public Shadow textShadow;
        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>The height of the header.</summary>
        public float Height { get => layoutElement.preferredHeight; set => layoutElement.preferredHeight = value; }
        /// <summary>The background color for the header.</summary>
        public Color BackgroundColor { get => background.color; set => background.color = value; }
        /// <summary>The Outline color for the header.</summary>
        public Color OutlineColor { get => outline.effectColor; set => outline.effectColor = value; }
        /// <summary>Text displayed on the header.</summary>
        public string Text { get => text.text; set => text.text = value; }
    }
    /// <summary>Main Component for the Keybind setting type.</summary>
    public class SettingKeybind : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Text keyText;
        public Button keyButton;
        public Image backgroundImage;
        public LayoutElement layoutElement;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window.</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }

        /// <summary>Event that triggers just after the player have started the keybinding process before pressing any keys.</summary>
        public UnityEvent PreBind = new UnityEvent();
        /// <summary>Event that triggers just after the player have assigned a new binding.</summary>
        public UnityEvent PostBind = new UnityEvent();

        public UnityEvent OnKeyDown = new UnityEvent();
        public UnityEvent OnKey = new UnityEvent();
        public UnityEvent OnKeyUp = new UnityEvent();

        /// <summary>Current main key.</summary>
        public KeyCode keybind;
        /// <summary>Current main modifiers.</summary>
        public KeyCode[] modifiers;

        /// <summary>Default key.</summary>
        public KeyCode defaultKeybind;
        /// <summary>Default modifiers.</summary>
        public KeyCode[] defaultModifiers;

        WaitForEndOfFrame wait = new WaitForEndOfFrame();

        [SerializeField]
        GameObject bindButtons;
        bool cancelBind;
        bool deleteBind;

        void Start()
        {
            keyText.text = AdjustKeyNames();
        }

        public void ReassignKey()
        {
            StartCoroutine(BindKey());
        }

        IEnumerator BindKey()
        {
            PreBind.Invoke();

            List<KeyCode> keyCodes = new List<KeyCode>();

            keyText.text = "PRESS KEY(S)";

            layoutElement.preferredHeight = 50f;
            bindButtons.SetActive(true);

            while (true)
            {
                yield return wait;

                if (cancelBind) break;

                if (deleteBind)
                {
                    keybind = KeyCode.None;
                    modifiers = new KeyCode[0];
                    break;
                }

                if (Input.anyKeyDown)
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        // Skip the non-numbered joystick
                        if ((int)key >= 330 && (int)key <= 349) continue;

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
            }

            cancelBind = false;
            deleteBind = false;

            layoutElement.preferredHeight = 25f;
            bindButtons.SetActive(false);

            keyText.text = AdjustKeyNames();

            PostBind.Invoke();
        }

        internal string AdjustKeyNames()
        {
            string text = "";
            foreach (KeyCode key in modifiers) text += $"{key} + ";
            text += $"{keybind}";

            StringBuilder stringBuilder = new StringBuilder(text);

            stringBuilder.Replace("Left", "L-");
            stringBuilder.Replace("Right", "R-");
            stringBuilder.Replace("Control", "CTRL");
            stringBuilder.Replace("Keypad", "Num-");
            stringBuilder.Replace("Alpha", "");

            stringBuilder.Replace("Mouse0", "LMB");
            stringBuilder.Replace("Mouse1", "RMB");
            stringBuilder.Replace("Mouse2", "MB3");
            stringBuilder.Replace("Mouse3", "MB4");
            stringBuilder.Replace("Mouse4", "MB5");

            return stringBuilder.ToString().ToUpper();
        }
        /// <summary>Get if the keybind is held down (including modifiers).</summary>
        public bool GetKey() => GetModifiers() && Input.GetKey(keybind);
        /// <summary>Get if the keybind started being pressed down in the same frame (including modifiers).</summary>
        public bool GetKeyDown() => GetModifiers() && Input.GetKeyDown(keybind);
        /// <summary>Get if the keybind is released in the same frame (including modifiers).</summary>
        public bool GetKeyUp() => (Input.GetKeyUp(keybind) && GetModifiers()) ||
            (Input.GetKey(keybind) && GetModifiersUpAny() && GetModifiersUp());

        /// <summary>Get if the modifiers are pressed down.</summary>
        public bool GetModifiers()
        {
            for (int i = 0; i < modifiers.Length; i++)
                if (!Input.GetKey(modifiers[i])) return false;
            return true;
        }
        /// <summary>Get if the modifiers were released.</summary>
        public bool GetModifiersUp()
        {
            for (int i = 0; i < modifiers.Length; i++)
                if (!Input.GetKeyUp(modifiers[i]) && !Input.GetKey(modifiers[i])) 
                    return false;

            return true;
        }
        /// <summary>Get if the modifiers were released.</summary>
        public bool GetModifiersUpAny()
        {
            for (int i = 0; i < modifiers.Length; i++)
                if (Input.GetKeyUp(modifiers[i])) return true;

            return false;
        }

        /// <summary>Reset the setting to default values.</summary>
        public void ResetToDefaults()
        {
            keybind = defaultKeybind;
            modifiers = defaultModifiers;
            keyText.text = AdjustKeyNames();
        }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Keybinds.Count; i++)
            {
                if (modConfig.Keybinds[i].id == ID) 
                {
                    modConfig.Keybinds[i] = new ModConfigKeybind(ID, keybind, modifiers);
                    return;
                }
            }
            modConfig.Keybinds.Add(new ModConfigKeybind(ID, keybind, modifiers));
        }

        public void CancelBind()
        {
            cancelBind = true;
        }

        public void DeleteBind()
        {
            deleteBind = true;
        }
    }
    /// <summary>Main Component for the Radio Buttons setting type.</summary>
    public class SettingRadioButtons : ModSetting
    {
        [System.Serializable]
        public class RadioEvent : UnityEvent<int> { }

        internal int radioValue;
        public List<RadioButton> buttons = new List<RadioButton>();

        public Text nameText;
        public Shadow nameShadow;
        public ToggleGroup group;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window.</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Current setting value.</summary>
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
                onValueChanged.Invoke(radioValue);
            }
        }

        RadioEvent onValueChanged = new RadioEvent();
        /// <summary>Event that triggers whenever the setting changes value.</summary>
        public RadioEvent OnValueChanged { get => onValueChanged; set => onValueChanged = value; }

        /// <summary>Default setting value</summary>
        public int defaultValue;

        /// <summary>Set the label text of the button with the specified ID.</summary>
        /// <param name="id">ID of the button, throws exception if out of bounds.</param>
        /// <param name="text">Text to display on the label.</param>
        public void SetButtonLabelText(int id, string text)
        {
            if (id >= buttons.Count || id < 0) throw new IndexOutOfRangeException($"ID {id}, out of bounds.");
            buttons[id].labelText.text = text;
        }
        /// <summary>Get the label text of the button with the specified ID.</summary>
        /// <param name="id">ID of the button, throws exception if out of bounds.</param>
        public string GetButtonLabelText(int id)
        {
            if (id >= buttons.Count || id < 0) throw new IndexOutOfRangeException($"ID {id}, out of bounds.");
            return buttons[id].labelText.text;
        }

        public GameObject buttonPrefab;
        public RectTransform toggleGroup;
        /// <summary>Adds a new button to the RadioButtons setting.</summary>
        /// <param name="labelText">Text to display on the label of the new button.</param>
        /// <returns>Created RadioButton.</returns>
        public RadioButton AddButton(string labelText)
        {
            GameObject newButton = Instantiate(buttonPrefab);
            newButton.transform.SetParent(toggleGroup, false);

            RadioButton radioButton = newButton.GetComponent<RadioButton>();
            radioButton.settingRadioButtons = this;
            radioButton.radioID = buttons.Count;
            radioButton.name = $"Radio{radioButton.radioID}";
            radioButton.labelText.text = labelText;
            radioButton.toggle.group = group;
            buttons.Add(radioButton);

            radioButton.suspendSetValue = true;
            radioButton.toggle.isOn = (Value == radioButton.radioID);
            radioButton.suspendSetValue = false;

            return radioButton;
        }
        /// <summary>Suspends the action calling if true.</summary>
        public bool suspendActions = false;
        /// <summary>Adds an action that's called whenever the setting's value changes.</summary>
        /// <param name="action">Action to call.</param>
        /// <param name="ignoreSuspendActions">(Optional) Should the action always be called regardless of other settings? (Not recommended for regular use)</param>
        public void AddAction(UnityAction<int> action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                OnValueChanged.AddListener((actionValue) => { if (!suspendActions) action.Invoke(actionValue); });
            else
                OnValueChanged.AddListener(action);
        }

        /// <summary>Reset the setting to default values.</summary>
        public void ResetToDefaults()
        {
            Value = defaultValue;
        }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Numbers.Count; i++)
            {
                if (modConfig.Numbers[i].id == ID)
                {
                    modConfig.Numbers[i] = new ModConfigNumber(ID, Value);
                    return;
                }
            }
            modConfig.Numbers.Add(new ModConfigNumber(ID, Value));
        }
    }
    /// <summary>Component for the buttons in the Radio Buttons setting.</summary>
    public class RadioButton : MonoBehaviour
    {
        /// <summary>Setting the RadioButton belongs to.</summary>
        public SettingRadioButtons settingRadioButtons;
        /// <summary>ID of the button.</summary>
        public int radioID = 0;

        public Toggle toggle;
        public Image offImage, onImage;

        public Text labelText;
        public Shadow labelShadow;

        public bool suspendSetValue = false;
        public void SetSettingValue()
        {
            if (toggle.isOn && !suspendSetValue)
            {
                settingRadioButtons.radioValue = radioID;
                settingRadioButtons.OnValueChanged.Invoke(radioID);
            }
        }
    }
    /// <summary>Main Component for the Slider setting type.</summary>
    public class SettingSlider : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Text valueText;
        public Shadow valueShadow;

        public Slider slider;
        public Image backgroundImage, handleImage;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window.</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Current setting value.</summary>
        public float Value { get => slider.value; set => slider.value = value; }
        /// <summary>Current setting value as an integer.</summary>
        public int ValueInt { get => (int)slider.value; set => slider.value = value; }

        /// <summary>Minimum slider value.</summary>
        public float MinValue { get => slider.minValue; set => slider.minValue = value; }
        /// <summary>Maximum slider value.</summary>
        public float MaxValue { get => slider.maxValue; set => slider.maxValue = value; }
        /// <summary>Allow only whole numbers?</summary>
        public bool WholeNumbers { get => slider.wholeNumbers; set => slider.wholeNumbers = value; }
        /// <summary>How many digits to round the value to.</summary>
        public int RoundDigits { get => roundDigits; set => roundDigits = Math.Abs(value) % 16; }
        /// <summary>Event that triggers whenever the slider value is changed.</summary>
        public Slider.SliderEvent OnValueChanged { get => slider.onValueChanged; set => slider.onValueChanged = value; }
        /// <summary>Prefix for the value text.</summary>
        public string ValuePrefix
        {
            get => valuePrefix;
            set
            {
                valuePrefix = value;
                ChangeValueText();
            }
        }
        /// <summary>Suffix for the value text.</summary>
        public string ValueSuffix
        {
            get => valueSuffix;
            set
            {
                valueSuffix = value;
                ChangeValueText();
            }
        }
        /// <summary>Text to be displayed instead of the value, determined by index on the array.</summary>public string[] textValues
        public string[] TextValues
        {
            get => textValues;
            set
            {
                textValues = value;
                ChangeValueText();
            }
        }
        /// <summary>Default setting value.</summary>
        public float defaultValue;

        [SerializeField] string valuePrefix = "";
        [SerializeField] string valueSuffix = "";
        [SerializeField] string[] textValues = new string[0];

        public void ChangeValueText()
        {
            valueText.text = (TextValues.Length > ValueInt && ValueInt >= 0) ?
                $"{ValuePrefix}{TextValues[ValueInt]}{ValueSuffix}" :
                $"{ValuePrefix}{Value}{ValueSuffix}";
        }

        internal int roundDigits = -1;
        public void SetRoundValue()
        {
            if (roundDigits >= 0 && !suspendActions)
            {
                suspendActions = true;
                slider.value = (float)Math.Round(slider.value, roundDigits);
                suspendActions = false;
            }
        }
        /// <summary>Suspend action calling.</summary>
        public bool suspendActions = false;
        /// <summary>Add an action to the event that triggers whenever the slider changes value.</summary>
        public void AddAction(UnityAction<float> action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                slider.onValueChanged.AddListener((actionValue) => { if (!suspendActions) action.Invoke(actionValue); });
            else
                slider.onValueChanged.AddListener(action);
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Numbers.Count; i++)
            {
                if (modConfig.Numbers[i].id == ID)
                {
                    modConfig.Numbers[i] = new ModConfigNumber(ID, Value);
                    return;
                }
            }
            modConfig.Numbers.Add(new ModConfigNumber(ID, Value));
        }
    }
    /// <summary>Main Component for the Spacer setting type.</summary>
    public class SettingSpacer : ModSetting
    {
        public LayoutElement layoutElement;
        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>The height of the empty space.</summary>
        public float Height { get => layoutElement.preferredHeight; set => layoutElement.preferredHeight = value; }
    }
    /// <summary>Main Component for the Text setting type.</summary>
    public class SettingText : ModSetting
    {
        public Text text;
        public Shadow textShadow;

        public Image background;
        public Outline outline;
        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Text displayed in the box of text.</summary>
        public string Text { get => text.text; set => text.text = value; }
        /// <summary>Color of the text.</summary>
        public Color TextColor { get => text.color; set => text.color = value; }
        /// <summary>Background color of the header containing the text.</summary>
        public Color BackgroundColor { get => background.color; set => background.color = value; }
        /// <summary>Outline color of the header containing the text.</summary>
        public Color OutlineColor { get => outline.effectColor; set => outline.effectColor = value; }
    }
    /// <summary>Main Component for the TextBox setting type.</summary>
    public class SettingTextBox : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public InputField inputField;
        public Image inputImage;
        public Text inputPlaceholderText;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window.</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Current setting value.</summary>
        public string Value { get => inputField.text; set => inputField.text = value; }
        /// <summary>Placeholder example text.</summary>
        public string Placeholder { get => inputPlaceholderText.text; set => inputPlaceholderText.text = value; }
        /// <summary>What type of characters should be allowed?</summary>
        public InputField.CharacterValidation InputType { get => inputField.characterValidation; set => inputField.characterValidation = value; }
        /// <summary>Event called whenever a character is typed.</summary>
        public InputField.OnChangeEvent OnValueChange { get => inputField.onValueChange; }        
        /// <summary>Event called whenever the text box is exited (Pressing Enter, click outside etc.).</summary>
        public InputField.SubmitEvent OnEndEdit { get => inputField.onEndEdit; }
        /// <summary>Default setting value.</summary>
        public string defaultValue;

        /// <summary>Suspend action calling.</summary>
        public bool suspendActions = false;
        /// <summary>Add an action to the OnEndEdit event.</summary>
        public void AddOnEndEditAction(UnityAction<string> action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                inputField.onEndEdit.AddListener((actionValue) => { if (!suspendActions) action.Invoke(actionValue); });
            else
                inputField.onEndEdit.AddListener(action);
        }
        /// <summary>Add an action to the OnValueChange event.</summary>
        public void AddOnValueChangeAction(UnityAction<string> action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                inputField.onValueChange.AddListener((actionValue) => { if (!suspendActions) action.Invoke(actionValue); });
            else
                inputField.onValueChange.AddListener(action);
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Strings.Count; i++)
            {
                if (modConfig.Strings[i].id == ID)
                {
                    modConfig.Strings[i] = new ModConfigString(ID, Value);
                    return;
                }
            }
            modConfig.Strings.Add(new ModConfigString(ID, Value));
        }
    }
    /// <summary>Main Component for the Toggle setting type.</summary>
    public class SettingToggle : ModSetting
    {
        public Text nameText;
        public Shadow nameShadow;

        public Toggle toggle;
        public Image offImage, onImage;

        /// <summary>Should the setting be shown in the Mod Settings list?</summary>
        public bool Enabled { get => gameObject.activeSelf; set => gameObject.SetActive(value); }
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Setting name, displayed in the settings window.</summary>
        public string Name { get => nameText.text; set => nameText.text = value; }
        /// <summary>Current setting value.</summary>
        public bool Value { get => toggle.isOn; set => toggle.isOn = value; }
        /// <summary>Event that triggers whenever the toggle is pressed.</summary>
        public Toggle.ToggleEvent OnValueChanged { get => toggle.onValueChanged; }
        /// <summary>Default setting value.</summary>
        public bool defaultValue;

        /// <summary>Suspend action calling.</summary>
        public bool suspendActions = false;
        /// <summary>Add an action to the event that triggers whenever the toggle changes value.</summary>
        public void AddAction(UnityAction<bool> action, bool ignoreSuspendActions = false)
        {
            if (!ignoreSuspendActions)
                toggle.onValueChanged.AddListener((actionValue) => { if (!suspendActions) action.Invoke(actionValue); });
            else
                toggle.onValueChanged.AddListener(action);
        }

        public void ResetToDefaults()
        {
            Value = defaultValue;
        }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Booleans.Count; i++)
            {
                if (modConfig.Booleans[i].id == ID)
                {
                    modConfig.Booleans[i] = new ModConfigBool(ID, Value);
                    return;
                }
            }
            modConfig.Booleans.Add(new ModConfigBool(ID, Value));
        }
    }

    // Dummy Settings
    /// <summary>Main Component for the Boolean setting type.</summary>
    public class SettingBoolean : ModSetting
    {
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Current setting value.</summary>
        public bool Value;

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Booleans.Count; i++)
            {
                if (modConfig.Booleans[i].id == ID)
                {
                    modConfig.Booleans[i] = new ModConfigBool(ID, Value);
                    return;
                }
            }
            modConfig.Booleans.Add(new ModConfigBool(ID, Value));
        }
    }
    /// <summary>Main Component for the Number setting type.</summary>
    public class SettingNumber : ModSetting
    {
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Current setting value.</summary>
        public float Value;
        public int ValueInt { get => (int)Value; set => Value = value; }

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Numbers.Count; i++)
            {
                if (modConfig.Numbers[i].id == ID)
                {
                    modConfig.Numbers[i] = new ModConfigNumber(ID, Value);
                    return;
                }
            }
            modConfig.Numbers.Add(new ModConfigNumber(ID, Value));
        }
    }
    /// <summary>Main Component for the String setting type.</summary>
    public class SettingString : ModSetting
    {
        /// <summary>Setting ID. Also determines the containing GameObject's name.</summary>
        public string ID { get => gameObject.name; set => gameObject.name = value; }
        /// <summary>Current setting value.</summary>
        public string Value;

        public override void SaveSetting(ModConfig modConfig)
        {
            for (int i = 0; i < modConfig.Strings.Count; i++)
            {
                if (modConfig.Strings[i].id == ID)
                {
                    modConfig.Strings[i] = new ModConfigString(ID, Value);
                    return;
                }
            }
            modConfig.Strings.Add(new ModConfigString(ID, Value));
        }
    }
}
