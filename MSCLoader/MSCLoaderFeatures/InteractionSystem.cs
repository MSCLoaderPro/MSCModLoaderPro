using HutongGames.PlayMaker;
using System.Collections.Generic;
using UnityEngine;
using MSCLoader.Helper;
using static MSCLoader.Helper.PlayMakerHelper;
using static MSCLoader.Helper.ModHelper;
using UnityEngine.Events;

namespace MSCLoader.Features
{
    public class Interaction : MonoBehaviour
    {
        public InteractionSystem interactionSystem;

        public Collider interactionCollider;
        public Collider[] additionalColliders;

        public bool showUseIcon = false;
        public bool showAssembleIcon = false;
        public bool showDisassembleIcon = false;
        public bool showBuyIcon = false;
        public bool showDriveIcon = false;
        public bool showPassengerIcon = false;

        public string interactionText = "";
        public string subtitleText = "";

        public UnityEvent OnInteract = new UnityEvent();

        bool mouseOver = false;

        void Start()
        {
            if (Application.isEditor) { enabled = false; return; }
        }

        void Update()
        {
            if (interactionSystem.GetHit(interactionCollider) || interactionSystem.GetHit(additionalColliders))
            {
                mouseOver = true;

                if (showUseIcon) GUIUse = true;
                if (showAssembleIcon) GUIAssemble = true;
                if (showDisassembleIcon) GUIDisassemble = true;
                if (showBuyIcon) GUIBuy = true;
                if (showDriveIcon) GUIDrive = true;
                if (showPassengerIcon) GUIPassenger = true;
                if (interactionText != "") GUIInteraction = $" {interactionText} ";
                if (subtitleText != "") GUISubtitle = $" {subtitleText} ";

                OnInteract.Invoke();
            }
            else if (mouseOver)
            {
                mouseOver = false;

                if (showUseIcon) GUIUse = false;
                if (showAssembleIcon) GUIAssemble = false;
                if (showDisassembleIcon) GUIDisassemble = false;
                if (showBuyIcon) GUIBuy = false;
                if (showDriveIcon) GUIDrive = false;
                if (showPassengerIcon) GUIPassenger = false;
                if (interactionText != "" && interactionText == GUIInteraction) GUIInteraction = "";
                if (subtitleText != "" && subtitleText == GUISubtitle) GUISubtitle = "";
            }
        }
    }

    public class InteractionSystem : MonoBehaviour
    {
        public float rayDistance = 1.35f;
        public LayerMask layerMask = -1;

        public bool ignoreMenu = false;
        public bool useMousePosition = false;

        [HideInInspector]
        public RaycastHit hitInfo;
        [HideInInspector]
        public bool hasHit = false;

        Camera playerCamera;
        Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);

        FsmBool playerMenu;

        void Awake()
        {
            if (Application.isEditor) { enabled = false; return; }

            hitInfo = new RaycastHit();
            playerCamera = GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").GetComponent<Camera>();
            playerMenu = GetGlobalVariable<FsmBool>("PlayerInMenu");
        }

        void FixedUpdate()
        {
            hasHit = Physics.Raycast(useMousePosition ? playerCamera.ScreenPointToRay(Input.mousePosition) : playerCamera.ViewportPointToRay(viewportCenter), out hitInfo, rayDistance, layerMask.value);
        }

        public bool GetHit(Collider collider) => CheckHit() && hitInfo.collider == collider;
        public bool GetHit(IList<Collider> colliders)
        {
            if (!CheckHit()) return false;

            for (int i = 0; i < colliders.Count; i++)
                if (colliders[i] == hitInfo.collider) return true;

            return false;
        }

        bool CheckHit() => hasHit && (!playerMenu.Value || ignoreMenu);
    }
}
