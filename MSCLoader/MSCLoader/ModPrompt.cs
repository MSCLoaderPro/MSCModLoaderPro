using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MSCLoader
{
    public class ModPrompt : MonoBehaviour
    {
        public GameObject titleHeader;
        public Text titleText;
        public Shadow titleShadow;

        public GameObject textHeader;
        public Text textText;
        public Shadow textShadow;

        public Transform buttonParent;
        public GameObject buttonPrefab;
        public List<ModPromptButton> buttons = new List<ModPromptButton>();

        public bool destroyOnDisable = true;

        public string Title
        {
            get => titleText.text; set
            {
                titleText.text = value;
                titleHeader.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }
        public string Text
        {
            get => textText.text; set
            {
                textText.text = value;
                textHeader.gameObject.SetActive(!string.IsNullOrEmpty(value));
            }
        }

        public ModPromptButton AddButton(string buttonText, UnityAction action)
        {
            ModPromptButton button = Instantiate(buttonPrefab).GetComponent<ModPromptButton>();
            button.prompt = this;
            button.Text = buttonText;
            button.OnClick.AddListener(action);

            button.transform.SetParent(buttonParent, false);
            buttons.Add(button);

            return button;
        }

        void OnDisable()
        {
            if (destroyOnDisable) Destroy(gameObject);
        }
    }

    public class ModPromptButton : MonoBehaviour
    {
        public ModPrompt prompt;
        public Button button;

        public Text buttonText;
        public Shadow buttonShadow;

        public string Text { get => buttonText.text; set => buttonText.text = value; }
        public Button.ButtonClickedEvent OnClick { get => button.onClick; set => button.onClick = value; }

        public void ClickDisable()
        {
            prompt.gameObject.SetActive(false);
        }
    }
}
