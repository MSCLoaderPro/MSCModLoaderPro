using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable CS1591, IDE1006
namespace MSCLoader
{
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
