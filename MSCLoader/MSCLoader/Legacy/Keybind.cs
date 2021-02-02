using System;
using System.Collections.Generic;
using UnityEngine;

// GNU GPL 3.0
namespace MSCLoader
{
    [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
	public class Keybind
	{
        public static List<Keybind> Keybinds = new List<Keybind>();
        public static List<Keybind> DefaultKeybinds = new List<Keybind>();

        public string ID, Name;
        public KeyCode Key, Modifier;
        public Mod Mod;
        public object[] Vals { get; set; }

        public SettingKeybind keybind;

        public bool noModifier = false;

        [Obsolete("Old Keybind is obsolete, use modSettings.AddKeybind() instead.")]
        public static void Add(Mod mod, Keybind key)
        {
            key.Mod = mod;
            Keybinds.Add(key);
            DefaultKeybinds.Add(new Keybind(key.ID, key.Name, key.Key, key.Modifier) { Mod = mod });

            if (key.noModifier)
                key.keybind = mod.modSettings.AddKeybind(key.ID, key.Name, key.Key);
            else
                key.keybind = mod.modSettings.AddKeybind(key.ID, key.Name, key.Key, key.Modifier);
        }

        [Obsolete("Old header is obsolete, use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle) => 
            mod.modSettings.AddHeader(HeaderTitle);

        [Obsolete("Old header is obsolete, use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle, Color backgroundColor, Color textColor) => 
            mod.modSettings.AddHeader(HeaderTitle, backgroundColor, textColor);

        [Obsolete("Old Keybind is obsolete")]
        public static List<Keybind> Get(Mod mod) => Keybinds.FindAll(x => x.Mod == mod);

        [Obsolete("Old Keybind is obsolete")]
        public static List<Keybind> GetDefault(Mod mod) => DefaultKeybinds.FindAll(x => x.Mod == mod);

        [Obsolete("Old Keybind is obsolete")]
        public Keybind(string id, string name, KeyCode key)
        {
            ID = id;
            Name = name;
            Key = key;
            noModifier = true;
        }

        [Obsolete("Old Keybind is obsolete, use modSettings.AddKeybind() instead.")]
        public Keybind(string id, string name, KeyCode key, KeyCode modifier)
        {
            ID = id;
            Name = name;
            Key = key;
            Modifier = modifier;
            noModifier = false;
        }

        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybind() => keybind.GetKey();
        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybindDown() => keybind.GetKeyDown();
        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybindUp() => keybind.GetKeyUp();

        [Obsolete("Old Keybind is obsolete")]
        public bool IsPressed() => keybind.GetKey();
        [Obsolete("Old Keybind is obsolete")]
        public bool IsDown() => keybind.GetKeyDown();
    }
}