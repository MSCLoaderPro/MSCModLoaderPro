using Ionic.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace MSCLoader
{
    public enum CurrentScene
    {
        MainMenu,
        Game,
        NewGameIntro,
    }

    public class ModLoader : MonoBehaviour
    {
        public static bool LogAllErrors = false;
        public static List<Mod> LoadedMods;
        public static List<string> InvalidMods;
        public static ModLoader Instance;
        public static readonly string Version = "1.0.1";
        public static readonly bool experimental = false;

        private string expBuild = Assembly.GetExecutingAssembly().GetName().Version.Revision.ToString();
        private MSCUnloader mscUnloader;

        private static bool loaderPrepared = false;
        private static string ModsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MySummerCar\Mods");
        private static string ConfigFolder = Path.Combine(ModsFolder, @"Config\");
        private static string AssetsFolder = Path.Combine(ModsFolder, @"Assets\");

        private GameObject mainMenuInfo;
        private GameObject loading;
        private Animator menuInfoAnim;
        private GUISkin guiskin;
        private ModCore modCore;

        private bool IsDoneLoading = false;
        private bool IsModsLoading = false;
        private bool IsModsDoneLoading = false;
        private bool fullyLoaded = false;
        private bool allModsLoaded = false;
        private bool IsModsResetting = false;
        private bool IsModsDoneResetting = false;
        private static CurrentScene CurrentGameScene;

#pragma warning disable CS1591 
        public static bool unloader = false;
#pragma warning restore CS1591 

        public static bool CheckSteam()
        {
            return true;
        }

        public static CurrentScene GetCurrentScene()
        {
            return CurrentGameScene;
        }

        public static string GetModConfigFolder(Mod mod)
        {
            return Path.Combine(ConfigFolder, mod.ID);
        }

        public static string GetModAssetsFolder(Mod mod)
        {
            if (mod.UseAssetsFolder == false)
                ModConsole.Error(string.Format("<b>{0}:</b> Please set variable <b>UseAssetsFolder</b> to <b>true</b>", mod.ID));
            return Path.Combine(AssetsFolder, mod.ID);
        }

        public static void Init_MD()
        {
            if (unloader) return;
            ModsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"MySummerCar\Mods");
            PrepareModLoader();
        }
        public static void Init_GF()
        {
            if (unloader) return;
            ModsFolder = Path.GetFullPath(Path.Combine("Mods", ""));
            PrepareModLoader();
        }
        public static void Init_AD()
        {
            if (unloader) return;
            ModsFolder = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"..\LocalLow\Amistech\My Summer Car\Mods"));
            PrepareModLoader();
        }

        private static void PrepareModLoader()
        {
            if (!loaderPrepared)
            {
                loaderPrepared = true;
                GameObject go = new GameObject("MSCLoader", typeof(ModLoader));
                Instance = go.GetComponent<ModLoader>();
                DontDestroyOnLoad(go);
                Instance.Init();
            }
        }

        private void OnLevelWasLoaded(int level)
        {
            if (Application.loadedLevelName == "MainMenu")
            {
                CurrentGameScene = CurrentScene.MainMenu;
                if ((bool)ModSettings_menu.forceMenuVsync.GetValue())
                    QualitySettings.vSyncCount = 1; //vsync in menu
                if (IsDoneLoading && GameObject.Find("MSCLoader Info") == null)
                    MainMenuInfo();
                if (IsModsDoneLoading)
                {
                    loaderPrepared = false;
                    mscUnloader.MSCLoaderReset();
                    unloader = true;
                    return;
                }
            }
            else if (Application.loadedLevelName == "Intro")
            {
                CurrentGameScene = CurrentScene.NewGameIntro;

                if (!IsModsDoneResetting && !IsModsResetting)
                {
                    IsModsResetting = true;
                    StartCoroutine(NewGameMods());
                }
            }
            else if (Application.loadedLevelName == "GAME")
            {
                CurrentGameScene = CurrentScene.Game;
                if ((bool)ModSettings_menu.forceMenuVsync.GetValue())
                    QualitySettings.vSyncCount = 0;

                if (IsDoneLoading)
                    menuInfoAnim.SetBool("isHidden", true);
            }
        }

        private void StartLoadingMods()
        {
            menuInfoAnim.SetBool("isHidden", true);
            if (!IsModsDoneLoading && !IsModsLoading)
            {
                //introCheck = true;
                IsModsLoading = true;
                StartCoroutine(LoadMods());
            }
        }

        private void Init()
        {
            //Set config and Assets folder in selected mods folder
            ConfigFolder = Path.Combine(ModsFolder, @"Config\");
            AssetsFolder = Path.Combine(ModsFolder, @"Assets\");

            if (GameObject.Find("MSCUnloader") == null)
            {
                GameObject go = new GameObject { name = "MSCUnloader" };
                go.AddComponent<MSCUnloader>();
                mscUnloader = go.GetComponent<MSCUnloader>();
                DontDestroyOnLoad(go);
            }
            else
            {
                mscUnloader = GameObject.Find("MSCUnloader").GetComponent<MSCUnloader>();
            }
            if (IsDoneLoading) //Remove this.
            {

                if (Application.loadedLevelName != "MainMenu")
                    menuInfoAnim.SetBool("isHidden", true);
            }
            else
            {
                ModUI.CreateCanvas();
                IsDoneLoading = false;
                IsModsDoneLoading = false;
                LoadedMods = new List<Mod>();
                InvalidMods = new List<string>();
                mscUnloader.reset = false;
                if (!Directory.Exists(ModsFolder))
                    Directory.CreateDirectory(ModsFolder);
                if (!Directory.Exists(ConfigFolder))
                    Directory.CreateDirectory(ConfigFolder);
                if (!Directory.Exists(AssetsFolder))
                    Directory.CreateDirectory(AssetsFolder);

                LoadMod(new ModConsole(), Version);
                LoadedMods[0].ModSettings();
                LoadMod(new ModSettings_menu(), Version);
                LoadedMods[1].ModSettings();
                ModSettings_menu.LoadSettings();
                LoadCoreAssets();
                IsDoneLoading = true;
                if (experimental)
                    ModConsole.Print(string.Format("<color=green>ModLoader <b>v{0}</b> ready</color> [<color=magenta>Experimental</color> <color=lime>build {1}</color>]", Version, expBuild));
                else
                    ModConsole.Print(string.Format("<color=green>ModLoader <b>v{0}</b> ready</color>", Version));
                LoadReferences();
                PreLoadMods();
                ModConsole.Print(string.Format("<color=orange>Found <color=green><b>{0}</b></color> mods!</color>", LoadedMods.Count - 2));
                
                MainMenuInfo();
                LoadModsSettings();
            }
        }

        private void LoadReferences()
        {
            if (Directory.Exists(Path.Combine(ModsFolder, "References")))
            {
                string[] files = Directory.GetFiles(Path.Combine(ModsFolder, "References"), "*.dll");
                foreach (var file in files)
                    Assembly.LoadFrom(file);
            }
            else
                Directory.CreateDirectory(Path.Combine(ModsFolder, "References"));
        }

        private void LoadCoreAssets()
        {
            modCore = new ModCore();
            ModConsole.Print("Loading core assets...");
            AssetBundle ab = LoadAssets.LoadBundle(modCore, "core.unity3d");
            guiskin = ab.LoadAsset<GUISkin>("MSCLoader.guiskin");
            ModUI.messageBox = ab.LoadAsset<GameObject>("MSCLoader MB.prefab");
            mainMenuInfo = ab.LoadAsset<GameObject>("MSCLoader Info.prefab");
            loading = ab.LoadAsset<GameObject>("LoadingMods.prefab");
            loading.SetActive(false);
            loading = GameObject.Instantiate(loading);
            loading.transform.SetParent(GameObject.Find("MSCLoader Canvas").transform, false);
            ModConsole.Print("Loading core assets completed!");
            ab.Unload(false); //freeup memory
        }

        /// <summary>
        /// Toggle main menu path via settings
        /// </summary>
        public static void MainMenuPath()
        {
            Instance.mainMenuInfo.transform.GetChild(1).gameObject.SetActive((bool)ModSettings_menu.modPath.GetValue());
        }
        private void MainMenuInfo()
        {
            Text info, mf, modUpdates;
            mainMenuInfo = Instantiate(mainMenuInfo);
            mainMenuInfo.name = "MSCLoader Info";
            menuInfoAnim = mainMenuInfo.GetComponent<Animator>();
            menuInfoAnim.SetBool("isHidden", false);
            info = mainMenuInfo.transform.GetChild(0).gameObject.GetComponent<Text>();
            mf = mainMenuInfo.transform.GetChild(1).gameObject.GetComponent<Text>();
            modUpdates = mainMenuInfo.transform.GetChild(2).gameObject.GetComponent<Text>();
            info.text = string.Format("Mod Loader MSCLoader <color=cyan>v{0}</color> (Fredman Edition) is ready!", Version);
            
            mf.text = string.Format("<color=orange>Mods folder:</color> {0}", ModsFolder);
            MainMenuPath();
            modUpdates.text = string.Empty;
            mainMenuInfo.transform.SetParent(GameObject.Find("MSCLoader Canvas").transform, false);
        }
        
        IEnumerator NewGameMods()
        {
            loading.transform.GetChild(2).GetComponent<Text>().text = string.Format("MSCLoader <color=green>v{0}</color>", Version);
            ModConsole.Print("Resetting mods...");
            loading.SetActive(true);
            loading.transform.GetChild(3).GetComponent<Slider>().minValue = 1;
            loading.transform.GetChild(3).GetComponent<Slider>().maxValue = LoadedMods.Count - 2;

            int i = 1;
            foreach (Mod mod in LoadedMods)
            {
                loading.transform.GetChild(0).GetComponent<Text>().text = string.Format("<color=red>Resetting mods: <color=orange><b>{0}</b></color> of <color=orange><b>{1}</b></color>. <b>Do not skip intro yet!...</b></color>", i, LoadedMods.Count - 2);
                loading.transform.GetChild(3).GetComponent<Slider>().value = i;
                loading.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color = Color.red;
                if (mod.ID.StartsWith("MSCLoader_"))
                    continue;
                i++;
                loading.transform.GetChild(1).GetComponent<Text>().text = mod.Name;
                yield return new WaitForSeconds(.4f);
                try
                {
                    mod.OnNewGame();
                }
                catch (Exception e)
                {
                    StackFrame frame = new StackTrace(e, true).GetFrame(0);

                    string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                    ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));

                    UnityEngine.Debug.Log(e);
                }

            }
            loading.transform.GetChild(0).GetComponent<Text>().text = string.Format("Resetting Done! You can skip intro now!");
            yield return new WaitForSeconds(2f);
            loading.SetActive(false);
            IsModsDoneResetting = true;
            ModConsole.Print("Resetting done!");
            IsModsResetting = false;
        }

        IEnumerator LoadMods()
        {
            loading.transform.GetChild(2).GetComponent<Text>().text = string.Format("MSCLoader <color=green>v{0}</color>", Version);
            ModConsole.Print("Loading mods...");
            Stopwatch s = new Stopwatch();
            s.Start();
            ModConsole.Print("<color=#505050ff>");
            loading.SetActive(true);
            loading.transform.GetChild(3).GetComponent<Slider>().minValue = 1;
            loading.transform.GetChild(3).GetComponent<Slider>().maxValue = LoadedMods.Count - 2;

            int i = 1;
            foreach (Mod mod in LoadedMods)
            {

                loading.transform.GetChild(0).GetComponent<Text>().text = string.Format("Loading mods: <color=orage><b>{0}</b></color> of <color=orage><b>{1}</b></color>. Please wait...", i, LoadedMods.Count - 2);
                loading.transform.GetChild(3).GetComponent<Slider>().value = i;
                loading.transform.GetChild(3).GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(0, 113, 0, 255);
                if (mod.ID.StartsWith("MSCLoader_"))
                    continue;
                i++;
                if (!mod.isDisabled)
                    loading.transform.GetChild(1).GetComponent<Text>().text = mod.Name;
                yield return new WaitForSeconds(.05f);
                try
                {
                    if (!mod.isDisabled)
                    {
                        mod.OnLoad();
                        FsmHook.FsmInject(GameObject.Find("ITEMS"), "Save game", mod.OnSave);
                    }
                }
                catch (Exception e)
                {
                    StackFrame frame = new StackTrace(e, true).GetFrame(0);

                    string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                    ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    UnityEngine.Debug.Log(e);
                }

            }
            loading.SetActive(false);
            ModConsole.Print("</color>");
            allModsLoaded = true;
            ModSettings_menu.LoadBinds();
            IsModsDoneLoading = true;
            s.Stop();
            if (s.ElapsedMilliseconds < 1000)
                ModConsole.Print(string.Format("Loading mods completed in {0}ms!", s.ElapsedMilliseconds));
            else
                ModConsole.Print(string.Format("Loading mods completed in {0} sec(s)!", s.Elapsed.Seconds));
        }

        private void PreLoadMods()
        {
            foreach (string file in Directory.GetFiles(ModsFolder))
            {
                if (file.EndsWith(".dll"))
                {
                    LoadDLL(file);
                }
            }
        }

        private void LoadModsSettings()
        {
            foreach (Mod mod in LoadedMods)
            {
                if (mod.ID.StartsWith("MSCLoader_"))
                    continue;
                try
                {
                    mod.ModSettings();
                }
                catch (Exception e)
                {
                    ModConsole.Error(string.Format("Settings error for mod <b>{0}</b>{2}<b>Details:</b> {1}", mod.ID, e.Message, Environment.NewLine));
                    UnityEngine.Debug.Log(e);
                }
            }
            ModSettings_menu.LoadSettings();
        }
        
        private void LoadDLL(string file)
        {
            try
            {
                Assembly asm = Assembly.LoadFrom(file);
                bool isMod = false;

                AssemblyName[] list = asm.GetReferencedAssemblies();
                if (File.ReadAllText(file).Contains("RegistryKey"))
                    throw new FileLoadException();
                // Look through all public classes                
                foreach (Type type in asm.GetTypes())
                {
                    string msVer = null;
                    if (typeof(Mod).IsAssignableFrom(type))
                    {
                        for (int i = 0; i < list.Length; i++)
                        {
                            if (list[i].Name == "Assembly-CSharp-firstpass")
                            {
                                throw new Exception("Targeting forbidden reference");
                            }
                            if (list[i].Name == "MSCLoader")
                            {
                                string[] verparse = list[i].Version.ToString().Split('.');
                                if (list[i].Version.ToString() == "1.0.0.0")
                                    msVer = "0.1";
                                else
                                {
                                    if (verparse[2] == "0")
                                        msVer = string.Format("{0}.{1}", verparse[0], verparse[1]);
                                    else
                                        msVer = string.Format("{0}.{1}.{2}", verparse[0], verparse[1], verparse[2]);
                                }
                            }
                        }
                        isMod = true;
                        LoadMod((Mod)Activator.CreateInstance(type), msVer);
                        break;
                    }
                    else
                    {
                        isMod = false;
                    }
                }
                if (!isMod)
                {
                    ModConsole.Error(string.Format("<b>{0}</b> - doesn't look like a mod or missing Mod subclass!", Path.GetFileName(file)));
                    InvalidMods.Add(Path.GetFileName(file));
                }
            }
            catch (Exception e)
            {
                ModConsole.Error(string.Format("<b>{0}</b> - doesn't look like a mod, remove this file from mods folder!", Path.GetFileName(file)));
                UnityEngine.Debug.Log(e);
                InvalidMods.Add(Path.GetFileName(file));
            }

        }

        private void LoadMod(Mod mod, string msver)
        {
            // Check if mod already exists
            if (!LoadedMods.Contains(mod))
            {
                // Create config folder
                if (!Directory.Exists(ConfigFolder + mod.ID))
                    Directory.CreateDirectory(ConfigFolder + mod.ID);

                if (mod.UseAssetsFolder && !Directory.Exists(AssetsFolder + mod.ID))
                        Directory.CreateDirectory(AssetsFolder + mod.ID);

                try
                {
                    if (mod.LoadInMenu)
                    {
                        mod.OnMenuLoad();
                        ModSettings_menu.LoadBinds();
                    }
                }
                catch (Exception e)
                {
                    StackFrame frame = new StackTrace(e, true).GetFrame(0);

                    string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                    ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    ModConsole.Error(e.ToString());
                    UnityEngine.Debug.Log(e);
                }
                mod.compiledVersion = msver;
                LoadedMods.Add(mod);
            }
            else
            {
                ModConsole.Error(string.Format("<color=orange><b>Mod already loaded (or duplicated ID):</b></color><color=red><b>{0}</b></color>", mod.ID));
            }
        }

        private void OnGUI()
        {
            GUI.skin = guiskin;
            for (int i = 0; i < LoadedMods.Count; i++)
            {
                Mod mod = LoadedMods[i];
                try
                {
                    if (mod.LoadInMenu && !mod.isDisabled)
                        mod.OnGUI();
                    else if (Application.loadedLevelName == "GAME" && !mod.isDisabled && allModsLoaded)
                        mod.OnGUI();
                }
                catch (Exception e)
                {
                    if (LogAllErrors)
                    {
                        StackFrame frame = new StackTrace(e, true).GetFrame(0);

                        string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                        ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    }
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        private void Update()
        {
            if (!fullyLoaded)
            {
                //check if camera is active.
                if (GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera") != null)
                {
                    //load mods
                    allModsLoaded = false;
                    fullyLoaded = true;
                    StartLoadingMods();
                }
            }

            for (int i = 0; i < LoadedMods.Count; i++)
            {
                Mod mod = LoadedMods[i];
                try
                {
                    if (mod.LoadInMenu && !mod.isDisabled)
                        mod.Update();
                    else if (Application.loadedLevelName == "GAME" && !mod.isDisabled && allModsLoaded)
                        mod.Update();
                }
                catch (Exception e)
                {
                    if (LogAllErrors)
                    {
                        StackFrame frame = new StackTrace(e, true).GetFrame(0);

                        string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                        ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    }
                    UnityEngine.Debug.Log(e);
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < LoadedMods.Count; i++)
            {
                Mod mod = LoadedMods[i];
                try
                {
                    if (mod.LoadInMenu && !mod.isDisabled)
                        mod.FixedUpdate();
                    else if (Application.loadedLevelName == "GAME" && !mod.isDisabled && allModsLoaded)
                        mod.FixedUpdate();
                }
                catch (Exception e)
                {
                    if (LogAllErrors)
                    {
                        StackFrame frame = new StackTrace(e, true).GetFrame(0);

                        string errorDetails = string.Format("{2}<b>Details: </b>{0} in <b>{1}</b>", e.Message, frame.GetMethod(), Environment.NewLine);
                        ModConsole.Error(string.Format("Mod <b>{0}</b> throw an error!{1}", mod.ID, errorDetails));
                    }
                    UnityEngine.Debug.Log(e);
                }
            }
        }
    }
}