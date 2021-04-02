using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MSCLoader
{
    //[HarmonyPatch(typeof(PlayMakerArrayListProxy), "Awake")]
    class InjectModLoaderInit
    {
        public static void Prefix()
        {
            System.Console.WriteLine("MODLOADER: INITIALIZING");
            ModLoader.Init();
            // Start the click through fix after the Main Menu has been loaded fully
            MSCLoader.ModLoaderInstance.Patch(typeof(HutongGames.PlayMaker.Actions.MousePickEvent).GetMethod("DoRaycast", BindingFlags.Instance | BindingFlags.NonPublic), new Harmony.HarmonyMethod(typeof(InjectUIClickFix).GetMethod("Prefix")));
            MSCLoader.ModLoaderInstance.UnpatchAll("MSCModLoaderProInit");
        }
    }

    //[HarmonyPatch(typeof(PlayMakerFSM), "Awake")]
    class InjectSplashSkip
    {
        public static void Prefix()
        {
            if (MSCLoader.settings.SkipSplashScreen && Application.loadedLevel == 0)
            {
                System.Console.WriteLine("MODLOADER: SKIP SPLASH");
                Application.LoadLevel(1);
                MSCLoader.ModLoaderInstance.UnpatchAll("MSCModLoaderProSplash");

            }
        }
    }

    //[HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.MousePickEvent), "DoRaycast")]
    class InjectUIClickFix
    {
        public static bool Prefix(ref bool __result)
        {
            if (GUIUtility.hotControl != 0 || (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    //[HarmonyPatch(typeof(HutongGames.PlayMaker.Actions.LoadLevel), "OnEnter")]
    class InjectLoadSceneFix
    {
        public static void Prefix()
        {
            // Because of a delay this method can't be used in the main menu, 
            // that's done by adding an OnEnable to the load screen object instead 
            if (Application.loadedLevel > 1)
                ModLoader.modSceneLoadHandler.Disable();
        }
    }
}
