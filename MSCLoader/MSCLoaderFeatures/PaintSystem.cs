using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.Paint
{
    internal class PaintCore : MonoBehaviour
    {
        public static PaintCore instance = null;

        public static Material customMaterial = null;
        public static Material redGTMaterial = null;
        public static Material greenGTMaterial = null;

        bool sprayCanSetup = false;
        bool fleetariPaintSetup = false;

        public List<PaintSystem> paintSystems;

        public static PaintCore Setup()
        {
            if (ModLoader.CurrentScene != CurrentScene.Game) 
                throw new System.Exception("PaintCore: Can't setup painting outside the game scene!");

            if (instance == null)
            {
                instance = GameObject.Find("MSCLoader").AddComponent<PaintCore>();
                instance.paintSystems = new List<PaintSystem>();
            }

            if (customMaterial == null) 
                customMaterial = ModHelper.GetTransform("REPAIRSHOP", "Jobs/Paintjob").GetPlayMakerFSM("Work").GetVariable<HutongGames.PlayMaker.FsmMaterial>("PaintArt").Value;
            if (redGTMaterial == null)
                redGTMaterial = ModHelper.GetTransform("REPAIRSHOP", "Jobs/Paintjob").GetPlayMakerFSM("Work").GetVariable<HutongGames.PlayMaker.FsmMaterial>("PaintGT").Value;
            if (greenGTMaterial == null)
                greenGTMaterial = ModHelper.GetTransform("REPAIRSHOP", "Jobs/Paintjob").GetPlayMakerFSM("Work").GetVariable<HutongGames.PlayMaker.FsmMaterial>("PaintGT2").Value;

            if (!instance.sprayCanSetup) instance.SetupSprayCan();

            if (!instance.fleetariPaintSetup) instance.SetupFleetariPainting();

            return instance;
        }

        void OnDestroy()
        {
            customMaterial = null;
            redGTMaterial = null;
            greenGTMaterial = null;

            instance = null;

            sprayCanSetup = false;
        }

        void SetupSprayCan()
        {
            sprayCanSetup = true;
            PlayMakerFSM sprayCan = ModHelper.GetTransform("PLAYER", "Pivot/AnimPivot/Camera/FPSCamera/SprayCan")?.GetPlayMakerFSM("Paint");

            if (sprayCan == null)
            {
                sprayCanSetup = false;
                ModConsole.LogError("PaintCore: CAN'T FIND SPRAYCAN FSM ON THE PLAYER.");
                return;
            }

            sprayCan.Initialize();

            HutongGames.PlayMaker.FsmInt sprayPaintType = sprayCan.GetVariable<HutongGames.PlayMaker.FsmInt>("PaintType");
            HutongGames.PlayMaker.FsmColor sprayPaintColor = sprayCan.GetVariable<HutongGames.PlayMaker.FsmColor>("SprayColor");
            HutongGames.PlayMaker.FsmGameObject sprayPaintPart = sprayCan.GetVariable<HutongGames.PlayMaker.FsmGameObject>("PaintedPart");

            sprayCan.InsertAction("Regular paint", 0, new PaintSystemSprayCan(sprayPaintType, sprayPaintColor, sprayPaintPart));

            ModConsole.Log("PaintCore: Spray Can setup complete!");
        }

        void SetupFleetariPainting()
        {
            fleetariPaintSetup = true;
            PlayMakerFSM paintJob = ModHelper.GetTransform("REPAIRSHOP", "Jobs/Paintjob")?.GetPlayMakerFSM("Work");

            if (paintJob == null)
            {
                fleetariPaintSetup = false;
                ModConsole.LogError("PaintCore: CAN'T FIND WORK FSM ON THE REPAIRSHOP/Jobs/Paintjob.");
                return;
            }

            paintJob.Initialize();

            HutongGames.PlayMaker.FsmInt paintType = paintJob.GetVariable<HutongGames.PlayMaker.FsmInt>("PaintType");
            HutongGames.PlayMaker.FsmColor colorSelected = paintJob.GetVariable<HutongGames.PlayMaker.FsmColor>("ColorSelected");
            HutongGames.PlayMaker.FsmGameObject distanceTarget = paintJob.GetVariable<HutongGames.PlayMaker.FsmGameObject>("_DistanceTarget");

            paintJob.InsertAction("Body", 0, new PaintSystemFleetariPaint(paintType, colorSelected, distanceTarget));

            ModConsole.Log("PaintCore: Fleetari Painting setup complete!");
        }

        internal IEnumerator ApplyFleetariPaintJob(int paintType, Color color, Vector3 distanceTarget)
        {
            if (paintSystems.Count > 0)
            {
                ModConsole.Log("PaintCore: Fleetari painting initiated.");
                foreach (PaintSystem paintSystem in paintSystems)
                {
                    if (paintSystem.fleetariPainting && Vector3.Distance(paintSystem.transform.position, distanceTarget) <= 20)
                        paintSystem.Paint(paintType, color);

                    yield return null;
                }
                ModConsole.Log("PaintCore: Fleetari painting complete.");
            }
        }
    }

    [AddComponentMenu("Mod Loader Pro/Paint System")]
    public class PaintSystem : MonoBehaviour
    {
        //0 = default paint (rust or whatever), 1 = Regular paint, 2 = Metallic paint, 3 = Matte Paint, 4 = Custom Paint, 5 = GT RED, 6 = GT Green
        [HideInInspector] public int paintType;
        [HideInInspector] public Color color;

        [HideInInspector] public List<Material> materials = new List<Material>();
        [Header("Paint System, created by Fredrik!"), Header("Paint Materials:")]
        public Material rusty;
        public Material regular, metallic, matte, custom, redGT, greenGT;

        [Space(10)]
        public bool useDefaultCustomMaterial = false;
        public Color customPaintColor = Color.white;
        public bool useDefaultRedGTMaterial = false;
        public Color redGTPaintColor = Color.white;
        public bool useDefaultGreenGTMaterial = false;
        public Color greenGTPaintColor = Color.white;

        public Renderer[] renderers = new Renderer[1];
        [SerializeField] string[] rendererMaterialIndexes = new string[1];
        List<int[]> indexList = new List<int[]>();

        [Header("Fleetari Repair Shop Painting:")]
        public bool fleetariPainting = false;

        [Header("Painting Event:")]
        public PaintSystemOnPaint OnPaint = new PaintSystemOnPaint();

        void Awake()
        {
            PaintCore.Setup().paintSystems.Add(this);

            materials.Add(CopyMaterial(rusty));
            materials.Add(CopyMaterial(regular));
            materials.Add(CopyMaterial(metallic));
            materials.Add(CopyMaterial(matte));

            materials.Add(CopyMaterial(custom));
            materials.Add(CopyMaterial(redGT));
            materials.Add(CopyMaterial(greenGT));

            foreach (string indexString in rendererMaterialIndexes)
                indexList.Add(indexString.Split(',').Select(int.Parse).ToArray());
        }
        public void Paint(int newPaintType, Color newColor)
        {
            paintType = newPaintType;

            switch (paintType)
            {
                case 4: 
                    color = customPaintColor;
                    SetMaterial(useDefaultCustomMaterial ? PaintCore.customMaterial : materials[paintType]);
                    break;
                case 5: 
                    color = redGTPaintColor;
                    SetMaterial(useDefaultRedGTMaterial ? PaintCore.redGTMaterial : materials[paintType]);
                    break;
                case 6: 
                    color = greenGTPaintColor;
                    SetMaterial(useDefaultGreenGTMaterial ? PaintCore.greenGTMaterial : materials[paintType]);
                    break;
                default: 
                    color = newColor;
                    SetMaterial(materials[paintType]);
                    break;
            }

            OnPaint.Invoke(paintType, color);

            ModConsole.Log($"PaintSystem: {gameObject.name} painted with type: {paintType} and color: {color}");
        }
        public void SetMaterial(Material newMaterial)
        {
            if (newMaterial == null) return;

            //Change color and paint type for all the renderers with the provided new color and painttype.
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] rendererMaterials = renderers[i].sharedMaterials;
                foreach (int index in indexList[i])
                {
                    rendererMaterials[index] = newMaterial;
                    rendererMaterials[index].color = color;
                }
                renderers[i].sharedMaterials = rendererMaterials;
            }
        }
        public Material CopyMaterial(Material material)
        {
            if (material == null) return null;

            Material newMaterial = Instantiate(material);
            newMaterial.name = $"{material.name} ({gameObject.name})";

            return newMaterial;
        }

        [System.Serializable]
        public class PaintSystemOnPaint : UnityEvent<int, Color> { }
    }

    class PaintSystemSprayCan : HutongGames.PlayMaker.FsmStateAction
    {
        public HutongGames.PlayMaker.FsmInt paintType;
        public HutongGames.PlayMaker.FsmColor sprayColor;
        public HutongGames.PlayMaker.FsmGameObject paintedPart;

        public PaintSystemSprayCan(HutongGames.PlayMaker.FsmInt type, HutongGames.PlayMaker.FsmColor color, HutongGames.PlayMaker.FsmGameObject part)
        {
            paintType = type;
            sprayColor = color;
            paintedPart = part;
        }

        public override void OnEnter()
        {
            // If the part has a paintsystem component, then execute the paint method with the new color and painttype.
            paintedPart.Value?.GetComponent<PaintSystem>()?.Paint(paintType.Value, sprayColor.Value);

            Finish();
        }
    }

    class PaintSystemFleetariPaint : HutongGames.PlayMaker.FsmStateAction
    {
        public HutongGames.PlayMaker.FsmInt paintType;
        public HutongGames.PlayMaker.FsmColor colorSelected;
        public HutongGames.PlayMaker.FsmGameObject distanceTarget;

        public PaintSystemFleetariPaint(HutongGames.PlayMaker.FsmInt type, HutongGames.PlayMaker.FsmColor color, HutongGames.PlayMaker.FsmGameObject target)
        {
            paintType = type;
            colorSelected = color;
            distanceTarget = target;
        }
        public override void OnEnter()
        {
            // If the part has a paintsystem component, then execute the paint method with the new color and painttype.
            PaintCore.instance?.StartCoroutine(PaintCore.instance.ApplyFleetariPaintJob(paintType.Value, colorSelected.Value, distanceTarget.Value.transform.position));

            Finish();
        }
    }
}
