using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using Random = UnityEngine.Random;
using System.Collections;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary> Container for useful helper methods </summary>
    public static class ModHelper
    {
        /// <summary>Get a Transform child.</summary>
        /// <param name="parentPath">Hierarchy path to a parent Transform.</param>
        /// <param name="childPath">Hierarchy path from the parent to the wanted child Transform.</param>
        /// <returns>Transform with specified path.</returns>
        public static Transform GetTransform(string parentPath, string childPath) =>
            GameObject.Find(parentPath).transform.Find(childPath);
        /// <summary>Play a MasterAudio sound from the Transform.</summary>
        /// <param name="transform">Transform to play the sound from.</param>
        /// <param name="type">Type of sound.</param>
        /// <param name="variation">Variation of sound.</param>
        /// <param name="volume">(Optional) Sound volume.</param>
        public static void PlaySound3D(this Transform transform, string type, string variation, float volume = 1f) =>
            MasterAudio.PlaySound3DAndForget(type, transform, variationName: variation, volumePercentage: volume);
        /// <summary>Play a MasterAudio sound from a Vector3 world position.</summary>
        /// <param name="vector3">Vector3 to play the sound from.</param>
        /// <param name="type">Type of sound.</param>
        /// <param name="variation">Variation of sound.</param>
        /// <param name="volume">(Optional) Sound volume.</param>
        public static void PlaySound3D(this Vector3 vector3, string type, string variation, float volume = 1f) =>
            MasterAudio.PlaySound3DAtVector3AndForget(type, vector3, variationName: variation, volumePercentage: volume);
        /// <summary>Select a random element from a List or an Array.</summary>
        /// <typeparam name="T">List/Array Element type.</typeparam>
        /// <param name="list">List/Array to get a random element from.</param>
        /// <returns></returns>
        public static T SelectRandom<T>(this IList<T> list) => 
            list[Random.Range(0, list.Count)];
        /// <summary>Check if a specified layer is in the LayerMask.</summary>
        /// <param name="layerMask">LayerMask to check.</param>
        /// <param name="layer">Layer number to check for.</param>
        /// <returns>Whether or not the layer is in the </returns>
        public static bool InLayerMask(this LayerMask layerMask, int layer) => 
            layerMask == (layerMask | (1 << layer));
        /// <summary>Check if a specified layer is in the LayerMask.</summary>
        /// <param name="layerMask">LayerMask to check.</param>
        /// <param name="layer">Layer name to check for.</param>
        /// <returns>Whether or not the layer is in the </returns>
        public static bool InLayerMask(this LayerMask layerMask, string layer) => 
            layerMask == (layerMask | (1 << LayerMask.NameToLayer(layer)));
        /// <summary>Extension of SetParent, with this you can specify the new position, rotation, scale and name of the transform.</summary>
        /// <param name="transform">Transform to change parent of.</param>
        /// <param name="parent">New parent Transform.</param>
        /// <param name="position">New local position.</param>
        /// <param name="rotation">New local rotation (Euler Angles).</param>
        /// <param name="scale">New local scale.</param>
        /// <param name="name">(Optional) New name.</param>
        public static void SetParent(this Transform transform, Transform parent, Vector3 position, Vector3 rotation, Vector3 scale, string name = "")
        {
            if (name != "") transform.name = name;

            transform.parent = parent;
            transform.localPosition = position;
            transform.localEulerAngles = rotation;
            transform.localScale = scale;
        }
        /// <summary>Open folder in Explorer or file in its default app.</summary>
        /// <param name="path">Path to folder/file.</param>
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
        /// <summary>Open Website URL.</summary>
        /// <param name="url">Website URL.</param>
        public static void OpenWebsite(string url)
        {
            Application.OpenURL(url);
            System.Console.WriteLine($"MODLOADER: Opening website in external web browser: {url}");
        }
    }
    /// <summary>Container for PlayMaker related helper and extension methods.</summary>
    public static class PlayMakerHelper
    {
        /// <summary>Get a PlayMakerFSM by name on the GameObject.</summary>
        /// <param name="gameObject">The GameObject to look for the FSM on.</param>
        /// <param name="fsmName">Name of the FSM.</param>
        /// <returns>PlayMakerFSM of specified name, or null if it can't be found.</returns>
        public static PlayMakerFSM GetPlayMakerFSM(this GameObject gameObject, string fsmName)
        {
            PlayMakerFSM[] fsms = gameObject.GetComponents<PlayMakerFSM>();

            if (fsms == null) return null;

            for (int i = 0; i != fsms.Length; i++)
                if (fsms[i].FsmName == fsmName) return fsms[i];

            return null;
        }
        /// <summary>Get a PlayMakerFSM by name on the Transform's GameObject.</summary>
        /// <param name="transform">The Transform to look for the FSM on.</param>
        /// <param name="fsmName">Name of the FSM.</param>
        /// <returns>PlayMakerFSM of specified name, or null if it can't be found.</returns>
        public static PlayMakerFSM GetPlayMakerFSM(this Transform transform, string fsmName) =>
            transform.gameObject.GetPlayMakerFSM(fsmName);
        /// <summary>Get a state on the PlayMakerFSM.</summary>
        /// <param name="fsm">PlayMakerFSM to search on.</param>
        /// <param name="stateName">Name of the state to look for.</param>
        /// <returns>FsmState of specified name.</returns>
        public static FsmState GetState(this PlayMakerFSM fsm, string stateName)
        {
            for (int i = 0; i != fsm.FsmStates.Length; i++)
                if (fsm.FsmStates[i].Name == stateName) return fsm.FsmStates[i];
            throw new Exception($"GetState: Can't find state with name {stateName} on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");
        }
        /// <summary>Get a state on the PlayMakerFSM.</summary>
        /// <param name="fsm">PlayMakerFSM to search on.</param>
        /// <param name="stateIndex">Index of the state to look for.</param>
        /// <returns>FsmState of specified index.</returns>
        public static FsmState GetState(this PlayMakerFSM fsm, int stateIndex)
        {
            if (fsm.FsmStates.Length > stateIndex) return fsm.FsmStates[stateIndex];
            throw new IndexOutOfRangeException($"GetState: stateIndex out of range on FSM {fsm.FsmName} on GameObject {fsm.gameObject.name}");
        }
        /// <summary>Get a PlayMaker FSMStateAction of specified type in the specified state.</summary>
        /// <typeparam name="T">PlayMaker Action Type, must be of type FSMStateAction or sub-class</typeparam>
        /// <param name="state">State that contains the action</param>
        /// <param name="actionIndex">The index of the Actions Array in the FSMState.</param>
        /// <returns>FSMStateAction of type T</returns>
        public static T GetAction<T>(this FsmState state, int actionIndex) where T : FsmStateAction
        {
            if (state.Actions[actionIndex] is T) return state.Actions[actionIndex] as T;
            else throw new Exception($"GetAction<T>: Action of specified type {typeof(T)} can't be found on index {actionIndex} in state {state.Name} on FSM {state.Fsm.Name} on GameObject {state.Fsm.OwnerName}");
        }
        /// <summary>Get a PlayMaker FSMStateAction of specified type in the specified state.</summary>
        /// <typeparam name="T">PlayMaker Action Type, must be of type FSMStateAction or sub-class</typeparam>
        /// <param name="fsm">PlayMakerFSM to get the action from.</param>
        /// <param name="stateName">Name of the state that contains the action</param>
        /// <param name="actionIndex">The index of the Actions Array in the FSMState.</param>
        /// <returns>FSMStateAction of type T</returns>
        public static T GetAction<T>(this PlayMakerFSM fsm, string stateName, int actionIndex) where T : FsmStateAction =>
            fsm.GetState(stateName).GetAction<T>(actionIndex);
        /// <summary>Get a PlayMaker FSMStateAction of specified type in the specified state.</summary>
        /// <typeparam name="T">PlayMaker Action Type, must be of type FSMStateAction or sub-class</typeparam>
        /// <param name="fsm">PlayMakerFSM to get the action from.</param>
        /// <param name="stateIndex">Index of the state that contains the action</param>
        /// <param name="actionIndex">The index of the Actions Array in the FSMState.</param>
        /// <returns>FSMStateAction of type T</returns>
        public static T GetAction<T>(this PlayMakerFSM fsm, int stateIndex, int actionIndex) where T : FsmStateAction =>
            fsm.GetState(stateIndex).GetAction<T>(actionIndex);
        /// <summary>Insert an action into the state.</summary>
        /// <param name="state">State to insert the action to.</param>
        /// <param name="actionIndex">The index the action should be inserted to.</param>
        /// <param name="action">The action that will be inserted.</param>
        public static void InsertAction(this FsmState state, int actionIndex, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Insert(actionIndex, action);
            state.Actions = actions.ToArray();
        }
        /// <summary>Insert an action into the state.</summary>
        /// <param name="fsm">PlayMakerFSM to insert the action to.</param>
        /// <param name="stateName">Name of the state to insert the action to.</param>
        /// <param name="actionIndex">The index the action should be inserted to.</param>
        /// <param name="action">The action that will be inserted.</param>
        public static void InsertAction(this PlayMakerFSM fsm, string stateName, int actionIndex, FsmStateAction action) =>
            fsm.GetState(stateName).InsertAction(actionIndex, action);
        /// <summary>Insert an action into the state.</summary>
        /// <param name="fsm">PlayMakerFSM to insert the action to.</param>
        /// <param name="stateIndex">Index of the state to insert the action to.</param>
        /// <param name="actionIndex">The index the action should be inserted to.</param>
        /// <param name="action">The action that will be inserted.</param>
        public static void InsertAction(this PlayMakerFSM fsm, int stateIndex, int actionIndex, FsmStateAction action) =>
            fsm.GetState(stateIndex).InsertAction(actionIndex, action);
        /// <summary>Add an action the state.</summary>
        /// <param name="state">State to add the action to.</param>
        /// <param name="action">The action that will be added.</param>
        public static void AddAction(this FsmState state, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            actions.Add(action);
            state.Actions = actions.ToArray();
        }
        /// <summary>Add an action the state.</summary>
        /// <param name="fsm">PlayMakerFSM to add the action to.</param>
        /// <param name="stateName">Name of the state to add the action to</param>
        /// <param name="action">The action that will be added.</param>
        public static void AddAction(this PlayMakerFSM fsm, string stateName, FsmStateAction action) => 
            fsm.GetState(stateName).AddAction(action);
        /// <summary>Add an action the state.</summary>
        /// <param name="fsm">PlayMakerFSM to add the action to.</param>
        /// <param name="stateIndex">Index of the state to add the action to</param>
        /// <param name="action">The action that will be added.</param>
        public static void AddAction(this PlayMakerFSM fsm, int stateIndex, FsmStateAction action) => 
            fsm.GetState(stateIndex).AddAction(action);
        /// <summary>Replaces an action in the specified state.</summary>
        /// <param name="state">State to replace the action in.</param>
        /// <param name="actionIndex">Index of the action to replace.</param>
        /// <param name="action">The replacement action.</param>
        public static void ReplaceAction(this FsmState state, int actionIndex, FsmStateAction action)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            if (actionIndex < 0 && actionIndex >= actions.Count) throw new ArgumentOutOfRangeException();
            actions[actionIndex] = action;
            state.Actions = actions.ToArray();
        }
        /// <summary>Replaces an action in the specified state.</summary>
        /// <param name="fsm">PlayMakerFSM to replace the action on.</param>
        /// <param name="stateName">Name of the state to replace the action in.</param>
        /// <param name="actionIndex">Index of the action to replace.</param>
        /// <param name="action">The replacement action.</param>
        public static void ReplaceAction(this PlayMakerFSM fsm, string stateName, int actionIndex, FsmStateAction action) => 
            fsm.GetState(stateName).ReplaceAction(actionIndex, action);
        /// <summary>Replaces an action in the specified state.</summary>
        /// <param name="fsm">PlayMakerFSM to replace the action on.</param>
        /// <param name="stateIndex">Index of the state to replace the action in.</param>
        /// <param name="actionIndex">Index of the action to replace.</param>
        /// <param name="action">The replacement action.</param>
        public static void ReplaceAction(this PlayMakerFSM fsm, int stateIndex, int actionIndex, FsmStateAction action) => 
            fsm.GetState(stateIndex).ReplaceAction(actionIndex, action);
        /// <summary>Removes an action in the specified state.</summary>
        /// <param name="state">State to remove the action in.</param>
        /// <param name="actionIndex">Index of the action to remove.</param>
        public static void RemoveAction(this FsmState state, int actionIndex)
        {
            List<FsmStateAction> actions = state.Actions.ToList();
            if (actionIndex < 0 && actionIndex >= actions.Count) throw new ArgumentOutOfRangeException();
            actions.RemoveAt(actionIndex);
            state.Actions = actions.ToArray();
        }
        /// <summary>Removes an action in the specified state.</summary>
        /// <param name="state">State to remove the action in.</param>
        /// <param name="action">Action to remove.</param>
        public static void RemoveAction(this FsmState state, FsmStateAction action) => 
            state.RemoveAction(Array.IndexOf(state.Actions, action));
        /// <summary>Removes an action in the specified state.</summary>
        /// <param name="fsm">PlayMakerFSM to remove an action on.</param>
        /// <param name="stateName">Name of the state to remove the action in.</param>
        /// <param name="actionIndex">Index of the action to remove.</param>
        public static void RemoveAction(this PlayMakerFSM fsm, string stateName, int actionIndex) => 
            fsm.GetState(stateName).RemoveAction(actionIndex);
        /// <summary>Removes an action in the specified state.</summary>
        /// <param name="fsm">PlayMakerFSM to remove an action on.</param>
        /// <param name="stateIndex">Index of the state to remove the action in.</param>
        /// <param name="actionIndex">Index of the action to remove.</param>
        public static void RemoveAction(this PlayMakerFSM fsm, int stateIndex, int actionIndex) => 
            fsm.GetState(stateIndex).RemoveAction(actionIndex);
        /// <summary>Get a variable of specified type and name.</summary>
        /// <typeparam name="T">Type of variable to get. NamedVariable or sub-class (Standard PlayMaker Variable types)</typeparam>
        /// <param name="fsm">PlayMakerFSM that contains the variable.</param>
        /// <param name="name">Name of the variable to find.</param>
        /// <returns>PlayMaker variable of specified type T</returns>
        public static T GetVariable<T>(this PlayMakerFSM fsm, string name) where T : NamedVariable => 
            fsm.FsmVariables.FindVariable<T>(name);
        /// <summary>Get a global PlayMaker variable of specified type and name.</summary>
        /// <typeparam name="T">Type of variable to get. NamedVariable or sub-class (Standard PlayMaker Variable types)</typeparam>
        /// <param name="name">Name of the variable to find.</param>
        /// <returns>PlayMaker variable of specified type T</returns>
        public static T GetGlobalVariable<T>(string name) where T : NamedVariable => 
            PlayMakerGlobals.Instance.Variables.FindVariable<T>(name);
        /// <summary>Get a variable of specified type and name.</summary>
        /// <typeparam name="T">Type of variable to get. NamedVariable or sub-class (Standard PlayMaker Variable types)</typeparam>
        /// <param name="variables">Variables to search.</param>
        /// <param name="name">Name of the variable to find.</param>
        /// <returns>PlayMaker variable of specified type T</returns>
        public static T FindVariable<T>(this FsmVariables variables, string name) where T : NamedVariable
        {
            switch (typeof(T))
            {
                case Type _ when typeof(T) == typeof(FsmFloat): return variables.FindFsmFloat(name) as T;
                case Type _ when typeof(T) == typeof(FsmInt): return variables.FindFsmInt(name) as T;
                case Type _ when typeof(T) == typeof(FsmBool): return variables.FindFsmBool(name) as T;
                case Type _ when typeof(T) == typeof(FsmString): return variables.FindFsmString(name) as T;
                case Type _ when typeof(T) == typeof(FsmVector2): return variables.FindFsmVector2(name) as T;
                case Type _ when typeof(T) == typeof(FsmVector3): return variables.FindFsmVector3(name) as T;
                case Type _ when typeof(T) == typeof(FsmGameObject): return variables.FindFsmGameObject(name) as T;
                case Type _ when typeof(T) == typeof(FsmMaterial): return variables.FindFsmMaterial(name) as T;
                case Type _ when typeof(T) == typeof(FsmObject): return variables.FindFsmObject(name) as T;
                default: return null;
            }
        }
    }
    /// <summary>FsmStateAction that executes an Action.</summary>
    public class CallAction : FsmStateAction
    {
        public Action actionToCall;
        /// <summary>Create a new FsmStateAction that executes provided Action.</summary>
        /// <param name="action">Action to call when state executes the FsmStateAction.</param>
        public CallAction(Action action) { actionToCall = action; }

        public override void OnEnter()
        {
            actionToCall?.Invoke();
            Finish();
        }
    }
    /// <summary>Helper Methods to work with PlayMakerArrayListProxy and PlayMakerHashTableProxy components.<br></br><br></br>Courtesy of BrennFuchS.</summary>
    public class PlayMakerProxyUtility
    {
        /// <summary>
        /// add a variable to a specified PMproxy.
        /// the variable type has to match the selected PMproxys preFillType!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PMproxy"></param>
        /// <param name="ItemToAdd"></param>
        /// <param name="ClearBeforeAdding"></param>
        public static void AddToProxy<T>(T PMproxy, object ItemToAdd, bool ClearBeforeAdding = false)
        {
            switch (PMproxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    if (ClearBeforeAdding) arrayListProxy._arrayList = new ArrayList(1);
                    AddValueToArrayList(arrayListProxy, ItemToAdd, ClearBeforeAdding);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    if (ClearBeforeAdding && hashTableProxy._hashTable.Count > 0) hashTableProxy._hashTable.Clear();
                    AddValueToHashTable(hashTableProxy, ItemToAdd);
                    break;
            }
        }
        /// <summary>
        /// add a variable to one of the PMproxy's in the GameObject.
        /// the variable type has to match the selected PMproxys preFillType!
        /// </summary>
        /// <param name="PMproxysParent"></param>
        /// <param name="wantedProxyReferenceName"></param>
        /// <param name="ItemToAdd"></param>
        /// <param name="ClearBeforeAdding"></param>       
        public static void AddToProxy(GameObject PMproxysParent, string wantedProxyReferenceName, object ItemToAdd, bool ClearBeforeAdding = false)
        {
            var wantedProxy = PMproxysParent.GetComponentsInChildren<PlayMakerCollectionProxy>().FirstOrDefault(x => x.referenceName == wantedProxyReferenceName);
            switch (wantedProxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    if (ClearBeforeAdding) arrayListProxy._arrayList = new ArrayList(1);
                    AddValueToArrayList(arrayListProxy, ItemToAdd, ClearBeforeAdding);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    if (ClearBeforeAdding && hashTableProxy._hashTable.Count > 0) hashTableProxy._hashTable.Clear();
                    AddValueToHashTable(hashTableProxy, ItemToAdd);
                    break;
            }
        }

        /// <summary>
        /// add a list of variables to a specified PMproxy.
        /// the variable types have to match the selected PMproxy's preFillType!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PMproxy"></param>
        /// <param name="ItemsToAdd"></param>
        /// <param name="ClearBeforeAdding"></param>
        public static void AddRangeToProxy<T>(T PMproxy, IEnumerable<object> ItemsToAdd, bool ClearBeforeAdding = false)
        {
            switch (PMproxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    var varList = new List<object>();
                    if (ClearBeforeAdding) arrayListProxy._arrayList = new ArrayList(arrayListProxy._arrayList.Count);
                    else if (!ClearBeforeAdding && arrayListProxy._arrayList.Count > 0) varList.AddRange(arrayListProxy._arrayList.ToArray().ToList());
                    varList.AddRange(ItemsToAdd);
                    for (var i = 0; i < varList.Count; i++) AddValueToArrayList(arrayListProxy, varList[i], ClearBeforeAdding);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    if (ClearBeforeAdding && hashTableProxy._hashTable.Count > 0) hashTableProxy._hashTable.Clear();
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToHashTable(hashTableProxy, ItemsToAdd.ToList()[i]);
                    break;
            }
        }
        /// <summary>
        /// add variables to one of the PMproxys in the GameObject.
        /// the variable types have to match the selected PMproxy's preFillType!
        /// </summary>
        /// <param name="PMproxysParent"></param>
        /// <param name="wantedProxyReferenceName"></param>
        /// <param name="ItemsToAdd"></param>
        /// <param name="ClearBeforeAdding"></param>
        public static void AddRangeToProxy(GameObject PMproxysParent, string wantedProxyReferenceName, IEnumerable<object> ItemsToAdd, bool ClearBeforeAdding = false)
        {
            var wantedProxy = PMproxysParent.GetComponentsInChildren<PlayMakerCollectionProxy>().FirstOrDefault(x => x.referenceName == wantedProxyReferenceName);
            switch (wantedProxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    var varList = new List<object>();
                    if (ClearBeforeAdding) arrayListProxy._arrayList = new ArrayList(arrayListProxy._arrayList.Count);
                    else if (!ClearBeforeAdding && arrayListProxy._arrayList.Count > 0) varList.AddRange(arrayListProxy._arrayList.ToArray().ToList());
                    varList.AddRange(ItemsToAdd);
                    for (var i = 0; i < varList.Count; i++) AddValueToArrayList(arrayListProxy, varList[i], ClearBeforeAdding);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    if (ClearBeforeAdding && hashTableProxy._hashTable.Count > 0) hashTableProxy._hashTable.Clear();
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToHashTable(hashTableProxy, ItemsToAdd.ToList()[i]);
                    break;
            }
        }

        static void AddValueToArrayList(PlayMakerArrayListProxy proxy, object item, bool clear)
        {
            try
            {
                List<AudioClip> audios = new List<AudioClip>();
                List<bool> bools = new List<bool>();
                List<Color> colors = new List<Color>();
                List<float> floats = new List<float>();
                List<GameObject> gameObjects = new List<GameObject>();
                List<int> ints = new List<int>();
                List<Material> materials = new List<Material>();
                List<Quaternion> quaternions = new List<Quaternion>();
                List<Rect> rects = new List<Rect>();
                List<string> strings = new List<string>();
                List<Texture2D> texture2Ds = new List<Texture2D>();
                List<Vector2> vector2s = new List<Vector2>();
                List<Vector3> vector3s = new List<Vector3>();

                switch (proxy.preFillType)
                {
                    case PlayMakerCollectionProxy.VariableEnum.AudioClip:
                        if (proxy._arrayList.Count > 0 && !clear) audios.AddRange(proxy._arrayList.ToArray().Select(x => (AudioClip)x));
                        audios.Add((AudioClip)item);
                        proxy._arrayList.AddRange(audios);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Bool:
                        if (proxy._arrayList.Count > 0 && !clear) bools.AddRange(proxy._arrayList.ToArray().Select(x => (bool)x));
                        bools.Add((bool)item);
                        proxy._arrayList.AddRange(bools);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Color:
                        if (proxy._arrayList.Count > 0 && !clear) colors.AddRange(proxy._arrayList.ToArray().Select(x => (Color)x));
                        colors.Add((Color)item);
                        proxy._arrayList.AddRange(colors);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Float:
                        if (proxy._arrayList.Count > 0 && !clear) floats.AddRange(proxy._arrayList.ToArray().Select(x => (float)x));
                        floats.Add((float)item);
                        proxy._arrayList.AddRange(floats);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.GameObject:
                        if (proxy._arrayList.Count > 0 && !clear) gameObjects.AddRange(proxy._arrayList.ToArray().Select(x => (GameObject)x));
                        gameObjects.Add((GameObject)item);
                        proxy._arrayList.AddRange(gameObjects);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Int:
                        if (proxy._arrayList.Count > 0 && !clear) ints.AddRange(proxy._arrayList.ToArray().Select(x => (int)x));
                        ints.Add((int)item);
                        proxy._arrayList.AddRange(ints);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Material:
                        if (proxy._arrayList.Count > 0 && !clear) materials.AddRange(proxy._arrayList.ToArray().Select(x => (Material)x));
                        materials.Add((Material)item);
                        proxy._arrayList.AddRange(materials);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Quaternion:
                        if (proxy._arrayList.Count > 0 && !clear) quaternions.AddRange(proxy._arrayList.ToArray().Select(x => (Quaternion)x));
                        quaternions.Add((Quaternion)item);
                        proxy._arrayList.AddRange(quaternions);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Rect:
                        if (proxy._arrayList.Count > 0 && !clear) rects.AddRange(proxy._arrayList.ToArray().Select(x => (Rect)x));
                        rects.Add((Rect)item);
                        proxy._arrayList.AddRange(rects);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.String:
                        if (proxy._arrayList.Count > 0 && !clear) strings.AddRange(proxy._arrayList.ToArray().Select(x => (string)x));
                        strings.Add((string)item);
                        proxy._arrayList.AddRange(strings);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Texture:
                        if (proxy._arrayList.Count > 0 && !clear) texture2Ds.AddRange(proxy._arrayList.ToArray().Select(x => (Texture2D)x));
                        texture2Ds.Add((Texture2D)item);
                        proxy._arrayList.AddRange(texture2Ds);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Vector2:
                        if (proxy._arrayList.Count > 0 && !clear) vector2s.AddRange(proxy._arrayList.ToArray().Select(x => (Vector2)x));
                        vector2s.Add((Vector2)item);
                        proxy._arrayList.AddRange(vector2s);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Vector3:
                        if (proxy._arrayList.Count > 0 && !clear) vector3s.AddRange(proxy._arrayList.ToArray().Select(x => (Vector3)x));
                        vector3s.Add((Vector3)item);
                        proxy._arrayList.AddRange(vector3s);
                        break;
                }
            }
            catch (Exception ex)
            {
                ModConsole.LogError(ex.ToString());
            }
        }
        static void AddValueToHashTable(PlayMakerHashTableProxy proxy, object item)
        {
            try
            {
                switch (proxy.preFillType)
                {
                    case PlayMakerCollectionProxy.VariableEnum.AudioClip:
                        proxy._hashTable.Add(proxy._hashTable.Count, (AudioClip)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Bool:
                        proxy._hashTable.Add(proxy._hashTable.Count, (bool)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Color:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Color)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Float:
                        proxy._hashTable.Add(proxy._hashTable.Count, (float)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.GameObject:
                        proxy._hashTable.Add(proxy._hashTable.Count, (GameObject)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Int:
                        proxy._hashTable.Add(proxy._hashTable.Count, (int)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Material:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Material)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Quaternion:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Quaternion)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Rect:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Rect)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.String:
                        proxy._hashTable.Add(proxy._hashTable.Count, (string)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Texture:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Texture2D)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Vector2:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Vector2)item);
                        break;

                    case PlayMakerCollectionProxy.VariableEnum.Vector3:
                        proxy._hashTable.Add(proxy._hashTable.Count, (Vector3)item);
                        break;
                }
            }
            catch (Exception ex)
            {
                ModConsole.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// add a variable to a specified PMproxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PMproxy"></param>
        /// <param name="ItemToAdd"></param>
        public static void AddToPrefill<T>(T PMproxy, object ItemToAdd)
        {
            switch (PMproxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    AddValueToPrefill(arrayListProxy, ItemToAdd);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    AddValueToPrefill(hashTableProxy, ItemToAdd);
                    break;
            }
        }
        /// <summary>
        /// add a variable to one of the PMproxy's in the GameObject.
        /// </summary>
        /// <param name="PMproxysParent"></param>
        /// <param name="wantedProxyReferenceName"></param>
        /// <param name="ItemToAdd"></param>
        public static void AddToPrefill(GameObject PMproxysParent, string wantedProxyReferenceName, object ItemToAdd)
        {
            var wantedProxy = PMproxysParent.GetComponentsInChildren<PlayMakerCollectionProxy>().FirstOrDefault(x => x.referenceName == wantedProxyReferenceName);
            switch (wantedProxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    AddValueToPrefill(arrayListProxy, ItemToAdd);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    AddValueToPrefill(hashTableProxy, ItemToAdd);
                    break;
            }
        }
        /// <summary>
        /// add a list of variables to a specified PMproxy.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="PMproxy"></param>
        /// <param name="ItemsToAdd"></param>
        public static void AddRangeToPrefill<T>(T PMproxy, IEnumerable<object> ItemsToAdd)
        {
            switch (PMproxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToPrefill(arrayListProxy, ItemsToAdd.ToList()[i]);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToPrefill(hashTableProxy, ItemsToAdd.ToList()[i]);
                    break;
            }
        }
        /// <summary>add a list of variables to one of the PMproxy's in the GameObject.</summary>
        /// <param name="PMproxysParent"></param>
        /// <param name="wantedProxyReferenceName"></param>
        /// <param name="ItemsToAdd"></param>
        public static void AddRangeToPrefill(GameObject PMproxysParent, string wantedProxyReferenceName, IEnumerable<object> ItemsToAdd)
        {
            var wantedProxy = PMproxysParent.GetComponentsInChildren<PlayMakerCollectionProxy>().FirstOrDefault(x => x.referenceName == wantedProxyReferenceName);
            switch (wantedProxy)
            {
                case PlayMakerArrayListProxy arrayListProxy:
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToPrefill(arrayListProxy, ItemsToAdd.ToList()[i]);
                    break;
                case PlayMakerHashTableProxy hashTableProxy:
                    for (var i = 0; i < ItemsToAdd.Count(); i++) AddValueToPrefill(hashTableProxy, ItemsToAdd.ToList()[i]);
                    break;
            }
        }

        static void AddValueToPrefill(PlayMakerCollectionProxy proxy, object item)
        {
            switch (item)
            {
                case AudioClip _AudioClip:
                    proxy.preFillAudioClipList.Add(_AudioClip);
                    proxy.preFillCount++;
                    break;
                case bool _bool:
                    proxy.preFillBoolList.Add(_bool);
                    proxy.preFillCount++;
                    break;
                case Color _Color:
                    proxy.preFillColorList.Add(_Color);
                    proxy.preFillCount++;
                    break;
                case float _float:
                    proxy.preFillFloatList.Add(_float);
                    proxy.preFillCount++;
                    break;
                case GameObject _GameObject:
                    proxy.preFillGameObjectList.Add(_GameObject);
                    proxy.preFillCount++;
                    break;
                case int _int:
                    proxy.preFillIntList.Add(_int);
                    proxy.preFillCount++;
                    break;
                case Material _Material:
                    proxy.preFillMaterialList.Add(_Material);
                    proxy.preFillCount++;
                    break;
                case Quaternion _Quaternion:
                    proxy.preFillQuaternionList.Add(_Quaternion);
                    proxy.preFillCount++;
                    break;
                case Rect _Rect:
                    proxy.preFillRectList.Add(_Rect);
                    proxy.preFillCount++;
                    break;
                case string _string:
                    proxy.preFillStringList.Add(_string);
                    proxy.preFillCount++;
                    break;
                case Texture2D _Texture2D:
                    proxy.preFillTextureList.Add(_Texture2D);
                    proxy.preFillCount++;
                    break;
                case Vector2 _Vector2:
                    proxy.preFillVector2List.Add(_Vector2);
                    proxy.preFillCount++;
                    break;
                case Vector3 _Vector3:
                    proxy.preFillVector3List.Add(_Vector3);
                    proxy.preFillCount++;
                    break;
            }
        }
    }
}
