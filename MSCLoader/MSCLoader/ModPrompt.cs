using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

        /// <summary>Should the ModPrompt be destroyed after being disabled?</summary>
        public bool destroyOnDisable = true;

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
            if (buttons.Count == 0)
            {
                AddButton("OK", null);
            }
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
