using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#pragma warning disable CS1591, IDE1006
namespace MSCLoader
{
    /// <summary>Contains methods and variables suitable for controlling UI.</summary>
    public class ModUI
    {
        internal static GameObject canvas;

        ///<summary>Get the mod loader canvas GameObject.</summary>
        ///<returns>Returns the mod loader canvas GameObject.</returns>
        public static GameObject GetCanvas() => canvas;

        internal static GameObject prompt;

        /// <summary>Yellow Color that MSC uses.</summary>
        public static Color32 MSCYellow = new Color32(255, 255, 0, 255);
        /// <summary>Wine Red Color that MSC uses.</summary>
        public static Color32 MSCRed = new Color32(101, 34, 18, 255);
        /// <summary>Rose Color that MSC uses.</summary>
        public static Color32 MSCRose = new Color32(199, 152, 129, 255);
        /// <summary>Red Color for disabled mods.</summary>
        public static Color32 ModDisabledRed = new Color32(215, 0, 0, 255);

        // This one creates a dummy button and returns ModPrompt. Not meant to be accessed publicly.
        private static ModPrompt NewPrompt()
        {
            Transform newPrompt = Object.Instantiate(prompt).transform;
            newPrompt.SetParent(GetCanvas().transform);
            newPrompt.localPosition = Vector3.zero;

            return newPrompt.GetComponent<ModPrompt>();
        }

        /// <summary> Creates a prompt with "OK" button. </summary>
        /// <param name="message">A message that will appear in the prompt.</param>
        /// <param name="title">Title of the prompt.</param>
        /// <param name="onPromptClose">(Optional) Action that will happen after the window gets closed - regardless of player's choice.</param>
        /// <returns>Returns a ModPrompt component of the button. Can be</returns>
        public static ModPrompt CreatePrompt(string message, string title = "MESSAGE", UnityAction onPromptClose = null)
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.Text = message;
            modPrompt.Title = title;
            modPrompt.AddButton("OK", null);
            modPrompt.OnCloseAction = onPromptClose;

            return modPrompt;
        }
        /// <summary> Creates a prompt with "Yes" and "No" buttons. </summary>
        /// <param name="message">A message that will appear in the prompt.</param>
        /// <param name="title">Title of the prompt.</param>
        /// <param name="onYes">Action that will happen after player clicks Yes button.</param>
        /// <param name="onNo">(Optional) Action that will happen after player clicks No button.</param>
        /// <param name="onPromptClose">(Optional) Action that will happen after the window gets closed - regardless of player's choice.</param>
        /// <returns>Returns a ModPrompt component of the button.</returns>
        public static ModPrompt CreateYesNoPrompt(string message, string title, UnityAction onYes, UnityAction onNo = null, UnityAction onPromptClose = null)
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.Text = message;
            modPrompt.Title = title;
            modPrompt.AddButton("YES", onYes);
            modPrompt.AddButton("NO", onNo);
            modPrompt.OnCloseAction = onPromptClose;

            return modPrompt;
        }
        /// <summary>Creates a prompt with "Retry" and "Cancel" buttons.</summary>
        /// <param name="message">A message that will appear in the prompt.</param>
        /// <param name="title">Title of the prompt.</param>
        /// <param name="onRetry">Action that will happen after player clicks Retry button.</param>
        /// <param name="onCancel">(Optional) Action that will happen after player clicks Cancel button.</param>
        /// <param name="onPromptClose">(Optional) Action that will happen after the window gets closed - regardless of player's choice.</param>
        /// <returns>Returns a ModPrompt component of the button.</returns>
        public static ModPrompt CreateRetryCancelPrompt(string message, string title, UnityAction onRetry, UnityAction onCancel = null, UnityAction onPromptClose = null)
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.Text = message;
            modPrompt.Title = title;
            modPrompt.AddButton("RETRY", onRetry);
            modPrompt.AddButton("CANCEL", onCancel);
            modPrompt.OnCloseAction = onPromptClose;

            return modPrompt;
        }
        /// <summary>Creates a prompt with "Continue" and "Abort" buttons</summary>
        /// <param name="message">A message that will appear in the prompt.</param>
        /// <param name="title">Title of the prompt.</param>
        /// <param name="onContinue">Action that will happen after player clicks Continue button.</param>
        /// <param name="onAbort">(Optional) Action that will happen after player clicks Abort button.</param>
        /// <param name="onPromptClose">(Optional) Action that will happen after the window gets closed - regardless of player's choice.</param>
        /// <returns>Returns a ModPrompt component of the button.</returns>
        public static ModPrompt CreateContinueAbortPrompt(string message, string title, UnityAction onContinue, UnityAction onAbort = null, UnityAction onPromptClose = null)
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.Text = message;
            modPrompt.Title = title;
            modPrompt.AddButton("CONTINUE", onContinue);
            modPrompt.AddButton("ABORT", onAbort);
            modPrompt.OnCloseAction = onPromptClose;

            return modPrompt;
        }

        /// <summary>
        /// Creates a prompt that can be fully customized. You can add any buttons you like.<br></br>
        /// Custom prompts have to be showed manually using <b>ModPrompt.Show()</b>!
        /// </summary>
        /// <returns>Returns a ModPrompt component of the button.</returns>
        public static ModPrompt CreateCustomPrompt()
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.gameObject.SetActive(false); // Custom prompts have to be showed manually using ModPrompt.Show().

            return NewPrompt();
        }

        [Obsolete("Please use CreatePrompt() instead.")]
        public static void ShowMessage(string message, string title = "MESSAGE") => CreatePrompt(message, title);

        [Obsolete("Please use CreateYesNoPrompt() instead.")]
        public static void ShowYesNoMessage(string message, Action ifYes) => ShowYesNoMessage(message, "MESSAGE", ifYes);

        [Obsolete("Please use CreateYesNoPrompt() instead.")]
        public static void ShowYesNoMessage(string message, string title, Action ifYes) => CreateYesNoPrompt(message, title, () => { ifYes(); });

        [Obsolete()]
        public static GameObject messageBox { get => prompt; set => prompt = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SwitchToggleGraphic : MonoBehaviour
    {
        public Toggle toggle;
        public Image background;

        public void Start() => ChangeBackground();
        public void ChangeBackground()
        {
            background.enabled = !toggle.isOn;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ResizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public RectTransform element;
        public Vector3 hoverScale = new Vector3(0.9f, 0.9f, 0.9f);
        public Vector3 normalScale = Vector3.one;

        public void OnPointerEnter(PointerEventData eventData)
        {
            normalScale = element.localScale;
            element.localScale = hoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            element.localScale = normalScale;
        }

        void OnDisable()
        {
            if (element) element.localScale = normalScale;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ToggleActive : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        public void Toggle()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(!rectTransforms[i].gameObject.activeSelf);
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ModMenuDetection : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        void OnEnable()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(false);
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TextBoxHider : MonoBehaviour
    {
        public GameObject textObject;
        public Text text;

        void OnEnable()
        {
            textObject.SetActive(!string.IsNullOrEmpty(text.text));
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UIPositioning : MonoBehaviour
    {
        public RectTransform rectTransform;
        public Vector3 menuPosition, gamePosition;

        void OnEnable()
        {
            switch (Application.loadedLevelName)
            {
                case "MainMenu":
                    rectTransform.localPosition = menuPosition;
                    break;
                case "GAME":
                    rectTransform.localPosition = gamePosition;
                    break;
            }
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UILoadHandler : MonoBehaviour
    {
        public ModContainer modContainer;
        public ModLoaderSettings modLoaderSettings;
        public GameObject modMenu, modList, modSettings, modMenuButton, menuLabel;
        public List<GameObject> extra = new List<GameObject>();

        public bool lockEnable = false;

        public void Disable()
        {
            modMenu.SetActive(false);
            modList.SetActive(false);
            modMenuButton.SetActive(false);
            modSettings.SetActive(false);

            foreach (ModListElement mod in modContainer.modListDictionary.Values) mod.ToggleSettingsOff();
            modLoaderSettings.ToggleMenuOff();

            for (int i = 0; i < extra.Count; i++)
                extra[i].SetActive(false);
        }

        public void EnableModMenu()
        {
            if (!lockEnable)
            {
                modMenu.SetActive(true);
                modMenuButton.SetActive(true);
            }
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UIMainMenuLoad : MonoBehaviour
    {
        public UILoadHandler loadHandler;
        void OnEnable()
        {
            loadHandler.lockEnable = true;
            loadHandler.menuLabel.SetActive(false);
            loadHandler.Disable();
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UIModMenuHandler : MonoBehaviour
    {
        public GameObject modMenu, modList, modSettings, menuButton, gameButton;
        GameObject graphics, carControls, playerControls;

        public void Setup()
        {
            Transform systems = GameObject.Find("Systems").transform;
            transform.SetParent(systems.Find("OptionsMenu"));
            gameObject.SetActive(true);

            menuButton.SetActive(false);
            gameButton.SetActive(true);

            graphics = systems.Find("OptionsMenu/Graphics").gameObject;
            graphics.AddComponent<UISubMenuHandler>().menuHandler = this;

            carControls = systems.Find("OptionsMenu/CarControls").gameObject;
            carControls.AddComponent<UISubMenuHandler>().menuHandler = this;

            playerControls = systems.Find("OptionsMenu/PlayerControls").gameObject;
            playerControls.AddComponent<UISubMenuHandler>().menuHandler = this;
        }

        public void OnEnable()
        {
            modMenu.SetActive(true);
        }

        public void OnDisable()
        {
            modMenu.SetActive(false);
        }

        public void DisableDefaultMenus()
        {
            graphics.SetActive(false);
            carControls.SetActive(false);
            playerControls.SetActive(false);
        }

        public void DisableModMenus()
        {
            modList.SetActive(false);
            modSettings.SetActive(false);
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UISubMenuHandler : MonoBehaviour
    {
        public bool modMenu = false;
        public UIModMenuHandler menuHandler;

        void OnEnable()
        {
            if (Application.loadedLevel != 1)
            {
                if (modMenu)
                    menuHandler.DisableDefaultMenus();
                else
                    menuHandler.DisableModMenus();
            }
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UIMenuNewGameHandler : MonoBehaviour
    {
        public UILoadHandler loadHandler;
        void OnEnable()
        {
            loadHandler.Disable();
        }
        void OnDisable()
        {
            loadHandler.EnableModMenu();
            loadHandler.lockEnable = false;
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class StartDisable : MonoBehaviour
    {
        public GameObject[] objectToDisable;

        void Start()
        {
            for (int i = 0; i < objectToDisable.Length; i++)
                objectToDisable[i].SetActive(false);

            gameObject.SetActive(false);
        }
    }
}
