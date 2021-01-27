using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// GNU GPL 3.0
namespace MSCLoader 
{
    public class KeybindList
    {
        public List<Keybinds> keybinds = new List<Keybinds>();
    }

    public class Keybinds
    {
        public string ID;
        public KeyCode Key, Modifier;
    }

    public class KeyBinding : MonoBehaviour
    {
        public Mod mod;
        public string id;

        public KeyCode key, modifierKey;
        public Text keyDisplay, modifierDisplay;
        public GameObject keyButton, modifierButton;
        Image keyButtonImage, modifierButtonImage;

        bool reassignKey = false, ismodifier = false;
        public Color toggleColor = new Color32(0xFF, 0xFF, 0x00, 0xFF);
        
        Color originalColor = Color.white;

        public void LoadBind()
        {
            modifierButtonImage = modifierButton.GetComponent<Image>();
            keyButtonImage = keyButton.GetComponent<Image>();

            modifierButton.GetComponent<Button>().onClick.AddListener(() => ChangeKeyCode(true, true));
            keyButton.GetComponent<Button>().onClick.AddListener(() => ChangeKeyCode(true,false));

            modifierDisplay.text = modifierKey.ToString();
            keyDisplay.text = key.ToString();
        }

        public void ChangeKeyCode(bool toggle, bool modifier)
        {
            reassignKey = toggle;
            ismodifier = modifier;

            if (modifier) modifierButtonImage.color = toggle ? toggleColor : originalColor;
            else keyButtonImage.color = toggle ? toggleColor : originalColor;

            StartCoroutine(ReassignKey());
        }

        IEnumerator ReassignKey()
        {
            while (reassignKey)
            {
                if (Input.anyKeyDown)
                {
                    foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (Input.GetKeyDown(kcode))
                        {
                            if (kcode != KeyCode.Mouse0 && kcode != KeyCode.Mouse1) //LMB = cancel
                                UpdateKeyCode(kcode, ismodifier);
                            else if (kcode == KeyCode.Mouse1) //RMB = sets to none
                                UpdateKeyCode(KeyCode.None, ismodifier);
                            ChangeKeyCode(false, ismodifier);
                        }
                    }
                }

                yield return null;
            }
        }

        void UpdateKeyCode(KeyCode kcode, bool modifier)
        {
            Keybind bind = Keybind.Keybinds.Find(x => x.Mod == mod && x.ID == id);
            if (modifier)
            {
                bind.Modifier = kcode;
                modifierDisplay.text = kcode.ToString();
            }
            else
            {
                bind.Key = kcode;
                keyDisplay.text = kcode.ToString();
            }

            ModSettings_menu.SaveModBinds(mod);
        }
    }
}