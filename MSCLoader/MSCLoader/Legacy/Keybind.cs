using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591
namespace MSCLoader
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
	public class Keybind
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public static List<Keybind> Keybinds = new List<Keybind>();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public static List<Keybind> DefaultKeybinds = new List<Keybind>();

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public string ID, Name;
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public KeyCode Key, Modifier;
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public Mod Mod;
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public object[] Vals { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public SettingKeybind keybind;

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use SettingKeybind instead")]
        public bool noModifier = false;

        [EditorBrowsable(EditorBrowsableState.Never)]
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old header is obsolete, use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle) => 
            mod.modSettings.AddHeader(HeaderTitle);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old header is obsolete, use modSettings.AddHeader() instead.")]
        public static void AddHeader(Mod mod, string HeaderTitle, Color backgroundColor, Color textColor) => 
            mod.modSettings.AddHeader(HeaderTitle, backgroundColor, textColor);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public static List<Keybind> Get(Mod mod) => Keybinds.FindAll(x => x.Mod == mod);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public static List<Keybind> GetDefault(Mod mod) => DefaultKeybinds.FindAll(x => x.Mod == mod);

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public Keybind(string id, string name, KeyCode key)
        {
            ID = id;
            Name = name;
            Key = key;
            noModifier = true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete, use modSettings.AddKeybind() instead.")]
        public Keybind(string id, string name, KeyCode key, KeyCode modifier)
        {
            ID = id;
            Name = name;
            Key = key;
            Modifier = modifier;
            noModifier = false;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybind() => keybind.GetKey();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybindDown() => keybind.GetKeyDown();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public bool GetKeybindUp() => keybind.GetKeyUp();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public bool IsPressed() => keybind.GetKey();
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Old Keybind is obsolete")]
        public bool IsDown() => keybind.GetKeyDown();
    }
}