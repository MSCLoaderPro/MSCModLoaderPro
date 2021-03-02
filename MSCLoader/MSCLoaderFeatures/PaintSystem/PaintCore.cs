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
}
