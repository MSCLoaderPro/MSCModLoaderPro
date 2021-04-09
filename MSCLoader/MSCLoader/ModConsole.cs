using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable CS1591, IDE1006
namespace MSCLoader
{
    /// <summary>The handler for the ModConsole</summary>
    public class ModConsole : MonoBehaviour
    {
        public static ModConsole consoleInstance; 

        public static ConsoleController controller { get; internal set; }
        [SerializeField] internal GameObject console;

        [SerializeField] internal Text consoleText;
        [SerializeField] internal InputField inputField;

        [SerializeField] internal ModLoaderSettings settings;
        [SerializeField] internal Text buttonText;

        bool wasFocused;
        int commandHistoryIndex;

        [SerializeField] internal SettingKeybind toggleKey;
        [SerializeField] internal SettingSlider fontSizeSlider;

        ~ModConsole()
        {
            controller.LogChanged -= UpdateLog;
        }

        void Awake()
        {
            consoleInstance = this;

            UpdateFontSize();

            controller = new ConsoleController();
            controller.LogChanged += UpdateLog;

            UpdateLog(controller.scrollback.ToArray());
        }
        
        void Start()
        {
            console.SetActive(false);
            buttonText.text = "OPEN CONSOLE";
        }

        void Update()
        {
            if (settings.openConsoleKey.GetKeyDown()) ToggleConsole(!console.activeSelf);

            if (console.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.Return)) SubmitCommand();

                if (inputField.isFocused) CommandHistory();
                else wasFocused = false;
            }
        }

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
            consoleInstance.buttonText.text = consoleInstance.console.activeSelf ? "CLOSE CONSOLE" : "OPEN CONSOLE";
        }

        void CommandHistory()
        {
            if (!wasFocused)
            {
                wasFocused = true;
                commandHistoryIndex = controller.commandHistory.Count;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (controller.commandHistory.Count > 0)
                {
                    if (commandHistoryIndex > 0) commandHistoryIndex--;
                    SetCommandText();
                }
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (controller.commandHistory.Count > 0)
                {
                    commandHistoryIndex++;
                    if (commandHistoryIndex < controller.commandHistory.Count) SetCommandText();
                    else commandHistoryIndex--;

                }
            }
        }

        void SetCommandText()
        {
            inputField.text = controller.commandHistory[commandHistoryIndex];
            inputField.MoveTextEnd(false);
        }

        void UpdateLog(string[] newLog) =>
            consoleText.text = (newLog == null ? "" : string.Join("\n", newLog));

        /// <summary>Logs a string to the ModConsole and output_log.txt.</summary>
        /// <param name="text">Message to log.</param>
        public static void Log(string text)
        {
            // Add it to the log.
            controller.AppendLogLine(text);
            // Also write it to the output_log.txt (using Console.WriteLine instead of Debug.Log to avoid a stacktrace)
            Console.WriteLine($"MODLOADER: {OutputString(text)}");
        }

        /// <summary>Logs anything to the ModConsole and output_log.txt.</summary>
        /// <param name="obj">object to log.</param>
        public static void Log(object obj)
        {
            // Add it to the log.
            controller.AppendLogLine(obj.ToString());
            // Also write it to the output_log.txt (using Console.WriteLine instead of Debug.Log to avoid a stacktrace)
            Console.WriteLine($"MODLOADER: {obj}");
        }

        /// <summary>Logs a list (and optionally its elements) to the ModConsole and output_log.txt</summary>
        /// <param name="list">List to print.</param>
        /// <param name="printAllElements">(Optional) Should it log all elements of the list/array or should it only log the list/array itself. (default: true)</param>
        public static void Log(IList list, bool printAllElements = true)
        {
            // Check if it should print the elements or the list itself.
            if (printAllElements)
            {
                Log(list.ToString());
                for (int i = 0; i < list.Count; i++) Log(list[i]);
            }
            else Log(list.ToString());
        }

        /// <summary>Logs a string as an error to the ModConsole and output_log.txt. (Depending on user settings, this might auto-open the console)</summary>
        /// <param name="text">Message to error log.</param>
        public static void LogError(string text)
        {
            // Check if Console Auto open is set to open for Errors.
            if (consoleInstance.settings.ConsoleAutoOpen == 1 || consoleInstance.settings.ConsoleAutoOpen == 3) 
                ToggleConsole(true);

            // Add it to the log.
            controller.AppendLogLine($"<color=red><b>Error:</b> {text}</color>");
            // Also write it to the output_log.txt (using Console.WriteLine instead of Debug.Log to avoid a stacktrace)
            Debug.LogError($"MODLOADER ERROR: {OutputString(text)}");
        }

        /// <summary>Logs a string as a warning to the ModConsole and output_log.txt. (Depending on user settings, this might auto-open the console)</summary>
        /// <param name="text">Message to warning log.</param>
        public static void LogWarning(string text)
        {
            // Check if Console Auto open is set to open for Warnings.
            if (consoleInstance.settings.ConsoleAutoOpen >= 2) ToggleConsole(true);

            // Add it to the log.
            controller.AppendLogLine($"<color=yellow><b>Warning:</b> {text}</color>");
            // Also write it to the output_log.txt (using Console.WriteLine instead of Debug.Log to avoid a stacktrace)
            Console.WriteLine($"MODLOADER WARNING: {OutputString(text)}");
        }

        static string OutputString(string text) => Regex.Replace(text, "<.*?>", "");

        #region Obsolete Methods
        [Obsolete("Deprecated, use Log() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static void Print(string text) => Log(text);
        [Obsolete("Deprecated, use Log() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static void Print(object obj) => Log(obj);
        [Obsolete("Deprecated, use LogError() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static void Error(string text = "") => LogError(text);
        [Obsolete("Deprecated, use LogWarning() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static void Warning(string text) => LogWarning(text);
        #endregion
    }

    internal class ModConsoleResizer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
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
            heightSlider.suspendActions = true;
            widthSlider.suspendActions = true;
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
            heightSlider.suspendActions = false;
            widthSlider.suspendActions = false;

            modConsole.settings.SaveINISettings();
        }

        public void OnPointerEnter(PointerEventData eventData) => Cursor.SetCursor(cursor, new Vector2(24, 24), CursorMode.Auto);
        public void OnPointerExit(PointerEventData eventData) => Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        public void UpdateSize()
        {
            console.sizeDelta = new Vector2(widthSlider.Value, heightSlider.Value);
        }
    }

    internal class ModConsoleSliderMaxSize : MonoBehaviour
    {
        public Slider slider;
        public bool height = true;

        void Awake()
        {
            slider.maxValue = height ? 720 : 1280;
        }
    }
}
