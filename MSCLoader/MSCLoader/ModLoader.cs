using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary> Enumeration of the game's scenes.</summary>
    public enum CurrentScene { MainMenu, Game, NewGameIntro }

    /// <summary></summary>
    public class ModLoader : MonoBehaviour
    {
        /// <summary> Current Mod Loader Version. </summary>
        public static readonly string Version = "1.0";
        internal static string ModsFolder = $@"Mods";
        internal static string AssetsFolder = $@"{ModsFolder}\Assets";
        internal static string SettingsFolder = $@"{ModsFolder}\Settings";
        /// <summary> List of Loaded Mods. </summary>
        public static List<Mod> LoadedMods { get; internal set; }
        /// <summary> List of used Mod Class methods. </summary>
        public static List<List<Mod>> ModMethods;
        static string[] methodNames = { "OnNewGame", "OnMenuLoad", "MenuOnGUI", "MenuUpdate", "MenuFixedUpdate", "PreLoad", "OnLoad", "PostLoad", "OnGUI", "Update", "FixedUpdate", "OnSave" };
        /// <summary>Load handler for the UI. Add your GameObject to the extra list if you want your UI to be disabled when the game loads a scene.</summary>
        public UILoadHandler modSceneLoadHandler;

        internal static ModLoader modLoaderInstance;
        internal static ModUnloader modUnloader;
        internal static ModLoaderSettings modLoaderSettings;
        internal static ModContainer modContainer;

        internal static string modLoaderURL = "https://www.youtube.com/watch?v=DLzxrzFCyOs";

        Stopwatch timer;
        static bool loaderInitialized = false;
        internal static bool unloading = false, mainMenuReturn = false;

        GameObject modUILoadScreen;
        internal bool newGameStarted = false, vSyncEnabled = false;
        GUISkin modLoaderSkin;
        /// <summary> Get the current game scene. </summary>
        public static CurrentScene CurrentScene { get; internal set; }
        /// <summary>Get the settings folder path for a specific mod.</summary>
        /// <param name="mod">The mod you want to get the settings folder path for.</param>
        /// <param name="create">(Optional) Should the folder be created if it doesn't exist?</param>
        public static string GetModSettingsFolder(Mod mod, bool create = true)
        {
            string path = Path.Combine(SettingsFolder, mod.ID);

            if (!Directory.Exists(path) && create) Directory.CreateDirectory(path);

            return path;
        }
        /// <summary>Get the asset folder for a specific mod.</summary>
        /// <param name="mod">The mod you want to get the assets folder path for.</param>
        /// <param name="create">(Optional) Should the folder be created if it doesn't exist?</param>
        public static string GetModAssetsFolder(Mod mod, bool create = true)
        {
            string path = Path.Combine(AssetsFolder, mod.ID);

            if (!Directory.Exists(path) && create) Directory.CreateDirectory(path);

            return path;
        }
        /// <summary>Check if the specified mod ID is loaded and isn't disabled.</summary>
        /// <param name="modID">ID of the mod.</param>
        public static bool IsModPresent(string modID) => 
            LoadedMods.FirstOrDefault(mod => mod.Enabled && mod.ID == modID) != null;

        internal static void Init()
        {
            // Make sure the ModLoader isn't loaded while it is unloading.
            if (unloading) return;

            // Prevent the loader from initializing more than once.
            if (!loaderInitialized)
            {
                loaderInitialized = true;
                GameObject mscLoader = new GameObject("MSCLoader");
                modLoaderInstance = mscLoader.AddComponent<ModLoader>();
                DontDestroyOnLoad(mscLoader);
            }
        }

        void Awake()
        {
            // Create the stopwatch for method execution time.
            timer = new Stopwatch();

            // Prevent the menu music from being heard a split second when the loader is initializing.
            if (GameObject.Find("Music") && Application.loadedLevelName == "MainMenu")
                GameObject.Find("Music").GetComponent<AudioSource>().Stop();

            // Create the ModUnloader.
            CreateUnloader();

            // Create the Mod UI and load the settings.
            LoadModLoaderUI();
            LoadModLoaderSettings();

            // Prepare the mod lists.
            LoadedMods = new List<Mod>();
            ModMethods = new List<List<Mod>>() {
                new List<Mod>(), // 0 - OnNewGame
                new List<Mod>(), // 1 - MenuOnLoad
                new List<Mod>(), // 2 - MenuOnGUI
                new List<Mod>(), // 3 - MenuUpdate
                new List<Mod>(), // 4 - MenuFixedUpdate
                new List<Mod>(), // 5 - PreLoad
                new List<Mod>(), // 6 - OnLoad
                new List<Mod>(), // 7 - PostLoad
                new List<Mod>(), // 8 - OnGUI
                new List<Mod>(), // 9 - Update
                new List<Mod>(), // 10 - FixedUpdate
                new List<Mod>(), // 11 - OnSave
            };
            ModConsole.Log($"MOD LOADER PRO <b>{Version}</b> READY!\n");

            // Load the mods, their references and set up mod list and mod settings windows for each of them.
            LoadReferences();
            InitializeMods();
            SetupModList();
            
            ModConsole.Log($"<b>{LoadedMods.Count}</b> MOD{(LoadedMods.Count != 1 ? "S" : "")} FOUND!");

            // Log usage of methods to output_log.txt
            string modString = "";
            for (int i = 0; i < methodNames.Length; i++)
                modString += $"\n{ModMethods[i].Count} Mod{(ModMethods[i].Count != 1 ? "s" : "")} using {methodNames[i]}.{(ModMethods[i].Count > 0 ? "\n  " : "")}{string.Join("\n  ", ModMethods[i].Select(x => x.ID).ToArray())}";
            Console.WriteLine(modString);

            // Load mod settings for each loaded mod. Then call OnMenuLoad
            LoadModsSettings();
            CallOnMenuLoad();

            // Update the mod count in the mod list.
            modContainer.UpdateModCountText();
        }

        void OnLevelWasLoaded()
        {
            switch (Application.loadedLevelName)
            {
                case "MainMenu":
                    CurrentScene = CurrentScene.MainMenu;

                    // Enable the menu music again.
                    if (GameObject.Find("Music")) GameObject.Find("Music").GetComponent<AudioSource>().Play();

                    // Check if Vsync is enabled globally, save it, then if the menu vsync setting is enabled start the vsync in the menu.
                    if (QualitySettings.vSyncCount != 0) vSyncEnabled = true;
                    if (modLoaderSettings.UseVsyncInMenu && !vSyncEnabled) QualitySettings.vSyncCount = 1;

                    // Start the unloading/reset if the game returns from the main menu after being loaded into the game once.
                    if (mainMenuReturn)
                    {
                        mainMenuReturn = false;
                        loaderInitialized = false;
                        modUnloader.Reset();
                        unloading = true;
                    }
                    break;

                case "Intro":
                    CurrentScene = CurrentScene.NewGameIntro;

                    // If the UI is active, make sure to disable it for the intro scene.
                    modSceneLoadHandler.Disable();

                    newGameStarted = true;
                    break;

                case "GAME":
                    CurrentScene = CurrentScene.Game;

                    // Disable Vsync again if it was enabled in the menu.
                    if (modLoaderSettings.UseVsyncInMenu && !vSyncEnabled) QualitySettings.vSyncCount = 0;

                    mainMenuReturn = true;

                    // Make sure the UI reacts accordingly to the game's default menus.
                    ModUI.canvasGO.transform.Find("ModMenuUIHandler").GetComponent<UIModMenuHandler>().Setup();

                    // Lastly start the mod loading.
                    StartCoroutine(LoadMods());
                    break;
            }
        }

        void CreateUnloader()
        {
            // Make sure to only create it once. Assign the modUnloader variable if it has.
            if (GameObject.Find("MSCUnloader") == null)
            {
                GameObject go = new GameObject("MSCUnloader");
                modUnloader = go.AddComponent<ModUnloader>();
                DontDestroyOnLoad(go);
            }
            else
                modUnloader = GameObject.Find("MSCUnloader").GetComponent<ModUnloader>();
        }

        void LoadModLoaderUI()
        {
            // Load the embedded bundle, create and assign the canvas.
            AssetBundle bundle = AssetBundle.CreateFromMemoryImmediate(Properties.Resources.mscloadercanvas);
            GameObject loadedObject = bundle.LoadAsset<GameObject>("MSCLoaderCanvas.prefab");
            ModUI.canvasGO = Instantiate(loadedObject);
            ModUI.canvasGO.name = "MSCLoader Canvas";
            Destroy(loadedObject);
            DontDestroyOnLoad(ModUI.canvasGO);

            // Load the prefab for prompts.
            ModUI.prompt = bundle.LoadAsset<GameObject>("ModPrompt.prefab");

            // Assign the loading screen for easy access.
            modUILoadScreen = ModUI.canvasGO.transform.Find("ModLoaderUI/ModLoadScreen").gameObject;
            // Handle the disabling of the UI when a scene is loaded. Does not apply for the menu to game loading.
            modSceneLoadHandler = ModUI.canvasGO.GetComponent<UILoadHandler>();
            // Handle disabling the UI when loading the game from the menu.
            GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            allGameObjects.FirstOrDefault(x => !x.activeSelf && x.transform.parent == null && x.name == "Loading").AddComponent<UIMainMenuLoad>().loadHandler = modSceneLoadHandler;
            allGameObjects.FirstOrDefault(x => !x.activeSelf && x.transform.parent == null && x.name == "Licence").AddComponent<UIMenuNewGameHandler>().loadHandler = modSceneLoadHandler;

            // Load a nicer looking skin for the old GUI.
            modLoaderSkin = bundle.LoadAsset<GUISkin>("ModLoaderSkin.guiskin");

            bundle.Unload(false);
        }

        void LoadModLoaderSettings()
        {
            // Get the Settings Component sloppily.
            modLoaderSettings = ModUI.canvasGO.GetComponentsInChildren<ModLoaderSettings>(true)[0];
            MSCLoader.settings.ApplySettings(modLoaderSettings);
        }
        
        void LoadReferences()
        {
            if (Directory.Exists(Path.Combine(ModsFolder, "References")))
                foreach (string file in Directory.GetFiles(Path.Combine(ModsFolder, "References"), "*.dll"))
                    Assembly.LoadFrom(file);
        }

        void InitializeMods()
        {
            foreach (string file in Directory.GetFiles(ModsFolder).Where(file => file.EndsWith(".dll")))
            {
                try
                {
                    Assembly modAssembly = Assembly.LoadFrom(file);

                    AssemblyName[] list = modAssembly.GetReferencedAssemblies();

                    // Check if the dll is referencing either the registry or Steamworks by string.
                    string fileString = File.ReadAllText(file);
                    if (fileString.Contains("RegistryKey") || fileString.Contains("Steamworks")) throw new FileLoadException("Using forbidden key phrases.");

                    // Check if the dll is referencing Steamworks.
                    if (list.Any(assembly => assembly.Name == "Assembly-CSharp-firstpass") && (fileString.Contains("Steamworks") || fileString.Contains("GetSteamID")))
                        throw new Exception("Targeting forbidden reference.");

                    foreach (Type modType in modAssembly.GetTypes().Where(type => typeof(Mod).IsAssignableFrom(type)))
                    {
                        string msVer = "";
                        AssemblyName mscLoader = list.FirstOrDefault(assembly => assembly.Name == "MSCLoader");
                        if (mscLoader != null)
                            msVer = $"{string.Join(".", mscLoader.Version.ToString().Split('.').Take(3).ToArray())}";

                        Mod mod = (Mod)Activator.CreateInstance(modType);

                        if (!LoadedMods.Contains(mod)) // Check if mod already exists and show an error if so.
                        {
                            mod.compiledVersion = msVer;
                            mod.fileName = file;
                            LoadedMods.Add(mod);

                            AddToMethodLists(mod);
                        }
                        else
                            ModConsole.LogError($"<color=orange><b>Mod with ID: <color=red>{mod.ID}</color> already loaded:</color></b>");
                    }
                }
                catch (Exception e)
                {
                    ModConsole.LogError($"<b>{Path.GetFileName(file)}</b> can't be loaded as a mod. Contact the mod author and ask for help.");
                    ModConsole.LogError(e.ToString());
                    System.Console.WriteLine(e);
                }
            }
        }

        internal static void RemoveFromMethodLists(Mod mod)
        {
            // Start at 1 to make sure OnNewGame is always called regardless of the mod's enabled status.
            for (int i = 1; i < ModMethods.Count; i++)
                ModMethods[i].RemoveAll(x => x == mod);
        }

        internal static void AddToMethodLists(Mod mod)
        {
            for (int i = 0; i <= 11; i++)
            {
                if (!ModMethods[i].Contains(mod) && CheckEmptyMethod(mod, methodNames[i]))
                    ModMethods[i].Add(mod);
            }

            // Legacy methods
            if (!ModMethods[7].Contains(mod) && CheckEmptyMethod(mod, "SecondPassOnLoad")) 
                ModMethods[7].Add(mod);
            if (!ModMethods[1].Contains(mod) && CheckEmptyMethod(mod, "OnMenuLoad")) 
                ModMethods[1].Add(mod);
        }

        static bool CheckEmptyMethod(Mod mod, string methodName)
        {
            // Check if a method with the specified name is overridden in the Mod sub-class then check if it's not empty.
            MethodInfo method = mod.GetType().GetMethod(methodName);
            return (method.IsVirtual && method.DeclaringType == mod.GetType() && method.GetMethodBody().GetILAsByteArray().Length > 2);
        }

        void SetupModList()
        {
            modContainer = ModUI.canvasGO.GetComponentInChildren<ModContainer>();
            for (int i = 0; i < LoadedMods.Count; i++)
            {
                Mod mod = LoadedMods[i];
                mod.modListElement = modContainer.CreateModListElement(mod);
                mod.modSettings = modContainer.CreateModSettingWindow(mod);
            }
        }

        void LoadModsSettings()
        {
            MethodTimerStart("ModSettings");

            foreach (Mod mod in LoadedMods)
            {
                try
                {
                    mod.modSettings.LoadSettings();
                    mod.ModSettings();
                }
                catch (Exception e)
                {
                    ModConsole.LogError($"Settings error for mod <b>{mod.ID}</b>.\n{e.Message}");
                    ModConsole.LogError(e.ToString());
                    Console.WriteLine(e);
                }
            }

            MethodTimerStop("ModSettings");

            MethodTimerStart("ModSettingsLoaded");

            foreach (Mod mod in LoadedMods)
            {
                try { mod.ModSettingsLoaded(); }
                catch (Exception e)
                {
                    ModConsole.LogError($"Settings error for mod <b>{mod.ID}</b>.\n{e.Message}");
                    ModConsole.LogError(e.ToString());
                    Console.WriteLine(e);
                }
            }

            MethodTimerStop("ModSettingsLoaded");
        }

        void CallOnMenuLoad()
        {
            if (ModMethods[1].Count > 0)
            {
                MethodTimerStart("MenuOnLoad");

                for (int i = 0; i < ModMethods[1].Count; i++)
                {
                    try { ModMethods[1][i].MenuOnLoad(); }
                    catch (Exception e) { LogError(e, ModMethods[1][i]); }
                }
                MethodTimerStop("MenuOnLoad");
            }

            GameObject menuMethods = new GameObject("ModLoaderMenuMethods");

            menuMethods.AddComponent<ModMenuOnGUICall>().modLoader = this;
            menuMethods.AddComponent<ModMenuUpdateCall>().modLoader = this;
            menuMethods.AddComponent<ModMenuFixedUpdateCall>().modLoader = this;
        }

        IEnumerator LoadMods()
        {
            modUILoadScreen.SetActive(true);

            ModConsole.Log("<color=green>Loading mods...</color><color=#787878>\n");

            if (newGameStarted && ModMethods[0].Count > 0)
            {
                MethodTimerStart("OnNewGame");

                for (int i = 0; i < ModMethods[0].Count; i++)
                {
                    try { ModMethods[0][i].OnNewGame(); }
                    catch (Exception e) { LogError(e, ModMethods[0][i]); }
                }

                newGameStarted = false;

                MethodTimerStop("OnNewGame");

                yield return null;
            }

            if (ModMethods[5].Count > 0)
            {
                MethodTimerStart("PreLoad");

                for (int i = 0; i < ModMethods[5].Count; i++)
                {
                    try { ModMethods[5][i].PreLoad(); }
                    catch (Exception e) { LogError(e, ModMethods[5][i]); }
                }

                MethodTimerStop("PreLoad");

                yield return null;
            }

            while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null) yield return null;

            if (ModMethods[6].Count > 0)
            {
                yield return null;

                MethodTimerStart("OnLoad");

                for (int i = 0; i < ModMethods[6].Count; i++)
                {
                    try { ModMethods[6][i].OnLoad(); }
                    catch (Exception e) { LogError(e, ModMethods[6][i]); }
                }

                MethodTimerStop("OnLoad");
            }

            if (ModMethods[7].Count > 0)
            {
                yield return null;

                MethodTimerStart("PostLoad");

                for (int i = 0; i < ModMethods[7].Count; i++)
                {
                    try { ModMethods[7][i].PostLoad(); }
                    catch (Exception e) { LogError(e, ModMethods[7][i]); }
                }

                MethodTimerStop("PostLoad");
            }

            GameObject methods = new GameObject("ModLoaderMethods");
            methods.AddComponent<ModOnGUICall>().modLoader = this;
            methods.AddComponent<ModUpdateCall>().modLoader = this;
            methods.AddComponent<ModFixedUpdateCall>().modLoader = this;

            FsmHook.FsmInject(GameObject.Find("ITEMS"), "Save game", ModOnSave);

            ModConsole.controller.AppendLogLine("</color>");

            modUILoadScreen.SetActive(false);
        }

        void LogError(Exception e, Mod mod)
        {
            StackFrame frame = new StackTrace(e, true).GetFrame(0);

            ModConsole.LogError($"<b>{mod.ID}</b>! <b>Details:</b>\n{e.Message} in <b>{frame.GetMethod()}</b>.");
            //ModConsole.LogError(e.ToString());
            System.Console.WriteLine(e);
        }

        void MethodTimerStart(string message)
        {
            timer.Reset();
            timer.Start();
            ModConsole.Log($"\n<color=green>{message}()..</color>");
        }

        void MethodTimerStop(string message)
        {
            timer.Stop();
            ModConsole.Log($"<color=green>{message}() Done! ({timer.ElapsedMilliseconds}ms)</color>");
        }

        // Below Methods handle the various recurring methods in the mod class.
        internal void ModMenuOnGUI()
        {
            GUI.skin = modLoaderSkin;
            for (int i = 0; i < ModMethods[2].Count; i++)
            {
                try { ModMethods[2][i].MenuOnGUI(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModMenuUpdate()
        {
            for (int i = 0; i < ModMethods[3].Count; i++)
            {
                try { ModMethods[3][i].MenuUpdate(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModMenuFixedUpdate()
        {
            for (int i = 0; i < ModMethods[4].Count; i++)
            {
                try { ModMethods[4][i].MenuFixedUpdate(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModOnGUI()
        {
            GUI.skin = modLoaderSkin;
            for (int i = 0; i < ModMethods[8].Count; i++)
            {
                try { ModMethods[8][i].OnGUI(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModUpdate()
        {
            for (int i = 0; i < ModMethods[9].Count; i++)
            {
                try { ModMethods[9][i].Update(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModFixedUpdate()
        {
            for (int i = 0; i < ModMethods[10].Count; i++)
            {
                try { ModMethods[10][i].FixedUpdate(); }
                catch (Exception e) { Console.WriteLine(e); }
            }
        }

        internal void ModOnSave()
        {
            MethodTimerStart("OnSave");

            for (int i = 0; i < ModMethods[11].Count; i++)
            {
                try { ModMethods[11][i].OnSave(); }
                catch (Exception e) { LogError(e, ModMethods[11][i]); }
            }

            MethodTimerStop("OnSave");
        }

        // LEGACY
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Does not do anything.")]
        public static bool CheckSteam() => true;
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static string steamID = "NOYOUDONT";
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use ModLoaderVersion instead.")]
        public static readonly string MSCLoader_Ver = Version;
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, doesn't do anything.")]
        public static bool LogAllErrors = false;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool CheckIfExperimental()
        {
            try
            {
                return Steamworks.SteamApps.GetCurrentBetaName(out string Name, 128) && Name != "default_32bit";
            }
            catch
            {
                return false;
            }
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use GetModSettingsFolder() instead.")]
        public static string GetModConfigFolder(Mod mod) => GetModSettingsFolder(mod);
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated, use ModLoader.CurrentScene instead.")]
        public static CurrentScene GetCurrentScene() => CurrentScene;
    }

    class ModMenuOnGUICall : MonoBehaviour
    {
        public ModLoader modLoader;
        void OnGUI() => modLoader.ModMenuOnGUI();
    }

    class ModMenuUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void Update() => modLoader.ModMenuUpdate();
    }

    class ModMenuFixedUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void FixedUpdate() => modLoader.ModMenuFixedUpdate();
    }

    class ModOnGUICall : MonoBehaviour
    {
        public ModLoader modLoader;
        void OnGUI() => modLoader.ModOnGUI();
    }

    class ModUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void Update() => modLoader.ModUpdate();
    }

    class ModFixedUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void FixedUpdate() => modLoader.ModFixedUpdate();
    }
}