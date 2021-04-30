using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006
namespace MSCLoader
{
    public class ModUI
    {
        [Obsolete("Please use ModLoader.UICanvas instead.")]
        internal static GameObject canvas { get => ModLoader.UICanvas.gameObject; set => ModLoader.UICanvas = value.transform; }

        [Obsolete("Please use ModLoader.UICanvas instead.")]
        public static GameObject GetCanvas() => ModLoader.UICanvas.gameObject;

        [Obsolete("Please use ModPrompt.CreatePrompt() instead.")]
        public static void ShowMessage(string message, string title = "MESSAGE") => ModPrompt.CreatePrompt(message, title);

        [Obsolete("Please use ModPrompt.CreateYesNoPrompt() instead.")]
        public static void ShowYesNoMessage(string message, Action ifYes) => ShowYesNoMessage(message, "MESSAGE", ifYes);

        [Obsolete("Please use ModPrompt.CreateYesNoPrompt() instead.")]
        public static void ShowYesNoMessage(string message, string title, Action ifYes) => ModPrompt.CreateYesNoPrompt(message, title, () => { ifYes(); });

        [Obsolete()]
        public static GameObject messageBox { get => ModPrompt.prompt; set => ModPrompt.prompt = value; }
    }
}
