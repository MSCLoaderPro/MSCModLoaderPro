using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MSCLoader
{
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
            element.localScale = normalScale;
        }
    }

    public class ToggleActive : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        public void Toggle()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(!rectTransforms[i].gameObject.activeSelf);
        }
    }

    public class ModMenuDetection : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        void OnEnable()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(false);
        }
    }

    public class TextBoxHider : MonoBehaviour
    {
        public GameObject textObject;
        public Text text;

        void OnEnable()
        {
            textObject.SetActive(!string.IsNullOrEmpty(text.text));
        }
    }

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

    public class UILoadHandler : MonoBehaviour
    {
        public GameObject modMenu;
        public List<GameObject> extra = new List<GameObject>();

        public void SceneLoad()
        {
            modMenu.SetActive(false);
            for (int i = 0; i < extra.Count; i++)
                extra[i].SetActive(false);
        }
    }

    public class UIMainMenuLoad : MonoBehaviour
    {
        public UILoadHandler loadHandler;
        void OnEnable()
        {
            loadHandler.SceneLoad();
        }
    }

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
}