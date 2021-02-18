using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSCLoader.MSCCar
{
    public class GearIndicator : MonoBehaviour
    {
        public Transform gearStick;
        public Drivetrain drivetrain;
        [Space(10)]
        public string gearLabels = "";
        public List<Vector3> gearPositions;

        List<string> gears;
        int tempGear = -1;

        GameObject gearIndicator;
        List<TextMesh> gearIndicatorText;

        [HideInInspector]
        public HutongGames.PlayMaker.FsmBool gearIndicatorOn;

        public void Awake()
        {
            if (Application.isEditor) { enabled = false; return; }

            gears = gearLabels.Split(',').ToList();

            Transform indicators = ModHelper.GetTransform("GUI", "Indicators");

            gearIndicator = Instantiate(indicators.Find("Gear").gameObject);
            gearIndicator.transform.SetParent(indicators, new Vector3(12f, 0.21f, 0f), Vector3.zero, Vector3.one, "TangerineGear");
            gearIndicator.SetActive(false);

            DestroyImmediate(gearIndicator.GetComponent<PlayMakerFSM>());
            gearIndicatorText = gearIndicator.GetComponentsInChildren<TextMesh>(true).ToList();
        }

        void Update()
        {
            if (tempGear != drivetrain.gear)
            {
                tempGear = drivetrain.gear;
                gearStick.localEulerAngles = gearPositions[tempGear];
                gearIndicatorText.ForEach(x => x.text = gears[tempGear]);
                gearStick.PlaySound3D("CarFoley", "gear_shift", 0.6f);
            }
        }

        void OnEnable()
        {
            tempGear = drivetrain.gear;
            gearIndicatorText.ForEach(x => x.text = gears[tempGear]);
            gearIndicator.SetActive(gearIndicatorOn.Value);
        }

        void OnDisable()
        {
            if (gearIndicator != null) gearIndicator.SetActive(false);
        }

        public void ToggleIndicator()
        {
            if (gameObject.activeInHierarchy) gearIndicator.SetActive(gearIndicatorOn.Value);
        }
    }

    public class SteeringLimiter : MonoBehaviour
    {
        public Rigidbody rigidbody;
        public Wheel[] frontWheels = new Wheel[2];
        public float maxSteeringAngle = 33f, minSteeringAngle = 2f, steeringAngle = 33f, velocityDivider = 3.3f;

        void FixedUpdate()
        {
            steeringAngle = Mathf.Clamp(maxSteeringAngle / (rigidbody.velocity.magnitude / velocityDivider), minSteeringAngle, maxSteeringAngle);
            for (int i = 0; i != frontWheels.Length; i++)
                frontWheels[i].maxSteeringAngle = steeringAngle;
        }

        void OnDisable()
        {
            try
            {
                steeringAngle = maxSteeringAngle;
                for (int i = 0; i != frontWheels.Length; i++)
                    frontWheels[i].maxSteeringAngle = maxSteeringAngle;
            }
            catch { }
        }
    }

    public class AutoClutch : MonoBehaviour
    {
        public AxisCarController carController;
        public Drivetrain drivetrain;
        public string clutchInput = "Clutch";

        public float clutchInputThreshold = 0.9f, brakeThreshold = 0.5f, handbrakeThreshold = 0.5f;

        void Awake() { if (Application.isEditor) { enabled = false; return; } }

        void Update()
        {
            if (cInput.GetAxis(clutchInput) > clutchInputThreshold || (drivetrain.autoClutch && (carController.brake > brakeThreshold || carController.handbrakeInput > handbrakeThreshold)))
                carController.clutchInput = 1f;
        }
    }

    public class GripSystem : MonoBehaviour
    {
        public Wheel[] wheels;

        public bool calculateRain = true;
        public float rainFactor = 1f;
        public float rainIntensityMultiplier = 0.025f;

        //1,1 | 1.05,1.05 | 1,1 | 1.15,1.15 | 1,1
        [HideInInspector]
        public Dictionary<CarDynamics.SurfaceType, float[]> surfaceDictionary;

        public float gripTrackForward, gripTrackSideways;
        public float gripGrassForward, gripGrassSideways;
        public float gripSandForward, gripSandSideways;
        public float gripOffroadForward, gripOffroadSideways;
        public float gripOilForward, gripOilSideways;

        HutongGames.PlayMaker.FsmFloat rainIntensity;

        void Start()
        {
            if (Application.isEditor) { enabled = false; return; }

            rainIntensity = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmFloat>("RainIntensity");

            surfaceDictionary = new Dictionary<CarDynamics.SurfaceType, float[]>
            {
                { CarDynamics.SurfaceType.track, new float[] { gripTrackForward, gripTrackSideways } },
                { CarDynamics.SurfaceType.offroad, new float[] { gripOffroadForward, gripOffroadSideways } },
                { CarDynamics.SurfaceType.grass, new float[] { gripGrassForward, gripGrassSideways } },
                { CarDynamics.SurfaceType.sand, new float[] { gripSandForward, gripSandSideways } },
                { CarDynamics.SurfaceType.oil, new float[] { gripOilForward, gripOilSideways } }
            };
        }

        void FixedUpdate()
        {
            rainFactor = calculateRain ? (1f - rainIntensity.Value * rainIntensityMultiplier) : 1f;
            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].forwardGripFactor = surfaceDictionary[wheels[i].surfaceType][0] * rainFactor;
                wheels[i].sidewaysGripFactor = surfaceDictionary[wheels[i].surfaceType][1] * rainFactor;
            }
        }
    }

    public class LeanLeftRight : MonoBehaviour
    {
        public Animation leanAnimation;
        public Door leftDoor;
        public WindowOpener leftWindow;

        [Space(10)]
        public string leanLeft = "TangerineLeanLeftIn";
        public string leanLeftOff = "TangerineLeanLeftOut";
        public string leanRight = "TangerineLeanRightIn";
        public string leanRightOff = "TangerineLeanRightOut";

        [Space(10)]
        public string leanLeftButton = "ReachLeft";
        public string leanRightButton = "ReachRight";

        bool leaningLeft = false, leaningRight = false;

        void Awake() { if (Application.isEditor) { enabled = false; return; } }

        void Update()
        {
            if (cInput.GetButton(leanLeftButton) && !leaningLeft && !leaningRight && (leftDoor.doorOpen || leftWindow.windowState >= 35))
            {
                leaningLeft = true;
                leanAnimation.Play(leanLeft, PlayMode.StopAll);
            }
            else if (cInput.GetButtonUp(leanLeftButton) && leaningLeft)
            {
                leaningLeft = false;
                leanAnimation.Play(leanLeftOff, PlayMode.StopAll);
            }
            else if (cInput.GetButton(leanRightButton) && !leaningRight && !leaningLeft)
            {
                leaningRight = true;
                leanAnimation.Play(leanRight, PlayMode.StopAll);
            }
            else if (cInput.GetButtonUp(leanRightButton) && leaningRight)
            {
                leaningRight = false;
                leanAnimation.Play(leanRightOff, PlayMode.StopAll);
            }
        }
    }
}
