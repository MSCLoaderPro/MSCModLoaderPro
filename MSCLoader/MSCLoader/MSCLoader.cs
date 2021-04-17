using System;
using System.IO;
using System.Reflection;

namespace MSCLoader
{
    internal static class MSCLoader
    {
        internal static LoaderSettings settings;
        internal static Harmony.HarmonyInstance ModLoaderInstance;
        static string[] arguments;

        internal static void Main()
        {
            arguments = Environment.GetCommandLineArgs();
            settings = new LoaderSettings();
            ExtraTweaks();
            if (FindArgument("-disableModLoader")) return;
            AppDomain.CurrentDomain.AssemblyLoad += AssemblyWatcher;
        }

        static void AssemblyWatcher(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.GetName().Name == "System")
            {
                AppDomain.CurrentDomain.AssemblyLoad -= AssemblyWatcher;
                Console.WriteLine("STARTING MOD LOADER PRO!");
                StartModLoader();
            }
        }

        static void StartModLoader()
        {
            try
            {
                //HarmonyInstance.DEBUG = true;
                Console.WriteLine("MODLOADER: PATCHING METHODS!");
                ModLoaderInstance = Harmony.HarmonyInstance.Create("MSCModLoaderPro");
                if (settings.EnableModLoader)
                {
                    Harmony.HarmonyInstance.Create("MSCModLoaderProInit").Patch(typeof(PlayMakerArrayListProxy).GetMethod("Awake"), new Harmony.HarmonyMethod(typeof(InjectModLoaderInit).GetMethod("Prefix")));
                    Harmony.HarmonyInstance.Create("MSCModLoaderProSplash").Patch(typeof(PlayMakerFSM).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic), new Harmony.HarmonyMethod(typeof(InjectSplashSkip).GetMethod("Prefix")));
                    
                    ModLoaderInstance.Patch(typeof(HutongGames.PlayMaker.Actions.LoadLevel).GetMethod("OnEnter"), new Harmony.HarmonyMethod(typeof(InjectLoadSceneFix).GetMethod("Prefix")));
                }
                else if (settings.SkipSplashScreen)
                {
                    Harmony.HarmonyInstance.Create("MSCModLoaderProSplash").Patch(typeof(PlayMakerFSM).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic), new Harmony.HarmonyMethod(typeof(InjectSplashSkip).GetMethod("Prefix")));
                }
            }
            catch (Exception exception)
            {
                using (TextWriter textWriter = File.CreateText("ModLoaderCrash.txt"))
                {
                    textWriter.WriteLine($"{exception}\n{exception.Message}");
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
                long offset = FindBytes(@"mysummercar_Data\mainData", data);

                using (FileStream stream = new FileStream(@"mysummercar_Data\mainData", FileMode.Open, FileAccess.ReadWrite))
                {
                    stream.Position = offset + 96L; 
                    stream.WriteByte(0x01);

                    if ((settings.SkipGameLauncher || FindArgument("-skipLauncher")) && !FindArgument("-disableModLoader") && !FindArgument("-showLauncher"))
                        stream.WriteByte(0x00);


                    stream.Position = offset + 115L;
                    stream.WriteByte(0x00);

                    if (settings.UseOutputLog) stream.WriteByte(0x01);

                    stream.Close();
                }
            }
            catch (Exception exception)
            {
                using (TextWriter textWriter = File.CreateText("ModLoaderCrash.txt"))
                {
                    textWriter.WriteLine($"{exception}\n{exception.Message}");
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
        }// Helper function for getting the command line arguments
        
        internal static bool FindArgument(string name)
        {
            for (int i = 0; i < arguments.Length; i++)
                if (arguments[i] == name && arguments.Length > i + 1)
                    return true;

            return false;
        }
    }
}
