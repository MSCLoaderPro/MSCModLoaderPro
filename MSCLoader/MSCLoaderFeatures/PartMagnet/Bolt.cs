using MSCLoader.Helper;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.PartMagnet
{
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
        public bool highlightBolt = true;

        [Header("(OPTIONAL) Custom AudioSource for screwing sound:")]
        public AudioSource customAudioSource;
        public AudioClip customScrewInSound;
        public AudioClip customScrewOutSound;

        [Header("(OPTIONAL) Breakable delta break-force and -torque:")]
        public float deltaBreakForce;
        public float deltaBreakTorque;

        [Header("(OPTIONAL) Highlight Options:")]
        public Material customHighlightMaterial;

        [Header("(OPTIONAL) Event actions:")]
        public UnityEvent OnScrew = new UnityEvent();
        public UnityEvent OnMaxTightness = new UnityEvent();
        public UnityEvent OnMinTightness = new UnityEvent();

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
            if (highlightMaterial == null) highlightMaterial = Instantiate(activeBolt ?? (activeBolt = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(material => material.name == "activebolt")));

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

        void Start()
        {
            OnEnable();
        }

        void OnEnable()
        {
            SetBoltPosition();

            if (boltMagnet && boltMagnet.joint) boltMagnet.StartCoroutine(boltMagnet.SetJointBreak());
        }

        public void Reset()
        {
            tightness = minTightness;
            SetBoltPosition();
        }

        void Update()
        {
            if ((!boltMagnet || boltMagnet.attached) && boltDetectionParent.activeInHierarchy)
            {
                if (toolWrenchSize.Value == size && boltDetection.Value == gameObject)
                {
                    boltOver = true;
                    if (highlightBolt) renderer.sharedMaterial = customHighlightMaterial ?? highlightMaterial;

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
                else if (boltOver) BoltOver();

                boltDelay -= Time.deltaTime;
            }
            else if (boltOver) BoltOver();
        }

        public void BoltOver()
        {
            boltOver = false;
            renderer.sharedMaterial = normalMaterial;
        }

        void BoltInOut(int direction)
        {
            tightness += direction;
            SetBoltPosition();

            if (boltMagnet && boltMagnet.joint) boltMagnet.UpdateJointBreakValues();

            if (customAudioSource != null) customAudioSource.PlayOneShot(direction > 0 ? customScrewInSound : customScrewOutSound);
            else transform.PlaySound3D("CarBuilding", "bolt_screw");

            boltDelay = ratchet.activeSelf ? 0.2f : 0.5f;

            OnScrew.Invoke();

            if (tightness == maxTightness) OnMaxTightness.Invoke();
            if (tightness == minTightness) OnMinTightness.Invoke();
        }

        void SetBoltPosition()
        {
            transform.localPosition = tightnessOriginalPosition + (tightnessPositionDelta * tightness);
            transform.localEulerAngles = tightnessOriginalRotation + (tightnessRotationDelta * tightness);
        }
    }
}
