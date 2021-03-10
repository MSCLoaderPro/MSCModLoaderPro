using MSCLoader.Helper;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.PartMagnet
{
    [AddComponentMenu("Mod Loader Pro/Part Magnet")]
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class PartMagnet : MonoBehaviour
    {
        public enum AttachmentType { Static, Breakable }

        [Header("Part Magnet, created by Fredrik!"), Space(10)]
        public AttachmentType attachmentType;

        public Collider[] attachmentPoints = new Collider[1];

        [Space(10), Header("(OPTIONAL) Interaction text.")]
        public string attachText = "";
        public string detachText = "";

        [Space(10), Header("(OPTIONAL) Specific rigidbodies to attach breakable parts to, else it looks for the best match.")]
        public Rigidbody[] attachmentPointsRigidbody;

        [Space(10), Header("(OPTIONAL) Custom Attach- and Detach-sound.")]
        public AudioSource customAudioSource;
        public AudioClip customAssembleSound, customDisassembleSound;
        
        [Space(10), Header("(OPTIONAL) Breakable base break-force and -torque.")]
        public float baseBreakForce = 100;
        public float baseBreakTorque = 100;

        [Space(10), Header("(OPTIONAL) Event actions:")]
        public UnityEvent OnAttach = new UnityEvent();
        public UnityEvent OnDetach = new UnityEvent();

        [HideInInspector] public FixedJoint joint;
        [HideInInspector] public bool attached;
        [HideInInspector] public int attachmentPointIndex; 
        
        int wheelLayer; // 16
        bool inTrigger = false;
        bool mouseOver = false;
        HutongGames.PlayMaker.FsmBool guiAssemble, guiDisassemble;
        HutongGames.PlayMaker.FsmString guiInteraction;
        string untagged = "Untagged", part = "PART";

        GameObject raycastParent;
        HutongGames.PlayMaker.FsmGameObject raycastObject;

        GameObject boltDetectionParent;
        HutongGames.PlayMaker.FsmGameObject boltDetection;

        RaycastHit hitInfo;
        Camera playerCamera;
        Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);
        LayerMask partLayerMask;

        Coroutine triggerRoutine, attachedRoutine;

        void Awake()
        {
            if (Application.isEditor) { enabled = false; return; }

            wheelLayer = LayerMask.NameToLayer("Wheel");
            guiAssemble = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmBool>("GUIassemble");
            guiDisassemble = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmBool>("GUIdisassemble");
            guiInteraction = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmString>("GUIinteraction");

            raycastParent = ModHelper.GetGameObject("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
            raycastObject = raycastParent.GetPlayMakerFSM("PickUp").GetVariable<HutongGames.PlayMaker.FsmGameObject>("RaycastHitObject");

            boltDetectionParent = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/2Spanner/Pick").gameObject;
            boltDetection = boltDetectionParent.GetPlayMakerFSM("PickUp").GetVariable<HutongGames.PlayMaker.FsmGameObject>("PickedTool");

            hitInfo = new RaycastHit();
            playerCamera = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").GetComponent<Camera>();
            partLayerMask = 1 << LayerMask.NameToLayer("Parts");
        }
        void OnTriggerEnter(Collider other)
        {
            if (!attached && attachmentPoints.Contains(other) && gameObject.layer == wheelLayer)
            {
                if (triggerRoutine != null) StopCoroutine(triggerRoutine);
                triggerRoutine = StartCoroutine(InTrigger(other));
            }
        }

        IEnumerator InTrigger(Collider other)
        {
            inTrigger = true;
            while (inTrigger)
            {
                guiAssemble.Value = true;
                if (attachText != "") guiInteraction.Value = attachText;
                if (Input.GetMouseButtonDown(0))
                {
                    guiAssemble.Value = false;
                    if (attachText != "") guiInteraction.Value = "";

                    Attach(other);

                    break;
                }
                yield return null;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (attachmentPoints.Contains(other))
            {
                if (triggerRoutine != null) StopCoroutine(triggerRoutine);
                inTrigger = false;
                guiAssemble.Value = false;
                if (attachText != "") guiInteraction.Value = "";
            }
        }

        public void Attach(Collider attachmentPoint, bool playSound = true)
        {
            attached = true;
            inTrigger = false;

            attachmentPoint.enabled = false;
            attachmentPointIndex = Array.IndexOf(attachmentPoints, attachmentPoint);

            gameObject.tag = untagged;
            StartCoroutine(SetParent(attachmentPoint.transform));

            if (attachmentType == AttachmentType.Breakable)
            {
                joint = gameObject.AddComponent<FixedJoint>();

                if (attachmentPointsRigidbody.Length > attachmentPointIndex)
                    joint.connectedBody = attachmentPointsRigidbody[attachmentPointIndex];
                else
                {
                    joint.connectedBody = attachmentPoint.GetComponentInParent<Rigidbody>();

                    if (joint.connectedBody == null)
                    {
                        ModConsole.LogError($"PartMagnet: {gameObject.name} can't find suitable rigidbody to attach to, Detaching...\nYou may have to specify exactly which rigidbody should be used with the \"attachmentPointsRigidbody\" array.");
                        Detach();
                        return;
                    }
                }

                StartCoroutine(SetJointBreak());
            }
            else
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            }

            if (playSound)
            {
                if (customAudioSource) customAudioSource.PlayOneShot(customAssembleSound);
                else transform.PlaySound3D("CarBuilding", "assemble");
            }

            OnAttach.Invoke();

            if (attachedRoutine != null) StopCoroutine(attachedRoutine);
            attachedRoutine = StartCoroutine(PartAttached());

            ModConsole.Log($"PartMagnet: {gameObject.name} attached on attachment point: {attachmentPoint}.");
        }

        IEnumerator SetJointBreak()
        {
            yield return new WaitForSeconds(0.5f);

            if (attached && joint)
            {
                joint.breakForce = baseBreakForce;
                joint.breakTorque = baseBreakTorque;
            }
        }

        IEnumerator PartAttached()
        {
            yield return null;
            yield return null;

            while (attached)
            {
                if ((raycastParent.activeInHierarchy && raycastObject.Value == gameObject) ||
                    (boltDetectionParent.activeInHierarchy && boltDetection.Value == null && Physics.Raycast(playerCamera.ViewportPointToRay(viewportCenter), out hitInfo, 1f, partLayerMask.value) && hitInfo.collider.transform == transform))
                {
                    mouseOver = true;
                    guiDisassemble.Value = true;
                    if (detachText != "") guiInteraction.Value = detachText;

                    if (Input.GetMouseButtonDown(1))
                    {
                        Detach();
                        MouseOver();
                    }
                }
                else if (mouseOver) MouseOver();

                yield return null;
            }
        }

        public void Detach(bool playSound = true)
        {
            attached = false;

            if (attachedRoutine != null) StopCoroutine(attachedRoutine);
            if (mouseOver) MouseOver();

            if (joint) Destroy(joint);

            gameObject.tag = part;
            transform.parent = null;
            attachmentPoints[attachmentPointIndex].enabled = true;

            if (attachmentType == AttachmentType.Static)
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = false;
                gameObject.GetComponent<Rigidbody>().detectCollisions = true;
            }

            if (playSound)
            {
                if (customAudioSource) customAudioSource.PlayOneShot(customDisassembleSound);
                else transform.PlaySound3D("CarBuilding", "disassemble");
            }

            OnDetach.Invoke();

            ModConsole.Log($"PartMagnet: {gameObject.name} detached from attachment point: {attachmentPoints[attachmentPointIndex]}.");
        }

        void OnJointBreak(float breakForce)
        {
            Detach();
        }

        void MouseOver()
        {
            mouseOver = false;
            guiDisassemble.Value = false;
            if (detachText != "") guiInteraction.Value = "";
        }

        IEnumerator SetParent(Transform parent)
        {
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;

            yield return new WaitForEndOfFrame();

            while (transform.parent != parent)
            {
                transform.parent = parent;
                transform.localPosition = Vector3.zero;
                transform.localEulerAngles = Vector3.zero;

                yield return null;
            }
        }
    }
}
