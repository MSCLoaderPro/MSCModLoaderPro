using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections;
using Random = UnityEngine.Random;

#pragma warning disable CS1591
namespace MSCLoader.Helper
{
    /// <summary> Container for useful helper methods </summary>
    public static class ModHelper
    {
        /// <summary>Make the provided GameObject interactable as a object you can pick up ingame.</summary>
        /// <param name="gameObject">GameObject to make pickable</param>
        /// <param name="includeTag">Should the tag also be changed? The tag is what makes the player able to interact with the object, whereas the layer only make its name show up as a subtitle.</param>
        public static void MakePickable(this GameObject gameObject, bool includeTag = true)
        {
            if (includeTag) gameObject.tag = "PART";
            gameObject.layer = LayerMask.NameToLayer("Parts");
        }
        /// <summary>Make the provided Transform interactable as a object you can pick up ingame.</summary>
        /// <param name="transform">Transform to make pickable</param>
        /// <param name="includeTag">Should the tag also be changed? The tag is what makes the player able to interact with the object, whereas the layer only make its name show up as a subtitle.</param>
        public static void MakePickable(this Transform transform, bool includeTag = true) =>
            transform.gameObject.MakePickable(includeTag);
        /// <summary>Get a Transform child.</summary>
        /// <param name="parentPath">Hierarchy path to a parent Transform.</param>
        /// <param name="childPath">Hierarchy path from the parent to the wanted child Transform.</param>
        /// <returns>Transform with specified path.</returns>
        public static Transform GetTransform(string parentPath, string childPath) =>
            GameObject.Find(parentPath)?.transform.Find(childPath);
        /// <summary>Get a GameObject child.</summary>
        /// <param name="parentPath">Hierarchy path to a parent GameObject.</param>
        /// <param name="childPath">Hierarchy path from the parent to the wanted child GameObject.</param>
        /// <returns>GameObject with specified path.</returns>
        public static GameObject GetGameObject(string parentPath, string childPath) =>
            GameObject.Find(parentPath)?.transform.Find(childPath).gameObject;
        /// <summary>Play a MasterAudio sound from the Transform.</summary>
        /// <param name="transform">Transform to play the sound from.</param>
        /// <param name="type">Type of sound.</param>
        /// <param name="variation">Variation of sound.</param>
        /// <param name="volume">(Optional) Sound volume.</param>
        /// <param name="pitch">(Optional) Sound pitch.</param>
        public static void PlaySound3D(this Transform transform, string type, string variation, float volume = 1f, float pitch = 1f) =>
            MasterAudio.PlaySound3DAndForget(type, transform, variationName: variation, volumePercentage: volume, pitch: pitch);
        /// <summary>Play a MasterAudio sound from a Vector3 world position.</summary>
        /// <param name="vector3">Vector3 to play the sound from.</param>
        /// <param name="type">Type of sound.</param>
        /// <param name="variation">Variation of sound.</param>
        /// <param name="volume">(Optional) Sound volume.</param>
        /// <param name="pitch">(Optional) Sound pitch.</param>
        public static void PlaySound3D(this Vector3 vector3, string type, string variation, float volume = 1f, float pitch = 1f) =>
            MasterAudio.PlaySound3DAtVector3AndForget(type, vector3, variationName: variation, volumePercentage: volume, pitch: pitch);
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

        public static void AddToLayerMask(this ref LayerMask layerMask, params int[] layers)
        {
            foreach(int layer in layers) layerMask |= (1 << layer);
        }
        public static void AddToLayerMask(this ref LayerMask layerMask, params string[] layers)
        {
            foreach (string layer in layers) layerMask |= (1 << LayerMask.NameToLayer(layer));
        }
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

        public static bool IsWithinRange(this int value, int minValue, int maxValue) => (value > minValue && value < maxValue);

        public static string GetImagesFolder() => $@"{Path.GetFullPath(".")}\Images";
        public static string GetRadioFolder() => $@"{Path.GetFullPath(".")}\Radio";
        public static string GetCD1Folder() => $@"{Path.GetFullPath(".")}\CD1";
        public static string GetCD2Folder() => $@"{Path.GetFullPath(".")}\CD2";
        public static string GetCD3Folder() => $@"{Path.GetFullPath(".")}\CD3";

        public static bool StartsWithAny(this string text, params string[] values)
        {
            for (int i = 0; i < values.Length; i++)
                if (text.StartsWith(values[i])) 
                    return true;

            return false;
        }
    }
    /// <summary>Container for PlayMaker related helper and extension methods.</summary>
    public static class PlayMakerHelper
    {
        public static FsmBool FSMGUIUse { get; internal set; }
        public static FsmBool FSMGUIAssemble { get; internal set; }
        public static FsmBool FSMGUIDisassemble { get; internal set; }
        public static FsmBool FSMGUIBuy { get; internal set; }
        public static FsmBool FSMGUIDrive { get; internal set; }
        public static FsmBool FSMGUIPassenger { get; internal set; }
        public static FsmString FSMGUIInteraction { get; internal set; }
        public static FsmString FSMGUISubtitle { get; internal set; }
        public static bool GUIUse { get => FSMGUIUse.Value; set => FSMGUIUse.Value = value; }
        public static bool GUIAssemble { get => FSMGUIAssemble.Value; set => FSMGUIAssemble.Value = value; }
        public static bool GUIDisassemble { get => FSMGUIDisassemble.Value; set => FSMGUIDisassemble.Value = value; }
        public static bool GUIBuy { get => FSMGUIBuy.Value; set => FSMGUIBuy.Value = value; }
        public static bool GUIDrive { get => FSMGUIDrive.Value; set => FSMGUIDrive.Value = value; }
        public static bool GUIPassenger { get => FSMGUIPassenger.Value; set => FSMGUIPassenger.Value = value; }
        public static string GUIInteraction { get => FSMGUIInteraction.Value; set => FSMGUIInteraction.Value = value; }
        public static string GUISubtitle { get => FSMGUISubtitle.Value; set => FSMGUISubtitle.Value = value; }

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

            return null;
        }
        /// <summary>Get a state on the PlayMakerFSM.</summary>
        /// <param name="fsm">PlayMakerFSM to search on.</param>
        /// <param name="stateIndex">Index of the state to look for.</param>
        /// <returns>FsmState of specified index.</returns>
        public static FsmState GetState(this PlayMakerFSM fsm, int stateIndex)
        {
            if (fsm.FsmStates.Length > stateIndex) return fsm.FsmStates[stateIndex];

            return null;
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
            FsmVariables.GlobalVariables.FindVariable<T>(name);
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
                case Type _ when typeof(T) == typeof(FsmRect): return variables.FindFsmRect(name) as T;
                case Type _ when typeof(T) == typeof(FsmQuaternion): return variables.FindFsmQuaternion(name) as T;
                case Type _ when typeof(T) == typeof(FsmColor): return variables.FindFsmColor(name) as T;
                case Type _ when typeof(T) == typeof(FsmGameObject): return variables.FindFsmGameObject(name) as T;
                case Type _ when typeof(T) == typeof(FsmMaterial): return variables.FindFsmMaterial(name) as T;
                case Type _ when typeof(T) == typeof(FsmTexture): return variables.FindFsmTexture(name) as T;
                case Type _ when typeof(T) == typeof(FsmObject): return variables.FindFsmObject(name) as T;
                default: return null;
            }
        }
        /// <summary>Initialize a PlayMakerFSM to enable editing states.</summary>
        /// <param name="fsm">PlayMakerFSM to initialize.</param>
        public static void Initialize(this PlayMakerFSM fsm) => 
            fsm.Fsm.InitData();
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
    public static class PlayMakerProxyHelper
    {
        static Dictionary<PlayMakerCollectionProxy.VariableEnum, Type> prefillDictionary = new Dictionary<PlayMakerCollectionProxy.VariableEnum, Type>()
        {
            { PlayMakerCollectionProxy.VariableEnum.AudioClip, typeof(AudioClip) },
            { PlayMakerCollectionProxy.VariableEnum.Bool, typeof(bool) },
            { PlayMakerCollectionProxy.VariableEnum.Color, typeof(Color) },
            { PlayMakerCollectionProxy.VariableEnum.Float, typeof(float) },
            { PlayMakerCollectionProxy.VariableEnum.GameObject, typeof(GameObject) },
            { PlayMakerCollectionProxy.VariableEnum.Int, typeof(int) },
            { PlayMakerCollectionProxy.VariableEnum.Material, typeof(Material) },
            { PlayMakerCollectionProxy.VariableEnum.Quaternion, typeof(Quaternion) },
            { PlayMakerCollectionProxy.VariableEnum.Rect, typeof(Rect) },
            { PlayMakerCollectionProxy.VariableEnum.String, typeof(string) },
            { PlayMakerCollectionProxy.VariableEnum.Texture, typeof(Texture) },
            { PlayMakerCollectionProxy.VariableEnum.Vector2, typeof(Vector2) },
            { PlayMakerCollectionProxy.VariableEnum.Vector3, typeof(Vector3) }
        };
        
        // ARRAYLISTPROXY
        public static PlayMakerArrayListProxy GetArrayListProxy(this GameObject gameObject, string referenceName)
        {
            PlayMakerArrayListProxy[] proxies = gameObject.GetComponents<PlayMakerArrayListProxy>();

            if (proxies == null) return null;

            for (int i = 0; i < proxies.Length; i++)
                if (proxies[i].referenceName == referenceName) return proxies[i];

            return null;
        }
        public static PlayMakerArrayListProxy GetArrayListProxy(this Transform transform, string referenceName) =>
            transform.gameObject.GetArrayListProxy(referenceName); 
        public static void Add(this PlayMakerArrayListProxy proxy, object item, bool clear = false)
        {
            if (item.GetType() != prefillDictionary[proxy.preFillType])
                throw new Exception("The item you're trying to add has the incorrect type.");

            if (clear) proxy.Clear();
            proxy._arrayList.Add(item);
        }
        public static void Add(this PlayMakerArrayListProxy proxy, IEnumerable<object> items, bool clear = false)
        {
            if (items.Any(item => item.GetType() != prefillDictionary[proxy.preFillType]))
                throw new Exception("One or more of the items you're trying to add have the incorrect type.");

            if (clear) proxy.Clear();
            proxy._arrayList.AddRange(items.ToList());
        }       
        public static void Clear(this PlayMakerArrayListProxy proxy, bool clearPrefill = false)
        {
            proxy._arrayList = new ArrayList(0);
            if (clearPrefill) proxy.ClearPrefill();
        }
        public static void AddPrefill(this PlayMakerArrayListProxy proxy, object item, bool clear = false)
        {
            if (item.GetType() != prefillDictionary[proxy.preFillType])
                throw new Exception("The item you're trying to add has the incorrect type.");

            if (clear)
            {
                proxy.preFillCount = 0;
                proxy.cleanPrefilledLists();
            }

            proxy.preFillCount++;
            switch (item)
            {
                case AudioClip _AudioClip: proxy.preFillAudioClipList.Add(_AudioClip); break;
                case bool _bool: proxy.preFillBoolList.Add(_bool); break;
                case Color _Color: proxy.preFillColorList.Add(_Color); break;
                case float _float: proxy.preFillFloatList.Add(_float); break;
                case GameObject _GameObject: proxy.preFillGameObjectList.Add(_GameObject); break;
                case int _int: proxy.preFillIntList.Add(_int); break;
                case Material _Material: proxy.preFillMaterialList.Add(_Material); break;
                case Quaternion _Quaternion: proxy.preFillQuaternionList.Add(_Quaternion); break;
                case Rect _Rect: proxy.preFillRectList.Add(_Rect); break;
                case string _string: proxy.preFillStringList.Add(_string); break;
                case Texture2D _Texture2D: proxy.preFillTextureList.Add(_Texture2D); break;
                case Vector2 _Vector2: proxy.preFillVector2List.Add(_Vector2); break;
                case Vector3 _Vector3: proxy.preFillVector3List.Add(_Vector3); break;
            }
        }
        public static void AddPrefill(this PlayMakerArrayListProxy proxy, IEnumerable<object> items, bool clear = false)
        {
            if (items.Any(item => item.GetType() != prefillDictionary[proxy.preFillType]))
                throw new Exception("One or more of the items you're trying to add have the incorrect type.");

            if (clear)
            {
                proxy.preFillCount = 0;
                proxy.cleanPrefilledLists();
            }

            proxy.preFillCount += items.Count();
            switch (items.ToList()[0])
            {
                case AudioClip _AudioClip: proxy.preFillAudioClipList.AddRange(items.Select(x => (AudioClip)x)); break;
                case bool _bool: proxy.preFillBoolList.AddRange(items.Select(x => (bool)x)); break;
                case Color _Color: proxy.preFillColorList.AddRange(items.Select(x => (Color)x)); break;
                case float _float: proxy.preFillFloatList.AddRange(items.Select(x => (float)x)); break;
                case GameObject _GameObject: proxy.preFillGameObjectList.AddRange(items.Select(x => (GameObject)x)); break;
                case int _int: proxy.preFillIntList.AddRange(items.Select(x => (int)x)); break;
                case Material _Material: proxy.preFillMaterialList.AddRange(items.Select(x => (Material)x)); break;
                case Quaternion _Quaternion: proxy.preFillQuaternionList.AddRange(items.Select(x => (Quaternion)x)); break;
                case Rect _Rect: proxy.preFillRectList.AddRange(items.Select(x => (Rect)x)); break;
                case string _string: proxy.preFillStringList.AddRange(items.Select(x => (string)x)); break;
                case Texture2D _Texture2D: proxy.preFillTextureList.AddRange(items.Select(x => (Texture2D)x)); break;
                case Vector2 _Vector2: proxy.preFillVector2List.AddRange(items.Select(x => (Vector2)x)); break;
                case Vector3 _Vector3: proxy.preFillVector3List.AddRange(items.Select(x => (Vector3)x)); break;
            }
        }
        public static void ClearPrefill(this PlayMakerArrayListProxy proxy)
        {
            proxy.preFillCount = 0;
            proxy.cleanPrefilledLists();
        }

        // HASHTABLEPROXY
        public static PlayMakerHashTableProxy GetHashTableProxy(this GameObject gameObject, string referenceName)
        {
            PlayMakerHashTableProxy[] proxies = gameObject.GetComponents<PlayMakerHashTableProxy>();

            if (proxies == null) return null;

            for (int i = 0; i < proxies.Length; i++)
                if (proxies[i].referenceName == referenceName) return proxies[i];

            return null;
        }
        public static PlayMakerHashTableProxy GetHashTableProxy(this Transform transform, string referenceName) =>
            transform.gameObject.GetHashTableProxy(referenceName);
        public static void Add(this PlayMakerHashTableProxy proxy, string key, object item, bool clear = false)
        {
            if (item.GetType() != prefillDictionary[proxy.preFillType])
                throw new Exception("The item you're trying to add has the incorrect type.");

            if (clear) proxy.Clear();
            proxy.hashTable.Add(key, item);
        }
        public static void Add(this PlayMakerHashTableProxy proxy, IEnumerable<string> keys, IEnumerable<object> items, bool clear = false)
        {
            if (items.Any(item => item.GetType() != prefillDictionary[proxy.preFillType]))
                throw new Exception("The item you're trying to add has the incorrect type.");

            if (clear) proxy.Clear();

            List<string> keyList = keys.ToList();
            List<object> itemList = items.ToList();

            for (int i = 0; i < keyList.Count; i++)
                proxy.hashTable.Add(keyList[i], itemList[i]);
        }
        public static void Clear(this PlayMakerHashTableProxy proxy, bool clearPrefill = false)
        {
            proxy._hashTable = new Hashtable();
            if (clearPrefill) proxy.ClearPrefill();
        }
        public static void AddPrefill(this PlayMakerHashTableProxy proxy, string key, object item, bool clear = false)
        {
            if (item.GetType() != prefillDictionary[proxy.preFillType])
                throw new Exception("The item you're trying to add has the incorrect type.");

            if (clear)
            {
                proxy.preFillCount = 0;
                proxy.cleanPrefilledLists();
            }

            proxy.preFillCount++;
            proxy.preFillKeyList.Add(key);
            switch (item)
            {
                case AudioClip _AudioClip: proxy.preFillAudioClipList.Add(_AudioClip); break;
                case bool _bool: proxy.preFillBoolList.Add(_bool); break;
                case Color _Color: proxy.preFillColorList.Add(_Color); break;
                case float _float: proxy.preFillFloatList.Add(_float); break;
                case GameObject _GameObject: proxy.preFillGameObjectList.Add(_GameObject); break;
                case int _int: proxy.preFillIntList.Add(_int); break;
                case Material _Material: proxy.preFillMaterialList.Add(_Material); break;
                case Quaternion _Quaternion: proxy.preFillQuaternionList.Add(_Quaternion); break;
                case Rect _Rect: proxy.preFillRectList.Add(_Rect); break;
                case string _string: proxy.preFillStringList.Add(_string); break;
                case Texture2D _Texture2D: proxy.preFillTextureList.Add(_Texture2D); break;
                case Vector2 _Vector2: proxy.preFillVector2List.Add(_Vector2); break;
                case Vector3 _Vector3: proxy.preFillVector3List.Add(_Vector3); break;
            }
        }
        public static void AddPrefill(this PlayMakerHashTableProxy proxy, IEnumerable<string> keys, IEnumerable<object> items, bool clear = false)
        {
            if (items.Any(item => item.GetType() != prefillDictionary[proxy.preFillType]))
                throw new Exception("One or more of the items you're trying to add have the incorrect type.");

            if (clear)
            {
                proxy.preFillCount = 0;
                proxy.cleanPrefilledLists();
            }

            proxy.preFillCount += items.Count();
            proxy.preFillKeyList.AddRange(keys);
            switch (items.ToList()[0])
            {
                case AudioClip _AudioClip: proxy.preFillAudioClipList.AddRange(items.Select(x => (AudioClip)x)); break;
                case bool _bool: proxy.preFillBoolList.AddRange(items.Select(x => (bool)x)); break;
                case Color _Color: proxy.preFillColorList.AddRange(items.Select(x => (Color)x)); break;
                case float _float: proxy.preFillFloatList.AddRange(items.Select(x => (float)x)); break;
                case GameObject _GameObject: proxy.preFillGameObjectList.AddRange(items.Select(x => (GameObject)x)); break;
                case int _int: proxy.preFillIntList.AddRange(items.Select(x => (int)x)); break;
                case Material _Material: proxy.preFillMaterialList.AddRange(items.Select(x => (Material)x)); break;
                case Quaternion _Quaternion: proxy.preFillQuaternionList.AddRange(items.Select(x => (Quaternion)x)); break;
                case Rect _Rect: proxy.preFillRectList.AddRange(items.Select(x => (Rect)x)); break;
                case string _string: proxy.preFillStringList.AddRange(items.Select(x => (string)x)); break;
                case Texture2D _Texture2D: proxy.preFillTextureList.AddRange(items.Select(x => (Texture2D)x)); break;
                case Vector2 _Vector2: proxy.preFillVector2List.AddRange(items.Select(x => (Vector2)x)); break;
                case Vector3 _Vector3: proxy.preFillVector3List.AddRange(items.Select(x => (Vector3)x)); break;
            }
        }
        public static void ClearPrefill(this PlayMakerHashTableProxy proxy)
        {
            proxy.preFillCount = 0;
            proxy.cleanPrefilledLists();
        }
    }
}
