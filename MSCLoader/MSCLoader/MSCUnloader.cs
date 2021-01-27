using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MSCLoader
{
    public class MSCUnloader : MonoBehaviour
    {
        internal bool reset;

        public static Queue<string> consoleText;
        public static bool consoleOpen = false;

        internal void Reset()
        {
            if (!reset)
            {
                reset = true;
                consoleText = ModConsole.controller.scrollback;
                consoleOpen = ModConsole.consoleInstance.console.activeSelf;
            }
        }

        void Update()
        {
            if(reset && !Application.isLoadingLevel)
            {
                foreach (GameObject o in Resources.FindObjectsOfTypeAll<GameObject>().Where(o => o.transform.parent == null && o.name.Contains("MSCLoader")))
                    DestroyImmediate(o);

                ModLoader.unloading = false;

                ModLoader.Init();

                reset = false;
            }
        }
    }
}