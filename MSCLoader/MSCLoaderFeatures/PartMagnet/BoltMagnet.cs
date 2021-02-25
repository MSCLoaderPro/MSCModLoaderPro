using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.PartMagnet
{
    public class BoltMagnetSaveData
    {
        public bool Attached = false;
        public int attachIndex;
        public int[] boltTightness;
    }

    [AddComponentMenu("Mod Loader Pro/Bolt")]
    [RequireComponent(typeof(Collider), typeof(Renderer))]
    public class Bolt : MonoBehaviour
    {
        /*
            Tool sizes:

            0.5f = 5 mm
            0.6f = 6 mm
            0.7f = 7 mm
            0.8f = 8 mm
            0.9f = 9 mm
            1f = 10 mm
            1.1f = 11 mm
            1.2f = 12 mm
            1.3f = 13 mm
            1.4f = 14 mm
            1.5f = 15 mm

            0.55f = Sparkplug Wrench
            0.65f = Screwdriver
        */
        public enum BoltSizes
        {
            Bolt5mm = 5,
            Bolt6mm = 6,
            Bolt7mm = 7,
            Bolt8mm = 8,
            Bolt9mm = 9,
            Bolt10mm = 10,
            Bolt11mm = 11,
            Bolt12mm = 12,
            Bolt13mm = 13,
            Bolt14mm = 14,
            Bolt15mm = 15,
            SparkPlug = 55,
            Screw = 65
        }

        [Header("Bolt Magnet, created by Fredrik!"), Space(10)]
        public BoltMagnet boltMagnet;
        [SerializeField] BoltSizes boltSize = BoltSizes.Bolt5mm;

        public BoltSizes BoltSize
        {
            get => boltSize; set
            {
                boltSize = value;
                size = (int)value / ((int)value < 16 ? 10 : 100);
            }
        }
        [Space(10)]
        public int tightness;
        public int minTightness;
        public int maxTightness;

        [Space(10)]
        public Vector3 tightnessPositionDelta, tightnessRotationDelta;
        [HideInInspector] public Vector3 tightnessOriginalPosition, tightnessOriginalRotation;

        public bool allowRatchetWrench = true;

        [Header("(OPTIONAL) Custom AudioSource for screwing sound:")]
        public AudioSource customScrewSound;
        [Header("(OPTIONAL) Use if the BoltMagnet is set to \"Breakable\":")]
        public float jointBreakForceDelta;
        public float jointBreakTorqueDelta;

        [HideInInspector] public float size = 0f;
        Renderer renderer;
        Material normalMaterial, highlightMaterial;

        HutongGames.PlayMaker.FsmFloat toolWrenchSize;
        GameObject boltDetectionParent;
        HutongGames.PlayMaker.FsmGameObject boltDetection;
        GameObject ratchet;
        HutongGames.PlayMaker.FsmBool ratchetMode; // true == screw IN, false == screw OUT

        static Material activeBolt;
        float boltDelay = 0f;
        bool boltOver = false; 
        string mouseWheel = "Mouse ScrollWheel";

        void Awake()
        {
            if (Application.isEditor) { enabled = false; return; }

            renderer = GetComponent<Renderer>();
            normalMaterial = renderer.sharedMaterial;
            highlightMaterial = Instantiate(activeBolt ?? (activeBolt = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(material => material.name == "activebolt")));
            
            size = (float)boltSize / ((int)boltSize < 16 ? 10f : 100f);
            gameObject.layer = LayerMask.NameToLayer("Bolts"); // 12

            tightnessOriginalPosition = transform.localPosition;
            tightnessOriginalRotation = transform.localEulerAngles;

            // SET UP BOLT DETECTION
            toolWrenchSize = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmFloat>("ToolWrenchSize");

            boltDetectionParent = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/2Spanner/Pick").gameObject;
            boltDetection = boltDetectionParent.GetPlayMakerFSM("PickUp").GetVariable<HutongGames.PlayMaker.FsmGameObject>("PickedTool");

            ratchet = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/2Spanner/Pivot/Ratchet").gameObject;
            ratchetMode = ratchet.GetPlayMakerFSM("Switch").GetVariable<HutongGames.PlayMaker.FsmBool>("Switch");
        }
        void Start() => OnEnable();

        void OnEnable()
        {
            transform.localPosition = tightnessOriginalPosition + (tightnessPositionDelta * tightness);
            transform.localEulerAngles = tightnessOriginalRotation + (tightnessRotationDelta * tightness);
            if (boltMagnet.joint) boltMagnet.UpdateJointBreakValues();
        }

        public void Reset()
        {
            tightness = minTightness;
            transform.localPosition = tightnessOriginalPosition + (tightnessPositionDelta * tightness);
            transform.localEulerAngles = tightnessOriginalRotation + (tightnessRotationDelta * tightness);
        }

        void Update()
        {
            if (boltDetectionParent.activeInHierarchy && boltMagnet.attached)
            {
                if (toolWrenchSize.Value == size && boltDetection.Value == gameObject)
                {
                    boltOver = true;
                    renderer.sharedMaterial = highlightMaterial;
                    if (boltDelay <= 0f)
                    {
                        if (Input.GetAxis(mouseWheel) > 0f) // Scroll Up
                        {
                            // If Ratchet is active and switch is in IN position;
                            if (tightness < maxTightness && (!ratchet.activeSelf || (allowRatchetWrench && ratchet.activeSelf && ratchetMode.Value)))
                                BoltInOut(+1);
                            // If Ratchet is active and switch is in OUT position;
                            else if (tightness > minTightness && allowRatchetWrench && ratchet.activeSelf && !ratchetMode.Value)
                                BoltInOut(-1);

                        }
                        else if (Input.GetAxis(mouseWheel) < 0f) // Scroll Down
                        {
                            // If Ratchet is active and switch is in OUT position;
                            if (tightness > minTightness && (!ratchet.activeSelf || (allowRatchetWrench && ratchet.activeSelf && !ratchetMode.Value)))
                                BoltInOut(-1);
                            // If Ratchet is active and switch is in IN position;
                            else if (tightness < maxTightness && allowRatchetWrench && ratchet.activeSelf && ratchetMode.Value)
                                BoltInOut(+1);
                        }
                    }
                }
                else if (boltOver) 
                    BoltOver();

                boltDelay -= Time.deltaTime;
            }
            else if (boltOver) 
                BoltOver();
        }

        public void BoltOver()
        {
            boltOver = false;
            renderer.sharedMaterial = normalMaterial;
        }

        void BoltInOut(int direction)
        {
            tightness += direction;
            transform.localPosition = tightnessOriginalPosition + (tightnessPositionDelta * tightness);
            transform.localEulerAngles = tightnessOriginalRotation + (tightnessRotationDelta * tightness);

            if (boltMagnet.joint) boltMagnet.UpdateJointBreakValues();

            if (customScrewSound != null) customScrewSound.Play();
            else transform.PlaySound3D("CarBuilding", "bolt_screw");
            
            boltDelay = ratchet.activeSelf ? 0.2f : 0.5f;
        }
    }
    
    [AddComponentMenu("Mod Loader Pro/Bolt Magnet")]
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class BoltMagnet : MonoBehaviour
    {
        public enum AttachmentType { Static, Breakable }

        [Header("Bolt Magnet, created by Fredrik!"), Space(10)]
        public AttachmentType attachmentType;

        public Collider[] attachmentPoints = new Collider[1];

        public GameObject boltParent;
        public Bolt[] bolts = new Bolt[1];

        [Space(10), Header("(OPTIONAL) Specific rigidbodies to attach breakable parts to, else it looks for the best match.")]
        public Rigidbody[] attachmentPointsRigidbody;

        [Space(10), Header("(OPTIONAL) Custom Attach- and Detach-sound.")]
        public AudioSource customAudioSource;
        public AudioClip customAssembleSound, customDisassembleSound;

        [Space(10), Header("(OPTIONAL) Breakable base break-force and -torque.")]
        public float baseBreakForce = 100;
        public float baseBreakTorque = 100;

        [Space(10), Header("(OPTIONAL) Event actions:")]
        public BoltMagnetOnAttach OnAttach = new BoltMagnetOnAttach();
        public BoltMagnetOnDetach OnDetach = new BoltMagnetOnDetach();

        [HideInInspector] public FixedJoint joint;
        [HideInInspector] public bool attached;
        [HideInInspector] public int attachmentPointIndex;

        int wheelLayer; // 16
        bool inTrigger = false;
        bool mouseOver = false;
        HutongGames.PlayMaker.FsmBool guiAssemble, guiDisassemble;
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

            raycastParent = ModHelper.GetGameObject("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/1Hand_Assemble/Hand");
            raycastObject = raycastParent.GetPlayMakerFSM("PickUp").GetVariable<HutongGames.PlayMaker.FsmGameObject>("RaycastHitObject");

            boltDetectionParent = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/2Spanner/Pick").gameObject;
            boltDetection = boltDetectionParent.GetPlayMakerFSM("PickUp").GetVariable<HutongGames.PlayMaker.FsmGameObject>("PickedTool");

            hitInfo = new RaycastHit();
            playerCamera = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/FPSCamera").GetComponent<Camera>();
            partLayerMask = 1 << LayerMask.NameToLayer("Parts");
        }

        public void Setup(bool attached, int attachmentIndex, int[] boltTightness)
        {
            if (attached) Attach(attachmentPoints[attachmentIndex]);
            for (int i = 0; i < bolts.Length; i++) bolts[i].tightness = boltTightness[i];
        }

        public BoltMagnetSaveData Save()
        {
            return new BoltMagnetSaveData
            {
                Attached = attached,
                attachIndex = attachmentPointIndex,
                boltTightness = bolts.Select(x => x.tightness).ToArray()
            };
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
                if (Input.GetMouseButtonDown(0))
                {
                    guiAssemble.Value = false;

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
            }
        }

        void Attach(Collider attachmentPoint, bool playSound = true)
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
                joint.breakForce = baseBreakForce;
                joint.breakTorque = baseBreakTorque;

                if (attachmentPointsRigidbody.Length > attachmentPointIndex) 
                    joint.connectedBody = attachmentPointsRigidbody[attachmentPointIndex];
                else
                {
                    joint.connectedBody = attachmentPoint.GetComponent<Rigidbody>();
                    if (joint.connectedBody == null) joint.connectedBody = attachmentPoint.transform.root.GetComponent<Rigidbody>();

                    if (joint.connectedBody == null)
                    {
                        ModConsole.LogError($"BoltMagnet: {gameObject.name} can't find suitable rigidbody to attach to, Detaching...\nYou may have to specify exactly which rigidbody should be used with the \"attachmentPointsRigidbody\" array.");
                        Detach();
                    }
                }
            }
            else
            {
                gameObject.GetComponent<Rigidbody>().isKinematic = true;
                gameObject.GetComponent<Rigidbody>().detectCollisions = false;
            }

            boltParent.SetActive(true);
            for (int i = 0; i < bolts.Length; i++) bolts[i].tightness = bolts[i].minTightness;

            if (playSound) 
            {
                if (customAudioSource) customAudioSource.PlayOneShot(customAssembleSound);
                else transform.PlaySound3D("CarBuilding", "assemble");
            }

            OnAttach.Invoke(attachmentPointIndex);

            if (attachedRoutine != null) StopCoroutine(attachedRoutine);
            attachedRoutine = StartCoroutine(PartAttached());
        }
        
        IEnumerator PartAttached()
        {
            yield return null;
            yield return null;
            while (attached)
            {
                if (((raycastParent.activeInHierarchy && raycastObject.Value == gameObject) ||
                    (boltDetectionParent.activeInHierarchy && boltDetection.Value == null && Physics.Raycast(playerCamera.ViewportPointToRay(viewportCenter), out hitInfo, 1f, partLayerMask) && hitInfo.transform == transform))
                    && bolts.All(tempBolt => tempBolt.tightness <= tempBolt.minTightness))
                {
                    mouseOver = true;
                    guiDisassemble.Value = true;

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

        void Detach(bool playSound = true)
        {
            attached = false;

            if (attachedRoutine != null) StopCoroutine(attachedRoutine);
            if (mouseOver) MouseOver();

            if (joint) Destroy(joint);

            gameObject.tag = part;
            transform.parent = null;
            attachmentPoints[attachmentPointIndex].enabled = true;

            boltParent.SetActive(false);
            for (int i = 0; i < bolts.Length; i++) bolts[i].Reset();

            if (playSound)
            {
                if (customAudioSource) customAudioSource.PlayOneShot(customAssembleSound);
                else transform.PlaySound3D("CarBuilding", "disassemble");
            }

            OnDetach.Invoke(attachmentPointIndex);
        }
       
        void OnJointBreak(float breakForce)
        {
            Detach();
        }

        void MouseOver()
        {
            mouseOver = false;
            guiDisassemble.Value = false;
            for (int i = 0; i < bolts.Length; i++) bolts[i].BoltOver();
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

        public void UpdateJointBreakValues() 
        {
            if (attached && joint)
            {
                joint.breakForce = baseBreakForce;
                for (int i = 0; i < bolts.Length; i++)
                    joint.breakForce += (bolts[i].jointBreakForceDelta * bolts[i].tightness);

                joint.breakTorque = baseBreakTorque;
                for (int i = 0; i < bolts.Length; i++)
                    joint.breakTorque += (bolts[i].jointBreakTorqueDelta * bolts[i].tightness);
            }
        }

        [Serializable] public class BoltMagnetOnAttach : UnityEvent<int> { }
        [Serializable] public class BoltMagnetOnDetach : UnityEvent<int> { }
    }
}
