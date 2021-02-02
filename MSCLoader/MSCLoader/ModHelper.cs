using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using Random = UnityEngine.Random;
using System.IO;
using System.Diagnostics;

namespace MSCLoader
{
    public static class ModHelper
    {
        public static PlayMakerFSM GetFsmByName(this GameObject gameObject, string fsmName)
        {
            PlayMakerFSM[] fsms = gameObject.GetComponents<PlayMakerFSM>();

            if (fsms == null) return null;

            for (int i = 0; i != fsms.Length; i++)
                if (fsms[i].FsmName == fsmName) return fsms[i];

            return null;
        }

        public static PlayMakerFSM GetFsmByName(this Transform transform, string fsmName)
        {
            return transform.gameObject.GetFsmByName(fsmName);
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int actionIndex) where T : FsmStateAction
        {
            FsmState state = null;
            for (int i = 0; i != fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    state = fsm.FsmStates[i];
                    break;
                }
            }
            if (state == null)
                throw new System.Exception($"GetAction<T>: Can't find state with name {stateName} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");

            if (state.Actions[actionIndex] is T)
                return state.Actions[actionIndex] as T;
            else throw new System.Exception($"GetAction<T>: Action of specified type {typeof(T).ToString()} can't be found on index {actionIndex} in state {stateName} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");
        }

        public static T GetAction<T>(this PlayMakerFSM fsm, int stateIndex, int actionIndex) where T : FsmStateAction
        {
            if (fsm.FsmStates[stateIndex].Actions[actionIndex] is T)
                return fsm.FsmStates[stateIndex].Actions[actionIndex] as T;
            else throw new System.Exception($"GetAction<T>: Action of specified type {typeof(T).ToString()} can't be found on index {actionIndex} in state {stateIndex} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");
        }

        public static void InsertAction(this PlayMakerFSM fsm, string stateName, int actionIndex, FsmStateAction action)
        {
            FsmState state = null;
            for (int i = 0; i != fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    state = fsm.FsmStates[i];
                    break;
                }
            }
            if (state == null)
                throw new System.Exception($"InsertAction<T>: Can't find state with name {stateName} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");

            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Insert(actionIndex, action);
            state.Actions = actions.ToArray();
        }

        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action)
        {
            FsmState state = null;
            for (int i = 0; i != fsm.FsmStates.Length; i++)
            {
                if (fsm.FsmStates[i].Name == stateName)
                {
                    state = fsm.FsmStates[i];
                    break;
                }
            }
            if (state == null)
                throw new System.Exception($"InsertAction<T>: Can't find state with name {stateName} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");

            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Add(action);
            state.Actions = actions.ToArray();
        }

        public static void ReplaceAction(this PlayMakerFSM fsm, string stateName, int actionIndex, FsmStateAction action)
        {
            FsmState state = fsm.FsmStates.FirstOrDefault(x => x.Name == stateName);
            List<FsmStateAction> actions = state.Actions.ToList();
            if (actionIndex < 0 && actionIndex >= actions.Count) throw new ArgumentOutOfRangeException();
            actions[actionIndex] = action;
            state.Actions = actions.ToArray();
        }

        public static void RemoveAction(this PlayMakerFSM fsm, string stateName, int actionIndex)
        {
            FsmState state = fsm.FsmStates.FirstOrDefault(x => x.Name == stateName);
            List<FsmStateAction> actions = state.Actions.ToList();
            if (actionIndex < 0 && actionIndex >= actions.Count) throw new ArgumentOutOfRangeException();
            actions.RemoveAt(actionIndex);
            state.Actions = actions.ToArray();
        }

        public static T GetVariable<T>(this PlayMakerFSM fsm, string name) where T : NamedVariable =>
            fsm.FsmVariables.FindVariable<T>(name);

        public static T GetGlobal<T>(string name) where T : NamedVariable =>
            PlayMakerGlobals.Instance.Variables.FindVariable<T>(name);

        public static T FindVariable<T>(this FsmVariables variables, string name) where T : NamedVariable
        {
            switch (typeof(T))
            {
                case Type type when typeof(T) == typeof(FsmFloat): return variables.FindFsmFloat(name) as T;
                case Type type when typeof(T) == typeof(FsmInt): return variables.FindFsmInt(name) as T;
                case Type type when typeof(T) == typeof(FsmBool): return variables.FindFsmBool(name) as T;
                case Type type when typeof(T) == typeof(FsmString): return variables.FindFsmString(name) as T;
                case Type type when typeof(T) == typeof(FsmVector2): return variables.FindFsmVector2(name) as T;
                case Type type when typeof(T) == typeof(FsmVector3): return variables.FindFsmVector3(name) as T;
                case Type type when typeof(T) == typeof(FsmGameObject): return variables.FindFsmGameObject(name) as T;
                case Type type when typeof(T) == typeof(FsmMaterial): return variables.FindFsmMaterial(name) as T;
                case Type type when typeof(T) == typeof(FsmObject): return variables.FindFsmObject(name) as T;
                default: return null;
            }
        }

        internal static Dictionary<float, WaitForSeconds> waitDictionary = new Dictionary<float, WaitForSeconds>();
        public static WaitForSeconds GetWaitTime(float waitTime)
        {
            if (waitDictionary.TryGetValue(waitTime, out WaitForSeconds wait)) return wait;

            waitDictionary.Add(waitTime, new WaitForSeconds(waitTime));
            return waitDictionary[waitTime];
        }

        internal static WaitForFixedUpdate waitFixedUpdate = new WaitForFixedUpdate();
        public static WaitForFixedUpdate GetWaitFixed() => waitFixedUpdate;

        public static Transform GetTransform(string parentPath, string childPath) =>
            GameObject.Find(parentPath).transform.Find(childPath);

        public static void PlaySound3D(this Transform transform, string type, string variation, float volume = 1f) =>
            MasterAudio.PlaySound3DAndForget(type, transform, variationName: variation, volumePercentage: volume);

        public static void PlaySound3D(this Vector3 vector3, string type, string variation, float volume = 1f) =>
            MasterAudio.PlaySound3DAtVector3AndForget(type, vector3, variationName: variation, volumePercentage: volume);

        public static T SelectRandom<T>(this IList<T> list) => list[Random.Range(0, list.Count)];

        public static bool InLayerMask(this LayerMask layerMask, int layer) => layerMask == (layerMask | (1 << layer));
        public static bool InLayerMask(this LayerMask layerMask, string layer) => layerMask == (layerMask | (1 << LayerMask.NameToLayer(layer)));

        public static void SetParentLocal(this Transform transform, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale, string name = "")
        {
            if (name != "") transform.name = name;

            transform.parent = parent;
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }

        public static void OpenFolder(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                Process.Start(path);
                System.Console.WriteLine($"MODLOADER: Opening path in explorer: {path}");
                return;
            }
            ModConsole.LogError($"Can't find path/file: {path}");
        }

        public static void OpenWebsite(string url)
        {
            Application.OpenURL(url);
            System.Console.WriteLine($"MODLOADER: Opening website in external browser: {url}");
        }
    }
}
