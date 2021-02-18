using Harmony;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;

#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MSCLoader
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Main(string[] args)
        {
            Console.WriteLine("STARTING MOD LOADER PRO!");
            ExtraTweaks();
            AppDomain.CurrentDomain.AssemblyLoad += AssemblyWatcher;
        }

        static void AssemblyWatcher(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.GetName().Name == "System")
            {
                AppDomain.CurrentDomain.AssemblyLoad -= AssemblyWatcher;
                StartModLoader();
            }
        }

        internal static HarmonyInstance ModLoaderInstance;
        static void StartModLoader()
        {
            try
            {
                //HarmonyInstance.DEBUG = true;
                Console.WriteLine("MODLOADER: PATCHING METHODS!");
                ModLoaderInstance = HarmonyInstance.Create("MSCModLoaderPro");
                if (settings.EnableModLoader)
                {
                    HarmonyInstance.Create("MSCModLoaderProInit").Patch(typeof(PlayMakerArrayListProxy).GetMethod("Awake"), new HarmonyMethod(typeof(InjectModLoaderInit).GetMethod("Prefix")));
                    HarmonyInstance.Create("MSCModLoaderProSplash").Patch(typeof(PlayMakerFSM).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic), new HarmonyMethod(typeof(InjectSplashSkip).GetMethod("Prefix")));
                    
                    ModLoaderInstance.Patch(typeof(HutongGames.PlayMaker.Actions.LoadLevel).GetMethod("OnEnter"), new HarmonyMethod(typeof(InjectLoadSceneFix).GetMethod("Prefix")));
                }
                else if (settings.SkipSplashScreen)
                {
                    HarmonyInstance.Create("MSCModLoaderProSplash").Patch(typeof(PlayMakerFSM).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic), new HarmonyMethod(typeof(InjectSplashSkip).GetMethod("Prefix")));
                }
            }
            catch (Exception ex)
            {
                using (TextWriter textWriter = File.CreateText("ModLoaderCrash.txt"))
                {
                    textWriter.WriteLine($"{ex}\n{ex.Message}");
                    textWriter.Flush();
                }
            }
        }

        static void ExtraTweaks()
        {
            byte[] data = {
               0x41, 0x6d, 0x69, 0x73, 0x74, 0x65, 0x63, 0x68, 0x0d, 0x00, 0x00, 0x00, 0x4d,
               0x79, 0x20, 0x53, 0x75, 0x6d, 0x6d, 0x65, 0x72, 0x20, 0x43, 0x61, 0x72
            };
            try
            {
                Console.WriteLine("MODLOADER: WRITING TWEAKS");

                settings = new LoaderSettings();
                long offset = FindBytes(@"mysummercar_Data\mainData", data);

                using (FileStream stream = new FileStream(@"mysummercar_Data\mainData", FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Position = offset + 96L;
                    if (!settings.SkipGameLauncher) stream.WriteByte(0x01);
                    else stream.WriteByte(0x00);

                    stream.Position = offset + 115L;
                    if (settings.UseOutputLog) stream.WriteByte(0x01);
                    else stream.WriteByte(0x00);

                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                using (TextWriter textWriter = File.CreateText("ModLoaderCrash.txt"))
                {
                    textWriter.WriteLine(ex.ToString());
                    textWriter.WriteLine(ex.Message);
                    textWriter.Flush();
                }
            }
        }

        static long FindBytes(string fileName, byte[] bytes)
        {
            long i, j;
            using (FileStream fs = File.OpenRead(fileName))
            {
                for (i = 0; i < fs.Length - bytes.Length; i++)
                {
                    fs.Seek(i, SeekOrigin.Begin);
                    for (j = 0; j < bytes.Length; j++)
                        if (fs.ReadByte() != bytes[j]) break;
                    if (j == bytes.Length) break;
                }
                fs.Close();
            }
            return i;
        }

        internal static LoaderSettings settings;
        internal class LoaderSettings
        {
            public bool SkipGameLauncher;
            public bool SkipSplashScreen;
            public bool UseVsyncInMenu;

            public int UpdateMode;
            public string LastUpdateCheck;
            public int UpdateInterval;

            public KeyCode[] OpenConsoleKey;
            public int ConsoleFontSize;
            public int ConsoleAutoOpen;
            public int ConsoleWindowHeight;
            public int ConsoleWindowWidth;

            public bool EnableModLoader;
            public bool UseOutputLog;
            readonly ModINI settingINI;

            public LoaderSettings()
            {
                settingINI = new ModINI("ModLoaderSettings");

                SkipGameLauncher = settingINI.Read<bool>("SkipGameLauncher", "General");
                SkipSplashScreen = settingINI.Read<bool>("SkipSplashScreen", "General");
                UseVsyncInMenu = settingINI.Read<bool>("UseVsyncInMenu", "General");

                UpdateMode = settingINI.Read<int>("UpdateMode", "Updates");
                LastUpdateCheck = settingINI.Read<string>("LastUpdateCheck", "Updates");
                UpdateInterval = settingINI.Read<int>("UpdateInterval", "Updates");

                OpenConsoleKey = settingINI.Read<string>("OpenConsoleKey", "Console").Split(';').Select(x => (KeyCode)Enum.Parse(typeof(KeyCode), x, true)).ToArray();
                ConsoleFontSize = settingINI.Read<int>("ConsoleFontSize", "Console");
                ConsoleAutoOpen = settingINI.Read<int>("ConsoleAutoOpen", "Console");
                ConsoleWindowHeight = settingINI.Read<int>("ConsoleWindowHeight", "Console");
                ConsoleWindowWidth = settingINI.Read<int>("ConsoleWindowWidth", "Console");

                EnableModLoader = settingINI.Read<bool>("EnableModLoader", "Hidden");
                UseOutputLog = settingINI.Read<bool>("UseOutputLog", "Hidden");
            }

            public void SaveSettings(ModLoaderSettings modLoaderSettings)
            {
                settings.SkipGameLauncher = modLoaderSettings.SkipGameLauncher;
                settings.SkipSplashScreen = modLoaderSettings.SkipSplashScreen;
                settings.UseVsyncInMenu = modLoaderSettings.UseVsyncInMenu;

                settings.UpdateMode = modLoaderSettings.UpdateMode;
                settings.LastUpdateCheck = modLoaderSettings.LastUpdateCheck;
                settings.UpdateInterval = modLoaderSettings.UpdateInterval;

                List<KeyCode> keycodeList = new List<KeyCode>() { modLoaderSettings.OpenConsoleKeyKeybind };
                keycodeList.AddRange(modLoaderSettings.OpenConsoleKeyModifiers);
                settings.OpenConsoleKey = keycodeList.ToArray();
                settings.ConsoleFontSize = (int)modLoaderSettings.ConsoleFontSize;
                settings.ConsoleAutoOpen = modLoaderSettings.ConsoleAutoOpen;
                settings.ConsoleWindowHeight = (int)modLoaderSettings.ConsoleWindowHeight;
                settings.ConsoleWindowWidth = (int)modLoaderSettings.ConsoleWindowWidth;

                settingINI.Write("SkipGameLauncher", "General", SkipGameLauncher);
                settingINI.Write("SkipSplashScreen", "General", SkipSplashScreen);
                settingINI.Write("UseVsyncInMenu", "General", UseVsyncInMenu);

                settingINI.Write("UpdateMode", "Updates", UpdateMode);
                settingINI.Write("LastUpdateCheck", "Updates", LastUpdateCheck);
                settingINI.Write("UpdateInterval", "Updates", UpdateInterval);

                settingINI.Write("OpenConsoleKey", "Console", string.Join(";", Array.ConvertAll(OpenConsoleKey, x => x.ToString())));
                settingINI.Write("ConsoleFontSize", "Console", ConsoleFontSize);
                settingINI.Write("ConsoleAutoOpen", "Console", ConsoleAutoOpen);
                settingINI.Write("ConsoleWindowHeight", "Console", ConsoleWindowHeight);
                settingINI.Write("ConsoleWindowWidth", "Console", ConsoleWindowWidth);
            }

            public void ApplySettings(ModLoaderSettings modLoaderSettings)
            {
                // Disable saving to the INI while the settings are loaded.
                modLoaderSettings.disableSave = true;

                // Apply the various settings.
                modLoaderSettings.Version = ModLoader.Version;
                modLoaderSettings.SkipGameLauncher = settings.SkipGameLauncher;
                modLoaderSettings.SkipSplashScreen = settings.SkipSplashScreen;

                modLoaderSettings.UseVsyncInMenu = settings.UseVsyncInMenu;
                modLoaderSettings.useVsyncInMenu.OnValueChanged.AddListener((value) =>
                {
                    if (!ModLoader.modLoaderInstance.vSyncEnabled && ModLoader.CurrentScene == CurrentScene.MainMenu)
                        QualitySettings.vSyncCount = modLoaderSettings.UseVsyncInMenu ? 1 : 0;
                });

                modLoaderSettings.UpdateMode = settings.UpdateMode;
                modLoaderSettings.ParseUpdateCheckTime(settings.LastUpdateCheck);
                modLoaderSettings.UpdateInterval = settings.UpdateInterval;

                modLoaderSettings.OpenConsoleKeyKeybind = settings.OpenConsoleKey[0];
                modLoaderSettings.OpenConsoleKeyModifiers = settings.OpenConsoleKey.Skip(1).ToArray();
                modLoaderSettings.openConsoleKey.PostBind.AddListener(modLoaderSettings.SaveSettings);

                modLoaderSettings.ConsoleFontSize = settings.ConsoleFontSize;
                modLoaderSettings.ConsoleAutoOpen = settings.ConsoleAutoOpen;
                modLoaderSettings.ConsoleWindowHeight = settings.ConsoleWindowHeight;
                modLoaderSettings.ConsoleWindowWidth = settings.ConsoleWindowWidth;

                // Enable saving again if any of the values are changed.
                modLoaderSettings.disableSave = false;
            }
        }

        [HarmonyPatch(typeof(PlayMakerArrayListProxy), "Awake")]
        class InjectModLoaderInit
        {
            public static void Prefix()
            {
                System.Console.WriteLine("MODLOADER: INITIALIZING");
                ModLoader.Init(); 
                ModLoaderInstance.Patch(typeof(HutongGames.PlayMaker.Actions.MousePickEvent).GetMethod("DoMousePickEvent", BindingFlags.Instance | BindingFlags.NonPublic), new HarmonyMethod(typeof(InjectUIClickFix).GetMethod("Prefix")));
                ModLoaderInstance.UnpatchAll("MSCModLoaderProInit");
            }
        }

        [HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
        class InjectSplashSkip
        {
            public static void Prefix()
            {
                if (settings.SkipSplashScreen && Application.loadedLevel == 0)
                {
                    System.Console.WriteLine("MODLOADER: SKIP SPLASH");
                    Application.LoadLevel(1);
                    ModLoaderInstance.UnpatchAll("MSCModLoaderProSplash");

                }
            }
        }

        [HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.MousePickEvent), "DoMousePickEvent")]
        class InjectUIClickFix
        {
            public static bool Prefix() => 
                (GUIUtility.hotControl == 0 && EventSystem.current != null && !EventSystem.current.IsPointerOverGameObject());
        }

        [HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.LoadLevel), "OnEnter")]
        class InjectLoadSceneFix
        {
            public static void Prefix()
            {
                // Because of a delay this method can't be used in the main menu, 
                // that's done by adding an OnEnable to the load screen object instead 
                if (Application.loadedLevel > 1)
                    ModLoader.modLoaderInstance.modSceneLoadHandler.Disable();
            }
        }
    }
}
#pragma warning restore CS0618, IDE1006, CS1591
