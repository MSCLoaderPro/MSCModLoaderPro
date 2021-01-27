using System.Collections.Generic;
using UnityEngine;
// GNU GPL 3.0
namespace MSCLoader
{
	public class Keybind
	{
        public static List<Keybind> Keybinds = new List<Keybind>();
        public static List<Keybind> DefaultKeybinds = new List<Keybind>();

        public string ID, Name;
        public KeyCode Key, Modifier;
        public Mod Mod;
        public object[] Vals { get; set; }

        public static void Add(Mod mod, Keybind key)
		{
			key.Mod = mod;
			Keybinds.Add(key);
			DefaultKeybinds.Add(new Keybind(key.ID, key.Name, key.Key, key.Modifier) { Mod = mod });
		}

        public static void AddHeader(Mod mod, string HeaderTitle) => AddHeader(mod, HeaderTitle, Color.blue, Color.white);

        public static void AddHeader(Mod mod, string HeaderTitle, Color backgroundColor, Color textColor)
        {
            Keybind kb = new Keybind(null, HeaderTitle, KeyCode.None, KeyCode.None)
            {
                Mod = mod,
                Vals = new object[3]
            };

            kb.Vals[0] = HeaderTitle;
            kb.Vals[1] = backgroundColor;
            kb.Vals[2] = textColor;

            Keybinds.Add(kb);
        }

        public static List<Keybind> Get(Mod mod) => Keybinds.FindAll(x => x.Mod == mod);

        public static List<Keybind> GetDefault(Mod mod) => DefaultKeybinds.FindAll(x => x.Mod == mod);

        public Keybind(string id, string name, KeyCode key)
        {
            ID = id;
            Name = name;
            Key = key;
            Modifier = KeyCode.None;
        }
        public Keybind(string id, string name, KeyCode key, KeyCode modifier = KeyCode.None)
		{
			ID = id;
			Name = name;
			Key = key;
			Modifier = modifier;
		}

        public bool GetKeybind() => (Modifier != KeyCode.None ? Input.GetKey(Modifier) && Input.GetKey(Key) : Input.GetKey(Key));
        public bool GetKeybindDown() => (Modifier != KeyCode.None ? Input.GetKey(Modifier) && Input.GetKeyDown(Key) : Input.GetKeyDown(Key));
        public bool GetKeybindUp() => (Modifier != KeyCode.None ? Input.GetKey(Modifier) && Input.GetKeyUp(Key) : Input.GetKeyUp(Key));

        public bool IsPressed() => GetKeybind();
        public bool IsDown() => GetKeybindDown();
	}
}