using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary>ModPrompt Component for a prompt window.</summary>
    public class ModPrompt : MonoBehaviour
    {
        /// <summary>Title header GameObject.</summary>
        public GameObject titleHeader;
        /// <summary>Title UI Text.</summary>
        public Text titleText;
        /// <summary>Title UI Text Shadow.</summary>
        public Shadow titleShadow;

        /// <summary>Message header GameObject.</summary>
        public GameObject textHeader;
        /// <summary>Message UI Text.</summary>
        public Text textText;
        /// <summary>Message UI Text Shadow.</summary>
        public Shadow textShadow;

        /// <summary>Button parent Transform.</summary>
        public Transform buttonParent;
        /// <summary>Button Prefab GameObject.</summary>
        public GameObject buttonPrefab;
        /// <summary>Button list for all added ModPromptButtons.</summary>
        public List<ModPromptButton> buttons = new List<ModPromptButton>();

        bool destroyOnDisable = true;
        /// <summary>Should the ModPrompt be destroyed after being disabled?</summary>
        public bool DestroyOnDisable { get => destroyOnDisable; set => destroyOnDisable = value; }

        protected bool DontCheckForMissingButtons;


        /// <summary>UnityAction that executes when the ModPrompt closes.</summary>
        public UnityAction OnCloseAction;

        /// <summary>Title for the ModPrompt.</summary>
        public string Title
        {
            get => titleText.text; set
            {
                titleText.text = value;
                titleHeader.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
        /// <summary>Text for the ModPrompt.</summary>
        public string Text
        {
            get => textText.text; set
            {
                textText.text = value;
                textHeader.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
        /// <summary>Adds a button to the mod prompt with the specified text and action when clicked.</summary>
        /// <param name="buttonText">Text displayed on the button.</param>
        /// <param name="action">UnityAction to be executed when the button is clicked.</param>
        /// <returns>The added ModPromptButton</returns>
        public ModPromptButton AddButton(string buttonText, UnityAction action)
        {
            ModPromptButton button = Instantiate(buttonPrefab).GetComponent<ModPromptButton>();
            button.prompt = this;
            button.Text = buttonText;
            button.OnClick.AddListener(() => { action?.Invoke(); });

            button.transform.SetParent(buttonParent, false);
            buttons.Add(button);

            return button;
        }

        void OnDisable()
        {
            OnCloseAction?.Invoke();
            if (destroyOnDisable) Destroy(gameObject);
        }

        /// <summary>Show the ModPrompt</summary>
        void OnEnable()
        {
            // We are checking if the custom prompt has buttons.
            // If it doesn't, we're adding a dummy "OK" button.
            // This is to prevent modders from creating prompts that can't be closed.
            StartCoroutine(IsAnyButtonPresent());
        }

        IEnumerator IsAnyButtonPresent()
        {
            yield return null;
            if (buttons.Count == 0 && !DontCheckForMissingButtons)
            {
                AddButton("OK", null);
            }
        }

        // CREATION METHODS!

        internal static GameObject prompt;
        // This one creates a dummy button and returns ModPrompt. Not meant to be accessed publicly.
        private static ModPrompt NewPrompt()
        {
            Transform newPrompt = Instantiate(prompt).transform;
            newPrompt.SetParent(ModLoader.UICanvas.transform);
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
        
        /// <summary>
        /// Makes a prompt without buttons (for internal use only!)<br></br>
        /// </summary>
        /// <returns>Returns a ModPrompt component of the button.</returns>
        internal static ModPrompt CreateButtonlessPrompt()
        {
            ModPrompt modPrompt = NewPrompt();
            modPrompt.DontCheckForMissingButtons = true;
            return modPrompt;
        }
    }

    /// <summary>ModPromptButton Component for buttons added the prompts.</summary>
    public class ModPromptButton : MonoBehaviour
    {
        /// <summary>Parent ModPrompt</summary>
        public ModPrompt prompt;
        /// <summary>UI Button Component</summary>
        public Button button;

        /// <summary>Button UI Text</summary>
        public Text buttonText;
        /// <summary>Button UI Text Shadow</summary>
        public Shadow buttonShadow;

        /// <summary>Text displayed on the button.</summary>
        public string Text { get => buttonText.text; set => buttonText.text = value; }
        /// <summary>Eventholder for the button click</summary>
        public Button.ButtonClickedEvent OnClick { get => button.onClick; set => button.onClick = value; }

        /// <summary>When the button is clicked, disable the prompt.</summary>
        public void ClickDisable()
        {
            prompt.gameObject.SetActive(false);
        }
    }
}
