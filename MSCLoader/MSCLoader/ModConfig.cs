using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS1591
namespace MSCLoader
{
    /// <summary>Holder class for easy saving of settings to a file.</summary>
    public class ModConfig
    {
        /// <summary>Whether or not the mod is enabled.</summary>
        public bool Enabled = true;
        /// <summary>Saved keybinds</summary>
        public List<ModConfigKeybind> Keybinds = new List<ModConfigKeybind>();
        /// <summary>Saved numbers</summary>
        public List<ModConfigNumber> Numbers = new List<ModConfigNumber>();
        /// <summary>Saved booleans</summary>
        public List<ModConfigBool> Booleans = new List<ModConfigBool>();
        /// <summary>Saved strings</summary>
        public List<ModConfigString> Strings = new List<ModConfigString>();
    }
    public class ModConfigKeybind
    {
        public string id;
        public KeyCode keybind;
        public KeyCode[] modifiers;
        public ModConfigKeybind(string ID, KeyCode key, KeyCode[] modifier)
        {
            id = ID;
            keybind = key;
            modifiers = modifier;
        }
    }
    public class ModConfigNumber
    {
        public string id;
        public float value;
        public ModConfigNumber(string ID, float number)
        {
            id = ID;
            value = number;
        }
    }
    public class ModConfigBool
    {
        public string id;
        public bool value;
        public ModConfigBool(string ID, bool boolean)
        {
            id = ID;
            value = boolean;
        }
    }
    public class ModConfigString
    {
        public string id;
        public string value;
        public ModConfigString(string ID, string text)
        {
            id = ID;
            value = text;
        }
    }
}
