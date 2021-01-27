using System;
using UnityEngine;
using UnityEngine.UI;
/*
namespace MSCLoader
{
    public class ConsoleView : MonoBehaviour
    {
        public ConsoleController controller;
        public GameObject viewContainer; //Container for console view, should be a child of this GameObject
        public Text logTextArea;
        public InputField inputField;
        bool wasFocused;
        int commands, pos;

        void Start()
        {
            if (controller != null) controller.LogChanged += onLogChanged;

            updateLogStr(controller.log);
        }

        ~ConsoleView()
        {
            controller.LogChanged -= onLogChanged;
        }

        public void toggleVisibility()
        {
            if (viewContainer.activeSelf) viewContainer.transform.GetChild(4).gameObject.GetComponent<ConsoleUIResizer>().SaveConsoleSize();

            setVisibility(!viewContainer.activeSelf);
            inputField.text = string.Empty;

            if (viewContainer.activeSelf && (bool)ModConsole.typing.GetValue())
            {
                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        public void setVisibility(bool visible) => viewContainer.SetActive(visible);

        void onLogChanged(string[] newLog) => updateLogStr(newLog);

        void updateLogStr(string[] newLog) => 
            logTextArea.text = newLog == null ? string.Empty : string.Join(Environment.NewLine, newLog);

        // Event that should be called by anything wanting to submit the current input to the console.
        public void runCommand()
        {
            controller.RunCommandString(inputField.text);
            inputField.text = string.Empty;

            inputField.ActivateInputField();
            inputField.Select();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && inputField.text.Length > 0)
                runCommand();

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
}*/