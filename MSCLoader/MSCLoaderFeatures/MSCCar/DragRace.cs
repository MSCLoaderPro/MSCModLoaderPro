using MSCLoader.Helper;
using System.Linq;
using UnityEngine;

namespace MSCLoader.MSCCar
{
    public class DragRaceVehicle : MonoBehaviour
    {
        public Transform vehicle;
        public string vehicleName = "";
        public Vector3 stagingWheelPosition;

        [HideInInspector]
        public DragRace dragRace;

        void Start()
        {
            dragRace = DragRace.AddVehicle(vehicle, vehicleName, stagingWheelPosition);
        }
    }

    public class DragRace
    {
        internal HutongGames.PlayMaker.FsmString Car;
        internal HutongGames.PlayMaker.FsmString Name;
        internal HutongGames.PlayMaker.FsmInt ID;
        public string VehicleName { get => Car.Value; set => Car.Value = value; }
        public string PlayerName { get => Name.Value; set => Name.Value = value; }
        public int VehicleID { get => ID.Value; internal set => ID.Value = value; }

        internal DragRace(PlayMakerFSM data)
        {
            Car = data.GetVariable<HutongGames.PlayMaker.FsmString>("Car");
            Name = data.GetVariable<HutongGames.PlayMaker.FsmString>("Name");
            ID = data.GetVariable<HutongGames.PlayMaker.FsmInt>("ID");
        }

        public static DragRace AddVehicle(Transform vehicle, string vehicleName, Vector3 stagingWheelPosition)
        {
            GameObject stagingWheel = Object.Instantiate(ModHelper.GetTransform("HAYOSIKO(1500kg, 250)", "StagingWheel").gameObject);
            stagingWheel.transform.SetParent(vehicle, stagingWheelPosition, Vector3.zero, Vector3.one, "StagingWheel");

            DragRace dragVehicle = new DragRace(stagingWheel.GetComponent<PlayMakerFSM>())
            {
                VehicleName = vehicleName,
                PlayerName = PlayMakerHelper.GetGlobalVariable<HutongGames.PlayMaker.FsmString>("PlayerName").Value
            };

            string[] stringProxies = new string[] { "Speeds", "Cars", "Names", "ResultsTimes", "ResultsSpeeds", "ResultsNames", "ResultsCars" };
            PlayMakerArrayListProxy[] proxyLists = GameObject.Find("DRAGRACE").transform.Find("LOD/DRAGSTRIP/DragTiming").GetComponents<PlayMakerArrayListProxy>();
            foreach (PlayMakerArrayListProxy proxy in proxyLists)
            {
                if (proxy.referenceName == "Times")
                {
                    proxy.AddPrefill(0f);
                    if (proxy._arrayList != null && proxy._arrayList.Count > 0) proxy._arrayList.Add(0f);

                    continue;
                }
                if (stringProxies.Contains(proxy.referenceName))
                {
                    proxy.AddPrefill("");
                    if (proxy._arrayList != null && proxy._arrayList.Count > 0) proxy._arrayList.Add("");

                    continue;
                }
            }

            PlayMakerHashTableProxy hashTable = GameObject.Find("DRAGRACE").transform.Find("LOD/DRAGSTRIP/DragTiming").GetComponents<PlayMakerHashTableProxy>().FirstOrDefault(x => x.referenceName == "Results");
            hashTable.AddPrefill("", 0f);
            if (hashTable._hashTable != null && hashTable._hashTable.Count > 0) hashTable._hashTable.Add("", 0f);

            dragVehicle.VehicleID = hashTable.preFillCount - 1;

            return dragVehicle;
        }
    }
}
