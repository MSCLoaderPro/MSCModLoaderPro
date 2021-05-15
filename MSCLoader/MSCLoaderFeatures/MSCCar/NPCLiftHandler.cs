using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace MSCLoader.MSCCar
{
    public class NPCLiftHandler : MonoBehaviour
    {
        public enum LiftType { DrunkGuy, DrunkGuyMoving, GrandmotherChurchLift }

        public LiftType liftType;
        public string vehicleName;
        public GameObject getInPivot, carMassPassenger;

        public UnityEvent openDoor = new UnityEvent();

        SendEvent doorAction1, doorAction2;

        [HideInInspector]
        internal FsmString CurrentVehicle;
        [HideInInspector]
        public bool customVehicle = false;

        void Start()
        {
            switch (liftType)
            {
                case LiftType.DrunkGuy: DrunkGuySetup(); break;
            }
        }

        void DrunkGuySetup()
        {

        }
    }

    public class LiftVehicleCheck : FsmStateAction
    {
        public NPCLiftHandler liftHandler;
        string sendEvent = "RUSCKO";
    }
}
