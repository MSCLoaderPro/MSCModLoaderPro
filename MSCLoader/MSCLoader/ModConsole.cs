using MSCLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable IDE1006 // Naming Styles
namespace MSCLoader
{
    public class ModConsole : MonoBehaviour
    {
        public static ModConsole consoleInstance;
        public static bool IsOpen { get; private set; }

        public static ConsoleController controller;
        public GameObject console;

        public Text consoleText;
        public InputField inputField;

        public ModLoaderSettings settings;
        public Text buttonText;

        bool wasFocused;
        int commands, pos;

        public SettingKeybind toggleKey;
        public SettingSlider fontSizeSlider;

        ~ModConsole()
        {
            controller.LogChanged -= LogChanged;
        }

        void Awake()
        {
            consoleInstance = this;
            buttonText.text = console.activeSelf ? "CLOSE" : "OPEN";

            UpdateFontSize();

            controller = new ConsoleController();
            controller.LogChanged += LogChanged;

            UpdateLog(controller.log);
        }

        void Update()
        {
            if (settings.openConsoleKey.GetKeyDown()) ToggleConsole(!console.activeSelf);

            if (console.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Return)) SubmitCommand();

                if (inputField.isFocused)
                {
                    if (!wasFocused)
                    {
                        wasFocused = true;
                        commands = controller.commandHistory.Count;
                        pos = commands;
                    }

                    if (Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        if (commands != 0)
                        {
                            if (pos != 0) pos--;
                            inputField.text = controller.commandHistory[pos];
                            inputField.MoveTextEnd(false);
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.DownArrow))
                    {
                        if (commands != 0)
                        {
                            pos++;
                            if (pos != commands)
                            {
                                inputField.text = controller.commandHistory[pos];
                                inputField.MoveTextEnd(false);
                            }
                            else pos--;

                        }
                    }
                }
                else
                    wasFocused = false;
            }
        }

        // Event that should be called by anything wanting to submit the current input to the console.
        public void SubmitCommand()
        {
            if (inputField.text.Length > 0)
            {
                controller.RunCommandString(inputField.text);
                inputField.text = string.Empty;

                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        public void UpdateFontSize()
        {
            consoleText.fontSize = (int)fontSizeSlider.Value;
        }

        public void ToggleConsole()
        {
            ToggleConsole(!console.activeSelf);
        }

        public static void ToggleConsole(bool enable = false)
        {
            consoleInstance.console.SetActive(enable);
            consoleInstance.inputField.text = string.Empty;
            consoleInstance.buttonText.text = consoleInstance.console.activeSelf ? "CLOSE" : "OPEN";
        }

        void LogChanged(string[] newLog) => UpdateLog(newLog);

        void UpdateLog(string[] newLog) =>
            consoleText.text = newLog == null ? string.Empty : string.Join(Environment.NewLine, newLog);

        public static void Print(string str)
        {
            controller.AppendLogLine(str);
            System.Console.WriteLine($"MSCLoader: {Regex.Replace(str, "<.*?>", "")}");
        }

        public static void Print(object obj)
        {
            controller.AppendLogLine(obj.ToString());
            System.Console.WriteLine($"MSCLoader: {obj}");
        }

        public static void Error(string str = "")
        {
            if (consoleInstance.settings.ConsoleAutoOpen == 1 || consoleInstance.settings.ConsoleAutoOpen == 3) ToggleConsole(true);
            controller.AppendLogLine($"<color=red><b>Error:</b> {str}</color>");
            System.Console.WriteLine($"MSCLoader: Error {Regex.Replace(str, "<.*?>", "")}");
        }

        public static void Warning(string str)
        {
            if (consoleInstance.settings.ConsoleAutoOpen > 1) ToggleConsole(true);
            controller.AppendLogLine(string.Format("<color=yellow><b>Warning: </b>{0}</color>", str));
            System.Console.WriteLine($"MSCLoader: Warning {Regex.Replace(str, "<.*?>", "")}");
        }
    }

    public class ModConsoleResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public ModConsole modConsole;

        public RectTransform console;
        public Vector2 vector;
        public Texture2D cursor;

        public SettingSlider heightSlider, widthSlider;

        Vector2 newSize;

        public void OnBeginDrag(PointerEventData eventData)
        {
            modConsole.settings.disableSave = true;
            heightSlider.suspendOnValueChangedActions = true;
            widthSlider.suspendOnValueChangedActions = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            console.sizeDelta += new Vector2(eventData.delta.x / 1.5f * vector.x, eventData.delta.y / 1.5f * vector.y);
            console.sizeDelta = new Vector2(Mathf.Clamp(console.sizeDelta.x, 250, 1280), Mathf.Clamp(console.sizeDelta.y, 75, 720));

            newSize = console.sizeDelta;

            heightSlider.Value = newSize.y;
            heightSlider.ChangeValueText();

            widthSlider.Value = newSize.x;
            widthSlider.ChangeValueText();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            modConsole.settings.disableSave = false;
            heightSlider.suspendOnValueChangedActions = false;
            widthSlider.suspendOnValueChangedActions = false;

            modConsole.settings.SaveINISettings();
        }

        public void OnPointerEnter(PointerEventData eventData) => Cursor.SetCursor(cursor, new Vector2(24, 24), CursorMode.Auto);
        public void OnPointerExit(PointerEventData eventData) => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        public void UpdateSize()
        {
            console.sizeDelta = new Vector2(widthSlider.Value, heightSlider.Value);
        }
    }

    public class ModConsoleSliderMaxSize : MonoBehaviour
    {
        public Slider slider;
        public bool height = true;

        void Awake()
        {
            slider.maxValue = height ? 720 : 1280;
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
