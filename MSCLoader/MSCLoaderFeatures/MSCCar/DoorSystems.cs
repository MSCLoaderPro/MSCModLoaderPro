using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MSCLoader.MSCCar
{
    public class Door : MonoBehaviour
    {
        public InteractionRaycast interaction;

        public Rigidbody carRigidbody;
        public Rigidbody doorRigidbody;

        public Collider handle;
        public HingeJoint doorJoint;
        [Space(10)]
        public Vector3 openTorque;
        public Vector3 closeTorque;

        public Vector3 vector3Open;
        public Vector3 vector3Closed;

        [Space(10)]
        public FixedJoint lockJoint;
        public float lockJointBreakForceClosed = 24000f;
        public float lockJointBreakForceOpen = 280f;

        [HideInInspector]
        public bool doorOpen = false;
        bool doorMoving = false;
        bool doorNPCMoving = false;

        Quaternion openRot;
        Quaternion closeRot;
        bool mouseOver = false;

        WaitForFixedUpdate waitFixed = new WaitForFixedUpdate();
        WaitForSeconds wait = new WaitForSeconds(0.1f);

        void Start()
        {
            openRot = Quaternion.Euler(vector3Open);
            closeRot = Quaternion.Euler(vector3Closed);
        }

        void Update()
        {
            if (interaction.GetHit(handle))
            {
                mouseOver = interaction.Use(true);

                if (Input.GetMouseButtonDown(0) && !doorMoving && !doorNPCMoving) StartCoroutine(DoorAction());
            }
            else if (mouseOver) mouseOver = interaction.Use(false);

            if (Input.GetMouseButtonUp(0) && doorMoving) doorMoving = false;
        }

        void OnJointBreak(float breakForce)
        {
            Debug.Log($"TANGERINE: Door Joint broken at {breakForce}");
            doorOpen = true;
        }

        IEnumerator DoorAction()
        {
            doorMoving = true;

            bool playOpenSound = false;
            if (lockJoint != null)
            {
                playOpenSound = true;
                Destroy(lockJoint);
            }

            if (!doorOpen)
            {
                doorOpen = true;
                if (playOpenSound) MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "open_door1");

                while (doorMoving)
                {
                    doorRigidbody.AddRelativeTorque(openTorque.x, openTorque.y, openTorque.z, ForceMode.Force);

                    if (Quaternion.Angle(transform.localRotation, openRot) <= 0.01f)
                    {
                        lockJoint = gameObject.AddComponent<FixedJoint>();
                        lockJoint.connectedBody = carRigidbody;

                        yield return wait;

                        lockJoint.breakTorque = lockJointBreakForceOpen;
                        break;
                    }
                    yield return waitFixed;
                }
            }
            else
            {
                while (doorMoving)
                {
                    doorRigidbody.AddRelativeTorque(closeTorque.x, closeTorque.y, closeTorque.z, ForceMode.Force);

                    if (Quaternion.Angle(transform.localRotation, closeRot) <= 0.01f)
                    {
                        doorOpen = false;
                        transform.localEulerAngles = vector3Closed;
                        MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "close_door1");
                        lockJoint = CreateFixedJoint(carRigidbody, lockJointBreakForceClosed);
                        break;
                    }
                    yield return waitFixed;
                }
            }
            doorMoving = false;
        }

        public void StartOpenNPC()
        {
            StartCoroutine(NPCOpen());
        }

        public IEnumerator NPCOpen()
        {
            Debug.Log("TANGERINE: NPC Door open begun");

            doorNPCMoving = true;

            bool playOpenSound = false;
            if (lockJoint != null)
            {
                playOpenSound = true;
                Destroy(lockJoint);
            }

            doorOpen = true;
            if (playOpenSound) MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "open_door1");

            while (doorNPCMoving)
            {
                doorRigidbody.AddRelativeTorque(openTorque.x, openTorque.y, openTorque.z, ForceMode.Force);

                if (Quaternion.Angle(transform.localRotation, openRot) <= 0.1f)
                {
                    Debug.Log("TANGERINE: NPC Door open lock");

                    yield return wait;
                    lockJoint = gameObject.AddComponent<FixedJoint>();
                    lockJoint.connectedBody = carRigidbody;
                    yield return wait;
                    lockJoint.breakTorque = lockJointBreakForceOpen;

                    break;
                }

                yield return waitFixed;
            }

            yield return new WaitForSeconds(2.7f);

            Destroy(lockJoint);

            yield return null;

            Debug.Log("TANGERINE: NPC Door close begun");
            while (doorNPCMoving)
            {
                doorRigidbody.AddRelativeTorque(closeTorque.x, closeTorque.y, closeTorque.z, ForceMode.Force);

                if (Quaternion.Angle(transform.localRotation, closeRot) <= 0.1f)
                {
                    Debug.Log("TANGERINE: NPC Door close lock");

                    doorOpen = false;
                    transform.localEulerAngles = vector3Closed;
                    MasterAudio.PlaySound3DAndForget("CarFoley", transform, variationName: "close_door1");
                    lockJoint = CreateFixedJoint(carRigidbody, lockJointBreakForceClosed);
                    break;
                }

                yield return waitFixed;
            }

            doorNPCMoving = false;

            Debug.Log("TANGERINE: NPC Door done");
        }

        FixedJoint CreateFixedJoint(Rigidbody rigidbody, float breakForce, float breakTorque = float.NaN)
        {
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = rigidbody;
            joint.breakForce = breakForce;
            joint.breakTorque = float.IsNaN(breakTorque) ? breakForce : breakTorque;

            return joint;
        }
    }
}
