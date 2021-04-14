using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable CS1591, IDE1006
namespace MSCLoader
{
    internal class SwitchToggleGraphic : MonoBehaviour
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
            if (element) element.localScale = normalScale;
        }
    }
    internal class ToggleActive : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        public void Toggle()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(!rectTransforms[i].gameObject.activeSelf);
        }
    }
    internal class ModMenuDetection : MonoBehaviour
    {
        public RectTransform[] rectTransforms;

        void OnEnable()
        {
            for (int i = 0; i < rectTransforms.Length; i++)
                rectTransforms[i].gameObject.SetActive(false);
        }
    }
    internal class TextBoxHider : MonoBehaviour
    {
        public GameObject textObject;
        public Text text;

        void OnEnable()
        {
            textObject.SetActive(!string.IsNullOrEmpty(text.text));
        }
    }
    internal class UIPositioning : MonoBehaviour
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
        [SerializeField] internal ModContainer modContainer;
        [SerializeField] internal ModLoaderSettings modLoaderSettings;

        [SerializeField] internal GameObject modMenu;
        [SerializeField] internal GameObject modList;
        [SerializeField] internal GameObject modSettings;
        [SerializeField] internal GameObject modMenuButton;
        [SerializeField] internal GameObject menuLabel;

        public List<GameObject> extra = new List<GameObject>();

        [SerializeField] internal bool lockEnable = false;

        public void Disable()
        {
            modMenu.SetActive(false);
            modList.SetActive(false);
            modMenuButton.SetActive(false);
            modSettings.SetActive(false);

            modLoaderSettings.SetSettingsOpen(false, true);
            foreach (ModListElement mod in modContainer.modListDictionary.Values) 
                mod.SetSettingsOpen(false, true);

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
    internal class UIMainMenuLoad : MonoBehaviour
    {
        public UILoadHandler loadHandler;
        void OnEnable()
        {
            loadHandler.lockEnable = true;
            loadHandler.menuLabel.SetActive(false);
            loadHandler.Disable();
        }
    }
    internal class UIModMenuHandler : MonoBehaviour
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
            modMenu?.SetActive(false);
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
    internal class UISubMenuHandler : MonoBehaviour
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
    internal class UIMenuNewGameHandler : MonoBehaviour
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
    internal class StartDisable : MonoBehaviour
    {
        public GameObject[] objectToDisable;

        void Awake()
        {
            for (int i = 0; i < objectToDisable.Length; i++)
                objectToDisable[i].SetActive(false);

            gameObject.SetActive(false);
        }
    }

    internal class UITooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string toolTipText;
        public static GameObject toolTipPrefab;

        Transform toolTip;
        WaitForSeconds wait = new WaitForSeconds(0.75f);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ModLoader.modLoaderSettings.ShowTooltips && toolTipPrefab != null) StartCoroutine(ShowDelay());
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            if (toolTip != null) Destroy(toolTip.gameObject);
        }

        IEnumerator ShowDelay()
        {
            yield return wait;

            toolTip = Instantiate(toolTipPrefab).transform;
            toolTip.SetParent(ModLoader.UICanvas);
            toolTip.localScale = Vector3.one;
            toolTip.GetComponentInChildren<Text>().text = toolTipText;

            while(true)
            {
                toolTip.position = Input.mousePosition;
                yield return null;
            }
        }

    }
}
