using HutongGames.PlayMaker;
using MSCLoader.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#pragma warning disable CS1591, CS0618
namespace MSCLoader
{
    /// <summary> Enumeration of the game's scenes.</summary>
    public enum CurrentScene { MainMenu, Game, NewGameIntro, Ending }

    /// <summary></summary>
    public class ModLoader : MonoBehaviour
    {
        /// <summary> Current Mod Loader Version. </summary>
        public static readonly string Version = "1.1.1";
        internal static string ModsFolder = $@"Mods";
        internal static string AssetsFolder = $@"{ModsFolder}\Assets";
        internal static string SettingsFolder = $@"{ModsFolder}\Settings";
        internal static string ReferenceFolder = $@"{ModsFolder}\References";
        /// <summary> List of Loaded Mods. </summary>
        public static List<Mod> LoadedMods { get; internal set; }
        /// <summary> List of used Mod Class methods. </summary>
        public static List<Mod>[] ModMethods { get; internal set; }
        static string[] methodNames = { "OnNewGame", "MenuOnLoad", "MenuOnGUI", "MenuUpdate", "MenuFixedUpdate", "PreLoad", "OnLoad", "PostLoad", "OnGUI", "Update", "FixedUpdate", "OnSave", "UniversalOnGUI", "UniversalUpdate", "UniversalFixedUpdate", "OnLevelLoaded" };
        internal static List<string> loadedAssemblies;
        
        
        /// <summary>Load handler for the UI. Add your GameObject to the extra list if you want your UI to be disabled when the game loads a scene.</summary>
        public static UILoadHandler modSceneLoadHandler;

        internal static ModLoader modLoaderInstance;
        internal static ModUnloader modUnloader;
        internal static ModLoaderSettings modLoaderSettings;
        internal static ModContainer modContainer;

        internal static string modLoaderURL = "https://mscloaderpro.github.io";
        /// <summary>Get current date.</summary>
        public static DateTime Date { get; internal set; }
        ///<summary>Get the mod loader canvas GameObject.</summary>
        public static Transform UICanvas { get; internal set; }

        /// <summary>Yellow Color that MSC uses.</summary>
        public static Color32 MSCYellow = new Color32(255, 255, 0, 255);
        /// <summary>Wine Red Color that MSC uses.</summary>
        public static Color32 MSCRed = new Color32(101, 34, 18, 255);
        /// <summary>Rose Color that MSC uses.</summary>
        public static Color32 MSCRose = new Color32(199, 152, 129, 255);
        /// <summary>Red Color for disabled mods.</summary>
        public static Color32 ModDisabledRed = new Color32(215, 0, 0, 255);

        static bool audioFixApplied;

        Stopwatch timer;
        static bool loaderInitialized = false;
        internal static bool unloading = false, mainMenuReturn = false, hasSaved = false;

        GameObject modUILoadScreen;
        internal bool newGameStarted = false, vSyncEnabled = false;
        GUISkin modLoaderSkin;
        /// <summary> Get the current game scene. </summary>
        public static CurrentScene CurrentScene { get; internal set; }
        /// <summary>Get the settings folder path for a specific mod.</summary>
        /// <param name="mod">The mod you want to get the settings folder path for.</param>
        /// <param name="create">(Optional) Should the folder be created if it doesn't exist?</param>
        public static string GetModSettingsFolder(Mod mod, bool create = true) => 
            GetModFolder(mod, SettingsFolder, create);
        /// <summary>Get the asset folder for a specific mod.</summary>
        /// <param name="mod">The mod you want to get the assets folder path for.</param>
        /// <param name="create">(Optional) Should the folder be created if it doesn't exist?</param>
        public static string GetModAssetsFolder(Mod mod, bool create = true) => 
            GetModFolder(mod, AssetsFolder, create);

        static string GetModFolder(Mod mod, string baseFolder, bool create)
        {
            string path = Path.Combine(baseFolder, mod.ID);
            if (!Directory.Exists(path) && create) Directory.CreateDirectory(path);
            return path;
        }
        /// <summary>Check if the specified mod ID is loaded.</summary>
        /// <param name="modID">ID of the mod.</param>
        /// <param name="ignoreEnabled">Ignore whether or not the mod is enabled.</param>
        public static Mod GetMod(string modID, bool ignoreEnabled = false)
        {
            for (int i = 0; i < LoadedMods.Count; i++)
                if (LoadedMods[i].ID == modID && (LoadedMods[i].Enabled || ignoreEnabled))
                    return LoadedMods[i];
            return null;
        }

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
            // Unpatch saving
            MSCLoader.ModLoaderInstance.UnpatchAll("ProLoaderSave");

            // Prevent the menu music from being heard a split second when the loader is initializing.
            if (GameObject.Find("Music") && Application.loadedLevelName == "MainMenu" && !audioFixApplied)
            {
                audioFixApplied = true;
                GameObject.Find("Music").GetComponent<AudioSource>().Stop();
            }

            // Get todays date from the internet or local system time
            Date = ModEarlyAccess.GetDate();

            // Setup PlayMakerHelper Global Variables
            PlayMakerHelper.FSMGUIUse = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIuse");
            PlayMakerHelper.FSMGUIAssemble = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIassemble");
            PlayMakerHelper.FSMGUIDisassemble = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIdisassemble");
            PlayMakerHelper.FSMGUIBuy = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIbuy");
            PlayMakerHelper.FSMGUIDrive = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIdrive");
            PlayMakerHelper.FSMGUIPassenger = PlayMakerHelper.GetGlobalVariable<FsmBool>("GUIpassenger");
            PlayMakerHelper.FSMGUIInteraction = PlayMakerHelper.GetGlobalVariable<FsmString>("GUIinteraction");
            PlayMakerHelper.FSMGUISubtitle = PlayMakerHelper.GetGlobalVariable<FsmString>("GUIsubtitle");

            // Create the stopwatch for method execution time.
            timer = new Stopwatch();

            // Set up Mods folder
            SetupFolders();

            // Create the ModUnloader.
            CreateUnloader();

            // Create the Mod UI and load the settings.
            LoadModLoaderUI();
            LoadModLoaderSettings();
            modLoaderSettings.nexusSSO = gameObject.AddComponent<NexusMods.NexusSSO>();

            // Prepare the mod lists.
            LoadedMods = new List<Mod>();
            ModMethods = new List<Mod>[] {
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
                new List<Mod>(), // 12 - UniversalOnGUI
                new List<Mod>(), // 13 - UniversalUpdate
                new List<Mod>(), // 14 - UniversalFixedUpdate
                new List<Mod>(), // 15 - OnLevelLoaded
            };
            ModConsole.Log($"MOD LOADER PRO <b>{Version}</b> READY!");

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

            // Log enabled status of mods to output_log.txt
            Console.WriteLine(string.Join("\n", LoadedMods.Select(x => $"{(x.Enabled ? "ENABLED" : "DISABLED")} - {x.Name}").ToArray()));

            // Load mod settings for each loaded mod. Then call OnMenuLoad
            LoadModsSettings();
            CallMenuOnLoad();

            // Prepare Universal methods
            gameObject.AddComponent<ModUniversalOnGUICall>().modLoader = this;
            gameObject.AddComponent<ModUniversalUpdateCall>().modLoader = this;
            gameObject.AddComponent<ModUniversalFixedUpdateCall>().modLoader = this;

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
                    UICanvas.Find("ModMenuUIHandler").GetComponent<UIModMenuHandler>().Setup();

                    // Lastly start the mod loading.
                    StartCoroutine(LoadMods());
                    break;

                case "Ending":
                    CurrentScene = CurrentScene.Ending;
                    break;
            }

            // Execute OnLevelLoaded for all applicable mods.
            CallOnLevelLoaded();
        }

        void SetupFolders()
        {
            ModsFolder = Path.GetFullPath(MSCLoader.settings.ModsFolderPath);
            AssetsFolder = $@"{ModsFolder}\Assets";
            SettingsFolder = $@"{ModsFolder}\Settings";
            ReferenceFolder = $@"{ModsFolder}\References";

            if (!Directory.Exists(ModsFolder)) Directory.CreateDirectory(ModsFolder);
            if (!Directory.Exists(AssetsFolder)) Directory.CreateDirectory(AssetsFolder);
            if (!Directory.Exists(SettingsFolder)) Directory.CreateDirectory(SettingsFolder);
            if (!Directory.Exists(ReferenceFolder)) Directory.CreateDirectory(ReferenceFolder);
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
            UICanvas = Instantiate(loadedObject).transform;
            UICanvas.name = "MSCLoader Canvas";
            Destroy(loadedObject);
            DontDestroyOnLoad(UICanvas);

            // Load the prefab for prompts.
            ModPrompt.prompt = bundle.LoadAsset<GameObject>("ModPrompt.prefab");
            // Load the prefab for tooltips.
            UITooltip.toolTipPrefab = bundle.LoadAsset<GameObject>("UITooltip.prefab");

            // Assign the loading screen for easy access.
            modUILoadScreen = UICanvas.Find("ModLoaderUI/ModLoadScreen").gameObject;
            // Handle the disabling of the UI when a scene is loaded. Does not apply for the menu to game loading.
            modSceneLoadHandler = UICanvas.GetComponent<UILoadHandler>();
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
            modLoaderSettings = UICanvas.GetComponentsInChildren<ModLoaderSettings>(true)[0];
            MSCLoader.settings.ApplySettings(modLoaderSettings);
        }
        
        void LoadReferences()
        {
            foreach (string file in Directory.GetFiles(ReferenceFolder, "*.dll"))
                Assembly.LoadFrom(file);
        }

        void InitializeMods()
        {
            string mscLoaderVersions = "\nAssembly MSCLoader version:\n";

            string[] files = Directory.GetFiles(ModsFolder, "*.dll");
            List<Assembly> modAssemblies = new List<Assembly>();

            for (int i = 0; i < files.Length; i++) 
                modAssemblies.Add(Assembly.LoadFrom(files[i]));

            Assembly modAssembly;
            string file;

            //ModConsole.Log(loadedAssemblies);

            for (int i = 0; i < modAssemblies.Count; i++)
            {
                modAssembly = modAssemblies[i];
                file = files[i];

                try
                {
                    AssemblyName[] referenceList = modAssembly.GetReferencedAssemblies();

                    //ModConsole.Log($"\n\n{file.Split('\\').Last()}");
                    //ModConsole.Log(referenceList.Select(x => x.Name).ToList());

                    // Check if the dll is referencing either the registry or Steamworks by string.
                    // Check if the dll is referencing Steamworks.
                    string fileString = File.ReadAllText(file);
                    if (referenceList.Any(assembly => assembly.Name == "Assembly-CSharp-firstpass") && (fileString.Contains("Steamworks") || fileString.Contains("GetSteamID") || fileString.Contains("RegistryKey") || fileString.Contains("Steamworks")))
                        throw new Exception("Targeting forbidden reference.");

                    Type[] modAssemblyTypes;
                    try
                    {
                        modAssemblyTypes = modAssembly.GetTypes();
                    }
                    catch (ReflectionTypeLoadException exception)
                    {
                        ModConsole.LogError($"<b>{Path.GetFileName(file)}</b> can't be loaded successfully. Maybe a missing mod requirement? Consult the mod's download page and look for help there.");
                        foreach (Exception loaderException in exception.LoaderExceptions) ModConsole.LogError($"{loaderException}");

                        modAssemblyTypes = exception.Types.Where(x => x != null).ToArray();
                    }

                    foreach (Type modType in modAssemblyTypes.Where(type => typeof(Mod).IsAssignableFrom(type)))
                    {
                        Mod mod = (Mod)Activator.CreateInstance(modType);

                        if (!LoadedMods.Any(x => x.ID == mod.ID)) // Check if mod already exists and show an error if so.
                            LoadedMods.Add(mod);
                        else
                            ModConsole.LogError($"Mod with ID: {mod.ID} already loaded, possible duplicate or a conflicting ID with another mod. Contact the mod author ({mod.Author}) for {mod.Name}!");
                    }

                    if (referenceList.Any(x => x.Name == "MSCLoader"))
                        mscLoaderVersions += $"{file.Split('\\').Last()}:\n    {referenceList.FirstOrDefault(x => x.Name == "MSCLoader").Version}\n";
                }
                catch (Exception exception)
                {
                    ModConsole.LogError($"<b>{Path.GetFileName(file)}</b> can't be loaded as a mod. Maybe a missing mod requirement? Consult the mod's download page and look for help there.\n{exception}");
                }
            }

            Console.Write($"{mscLoaderVersions}\n");

            // Sort the mod lists after the name of the mod rather than the file name.
            LoadedMods = LoadedMods.OrderBy(x => x.Name).ToList();

            for (int i = 0; i < LoadedMods.Count; i++)
                AddToMethodLists(LoadedMods[i]);
        }

        internal static void AddToMethodLists(Mod mod)
        {
            for (int i = 0; i < ModMethods.Length; i++)
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

        internal static void RemoveFromMethodLists(Mod mod)
        {
            // Start at 1 to make sure OnNewGame is always called regardless of the mod's enabled status.
            for (int i = 1; i < ModMethods.Length; i++)
                ModMethods[i].RemoveAll(x => x == mod);
        }

        static bool CheckEmptyMethod(Mod mod, string methodName)
        {
            // Check if a method with the specified name is overridden in the Mod sub-class then check if it's not empty.
            MethodInfo method = mod.GetType().GetMethod(methodName) ?? mod.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return (method.IsVirtual && method.DeclaringType == mod.GetType());
        }

        void SetupModList()
        {
            modContainer = UICanvas.GetComponentInChildren<ModContainer>();
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
                    Console.WriteLine(e);
                }
            }

            MethodTimerStop("ModSettings");

            MethodTimerStart("ModSettingsLoaded");

            foreach (Mod mod in LoadedMods)
            {
                try { mod.ModSettingsLoaded(); }
                catch (Exception exception)
                {
                    ModConsole.LogError($"Settings error for mod <b>{mod.ID}</b>.\n{exception.Message}");
                    Console.WriteLine(exception);
                }
            }

            MethodTimerStop("ModSettingsLoaded");
        }

        void CallMenuOnLoad()
        {
            if (ModMethods[1].Count > 0)
            {
                MethodTimerStart("MenuOnLoad");

                for (int i = 0; i < ModMethods[1].Count; i++)
                {
                    try { ModMethods[1][i].MenuOnLoad(); }
                    catch (Exception exception) { LogError(exception, ModMethods[1][i]); }
                }
                MethodTimerStop("MenuOnLoad");
            }

            GameObject menuMethods = new GameObject("ModLoaderMenuMethods");

            menuMethods.AddComponent<ModMenuOnGUICall>().modLoader = this;
            menuMethods.AddComponent<ModMenuUpdateCall>().modLoader = this;
            menuMethods.AddComponent<ModMenuFixedUpdateCall>().modLoader = this;
        }

        void CallOnLevelLoaded()
        {
            if (ModMethods != null && ModMethods[15].Count > 0)
            {
                for (int i = 0; i < ModMethods[15].Count; i++)
                {
                    try { ModMethods[15][i].OnLevelLoaded(); }
                    catch (Exception exception) { Console.WriteLine(exception); }
                }
            }
        }

        IEnumerator LoadMods()
        {
            modUILoadScreen.SetActive(true);

            // Should disable the ability to toggle mod to avoid unwanted effects.
            modContainer.DisableModToggle();

            ModConsole.Log("<color=green>Loading mods...</color>\n");

            if (newGameStarted && ModMethods[0].Count > 0)
            {
                newGameStarted = false;
                MethodTimerStart("OnNewGame");

                for (int i = 0; i < ModMethods[0].Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"ONNEWGAME: {ModMethods[0][i]}");
                        ModMethods[0][i].OnNewGame();
                    }
                    catch (Exception exception) { LogError(exception, ModMethods[0][i]); }
                }

                MethodTimerStop("OnNewGame");

                yield return null;
            }

            if (ModMethods[5].Count > 0)
            {
                MethodTimerStart("PreLoad");

                for (int i = 0; i < ModMethods[5].Count; i++)
                {
                    try 
                    {
                        Console.WriteLine($"PRELOAD: {ModMethods[5][i].ID}");
                        ModMethods[5][i].PreLoad();
                    }
                    catch (Exception exception) { LogError(exception, ModMethods[5][i]); }
                }

                MethodTimerStop("PreLoad");

                yield return null;
            }

            while (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") == null) yield return null;

            if (ModMethods[6].Count > 0)
            {
                MethodTimerStart("OnLoad");

                for (int i = 0; i < ModMethods[6].Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"ONLOAD: {ModMethods[6][i].ID}");
                        ModMethods[6][i].OnLoad();
                    }
                    catch (Exception exception) { LogError(exception, ModMethods[6][i]); }
                }

                MethodTimerStop("OnLoad");
            }

            if (ModMethods[7].Count > 0)
            {
                // Wait a few frames to give newly created MonoBehaviours and mods time to set up and call their various methods.
                yield return null;
                yield return null;

                MethodTimerStart("PostLoad");

                for (int i = 0; i < ModMethods[7].Count; i++)
                {
                    try
                    {
                        Console.WriteLine($"POSTLOAD: {ModMethods[7][i].ID}");
                        ModMethods[7][i].PostLoad();
                    }
                    catch (Exception exception) { LogError(exception, ModMethods[7][i]); }
                }

                MethodTimerStop("PostLoad");
            }

            yield return null;

            GameObject methods = new GameObject("ModLoaderMethods");
            methods.AddComponent<ModOnGUICall>().modLoader = this;
            methods.AddComponent<ModUpdateCall>().modLoader = this;
            methods.AddComponent<ModFixedUpdateCall>().modLoader = this;

            hasSaved = false;

            Harmony.HarmonyInstance saveInstance = Harmony.HarmonyInstance.Create("ProLoaderSave");

            MethodBase broadcastMethod = typeof(Fsm).GetMethod("BroadcastEvent", new Type[] { typeof(FsmEvent), typeof(bool) });
            Harmony.HarmonyMethod prefix = new Harmony.HarmonyMethod(typeof(InjectSaving).GetMethod("Prefix"));
            saveInstance.Patch(broadcastMethod, prefix);

            broadcastMethod = typeof(PlayMakerFSM).GetMethod("BroadcastEvent", new Type[] { typeof(FsmEvent) });
            prefix = new Harmony.HarmonyMethod(typeof(InjectSaving).GetMethod("Prefix"));
            saveInstance.Patch(broadcastMethod, prefix);

            //GameObject.Find("ITEMS").GetPlayMakerFSM("SaveItems").InsertAction("Save game", 0, new ModOnSave() { modLoader = this });

            modUILoadScreen.SetActive(false);
        }

        void LogError(Exception exception, Mod mod)
        {
            StackFrame frame = new StackTrace(exception, true).GetFrame(0);

            ModConsole.LogError($"<b>{mod.ID}</b>! <b>Details:</b>\n{exception.Message} in <b>{frame.GetMethod()}</b>.");
            UnityEngine.Debug.LogError(exception);
        }

        void MethodTimerStart(string message)
        {
            ModConsole.Log($"\n<color=green>{message}()..</color><color=#787878>");
            timer.Reset();
            timer.Start();
        }

        void MethodTimerStop(string message)
        {
            timer.Stop();
            ModConsole.Log($"</color><color=green>{message}() Done! ({timer.ElapsedMilliseconds}ms)</color>");
        }

        // Below Methods handle the various recurring methods in the mod class.
        internal void ModMenuOnGUI()
        {
            GUI.skin = modLoaderSkin;
            for (int i = 0; i < ModMethods[2].Count; i++)
            {
                try { ModMethods[2][i].MenuOnGUI(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }

            // Backwards Compatibility
            for (int i = 0; i < ModMethods[8].Count; i++)
            {
                try { if (ModMethods[8][i].LoadInMenu) ModMethods[8][i].OnGUI(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModMenuUpdate()
        {
            for (int i = 0; i < ModMethods[3].Count; i++)
            {
                try { ModMethods[3][i].MenuUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }

            // Backwards Compatibility
            for (int i = 0; i < ModMethods[9].Count; i++)
            {
                try { if (ModMethods[9][i].LoadInMenu) ModMethods[9][i].Update(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModMenuFixedUpdate()
        {
            for (int i = 0; i < ModMethods[4].Count; i++)
            {
                try { ModMethods[4][i].MenuFixedUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }

            // Backwards Compatibility
            for (int i = 0; i < ModMethods[10].Count; i++)
            {
                try { if (ModMethods[10][i].LoadInMenu) ModMethods[10][i].FixedUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModOnGUI()
        {
            GUI.skin = modLoaderSkin;
            for (int i = 0; i < ModMethods[8].Count; i++)
            {
                try { ModMethods[8][i].OnGUI(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModUpdate()
        {
            for (int i = 0; i < ModMethods[9].Count; i++)
            {
                try { ModMethods[9][i].Update(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModFixedUpdate()
        {
            for (int i = 0; i < ModMethods[10].Count; i++)
            {
                try { ModMethods[10][i].FixedUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModOnSave()
        {
            if (hasSaved) return;
            hasSaved = true;

            MethodTimerStart("OnSave");

            for (int i = 0; i < ModMethods[11].Count; i++)
            {
                try { ModMethods[11][i].OnSave(); }
                catch (Exception exception) { LogError(exception, ModMethods[11][i]); }
            }

            MethodTimerStop("OnSave");
        }

        internal void ModUniversalOnGUI()
        {
            GUI.skin = modLoaderSkin;
            for (int i = 0; i < ModMethods[12].Count; i++)
            {
                try { ModMethods[12][i].UniversalOnGUI(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModUniversalUpdate()
        {
            for (int i = 0; i < ModMethods[13].Count; i++)
            {
                try { ModMethods[13][i].UniversalUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        internal void ModUniversalFixedUpdate()
        {
            for (int i = 0; i < ModMethods[14].Count; i++)
            {
                try { ModMethods[14][i].UniversalFixedUpdate(); }
                catch (Exception exception) { Console.WriteLine(exception); }
            }
        }

        // LEGACY
        [Obsolete("Does not do anything."), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool CheckSteam() => true;
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal static string steamID = "12551291016475899";
        [Obsolete("Deprecated, use ModLoaderVersion instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly string MSCLoader_Ver = Version;
        [Obsolete("Deprecated, doesn't do anything."), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool LogAllErrors = false;
        [Obsolete("Deprecated, doesn't do anything."), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool CheckIfExperimental() => false;
        [Obsolete("Deprecated, use GetModSettingsFolder() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetModConfigFolder(Mod mod) => GetModSettingsFolder(mod);
        [Obsolete("Deprecated, use ModLoader.CurrentScene instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static CurrentScene GetCurrentScene() => CurrentScene;
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static string GetModAssetsFolder(Mod mod) => GetModAssetsFolder(mod, true);
        [Obsolete("Does not do anything."), EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly bool experimental = false;
        [Obsolete("Does not do anything."), EditorBrowsable(EditorBrowsableState.Never)]
        public static readonly bool devMode = false;
        [Obsolete("Deprecated, use ModLoader.GetMod() instead."), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool IsModPresent(string modID) => GetMod(modID) != null;
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

    class ModUniversalOnGUICall : MonoBehaviour
    {
        public ModLoader modLoader;
        void OnGUI() => modLoader.ModUniversalOnGUI();
    }

    class ModUniversalUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void Update() => modLoader.ModUniversalUpdate();
    }

    class ModUniversalFixedUpdateCall : MonoBehaviour
    {
        public ModLoader modLoader;
        void FixedUpdate() => modLoader.ModUniversalFixedUpdate();
    }
}