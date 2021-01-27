using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MSCLoader;

public class NewBehaviourScript : MonoBehaviour
{
    public SettingKeybind keybind;

    public Text down, up, held;

    int downCount, upCount;
	
	// Update is called once per frame
	void Update ()
    {
        held.text = keybind.GetKey().ToString();

        if (keybind.GetKeyDown())
        {
            downCount++;
            down.text = downCount.ToString();
        }

        if (keybind.GetKeyUp())
        {
            upCount++;
            up.text = upCount.ToString();
        }
    }
}
