﻿using MSCLoader;
using UnityEngine;

namespace $safeprojectname$
{
	public class $safeprojectname$ : Mod
    {
        public override string ID => "$safeprojectname$";
        public override string Name => "$projectname$";
        public override string Author => "YOU";
        public override string Version => "1.0";
        public override string Description => "THIS IS MY AWESOME MOD!";
        // public override string UpdateLink => "https://www.nexusmods.com/mysummercar/mods/9999";
        // public override byte[] Icon => Properties.Resources.Icon;

        // Learn more at https://mscloaderpro.github.io/docs/!

        public override void ModSettings()
        {
            // Here you can add all the settings you want for your mod!
        }

        public override void OnNewGame()
        {
            // If the player starts a new game, there are things you need to reset for maximum immersion in the game.
        }

        public override void OnLoad()
        {
            // This is likely the method you want to use to load your stuff in the game.
        }

        public override void OnSave()
        {
            // If you have something to save, this would be the place to do it!
        }
    }
}
