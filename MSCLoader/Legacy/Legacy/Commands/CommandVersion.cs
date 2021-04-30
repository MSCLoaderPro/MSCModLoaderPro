using System;
using UnityEngine;

// GNU GPL 3.0
namespace MSCLoader.Commands
{
#pragma warning disable CS1591, IDE1006, CS0618
    public class CommandVersion : ConsoleCommand
    {
        public override string Name => "ver";
        public override string Help => "Version information";

        public override void Run(string[] args)
        {
            ModConsole.Log(string.Format("Unity: <b>{0}</b>", Application.unityVersion));
            try
            {
                ModConsole.Log(string.Format("MSC buildID: <b>{0}</b>", Steamworks.SteamApps.GetAppBuildId())); //Get steam buildID
            }
            catch (Exception e)
            {
                ModConsole.Log(string.Format("<color=red>Failed to get build ID:</color> <b>{0}</b>", e.Message)); //Show steamworks error
            }
            ModConsole.Log(string.Format("MSCLoader: <b>{0}</b>", ModLoader.Version));
        }
    }
}
