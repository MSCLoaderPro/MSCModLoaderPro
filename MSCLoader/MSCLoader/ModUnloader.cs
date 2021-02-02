using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSCLoader
{
    internal class ModUnloader : MonoBehaviour
    {
        internal bool reset;

        internal static Queue<string> consoleText;
        internal static bool consoleOpen = false;

        internal void Reset()
        {
            if (!reset)
            {
                reset = true;

                // Make sure the console text is persistent
                consoleText = ModConsole.controller.scrollback;
                consoleOpen = ModConsole.consoleInstance.console.activeSelf;
            }
        }

        void Update()
        {
            if(reset && !Application.isLoadingLevel)
            {
                // Remove everything related to the mod loader.
                foreach (GameObject o in Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.transform.parent == null && o.name.Contains("MSCLoader")))
                    DestroyImmediate(o);

                ModLoader.unloading = false;

                // And then add it all back again.
                ModLoader.Init();

                reset = false;
            }
        }
    }
}