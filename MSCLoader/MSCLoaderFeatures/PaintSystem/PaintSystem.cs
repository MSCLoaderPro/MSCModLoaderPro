using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.Paint
{
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
        [HideInInspector] public List<int[]> indexList = new List<int[]>();

        [Header("Fleetari Repair Shop Painting:")]
        public bool fleetariPainting = false;

        [Header("Painting Event:")]
        public UnityEvent OnPaint = new UnityEvent();

        void Awake()
        {
            if (Application.isEditor) { enabled = false; return; }

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

            OnPaint.Invoke();

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

        void OnDestroy()
        {
            try
            {
                PaintCore.instance.paintSystems.Remove(this);
            }
            catch { }
        }
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
