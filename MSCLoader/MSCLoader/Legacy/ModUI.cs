using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// GNU GPL 3.0
namespace MSCLoader
{
    public class ModUI
    {
        internal static GameObject canvasGO;

        public static GameObject GetCanvas() => canvasGO;
    
        public static GameObject messageBox;

        public static void ShowMessage(string message, string title = "Message")
        {
            //SHOW PROMPT
        }

        public static void ShowYesNoMessage(string message, Action ifYes) => ShowYesNoMessage(message, "Message", ifYes);

        public static void ShowYesNoMessage(string message, string title, Action ifYes)
        {
            // SHOW PROMPT
        }
    }
}