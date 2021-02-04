using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    public class SaveData
    {
        public List<SaveDataList> save = new List<SaveDataList>();
    }

    public class SaveDataList
    {
        public string name;
        public Vector3 pos;
        public float rotX, rotY, rotZ;
    }

    public class SaveLoad
    {
        public static void SaveGameObject(Mod mod, GameObject g, string fileName)
        {
            SaveData save = new SaveData();
            save.save.Add(new SaveDataList
            {
                name = g.name,
                pos = g.transform.position,
                rotX = g.transform.localEulerAngles.x,
                rotY = g.transform.localEulerAngles.y,
                rotZ = g.transform.localEulerAngles.z
            });

            File.WriteAllText(Path.Combine(Application.persistentDataPath, string.Format("{0}_{1}", mod.ID, fileName)), JsonConvert.SerializeObject(save, Formatting.Indented));
        }

        public static void LoadGameObject(Mod mod, string fileName)
        {
            SaveData data = DeserializeSaveFile<SaveData>(mod, fileName);
            GameObject go = GameObject.Find(data.save[0].name);
            go.transform.position = data.save[0].pos;
            go.transform.rotation = Quaternion.Euler(data.save[0].rotX, data.save[0].rotY, data.save[0].rotZ);
        }

        public static void SerializeSaveFile<T>(Mod mod, T saveDataClass, string fileName) => 
            File.WriteAllText(Path.Combine(Application.persistentDataPath, string.Format("{0}_{1}", mod.ID, fileName)), JsonConvert.SerializeObject(saveDataClass, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore, Formatting = Formatting.Indented } ));

        public static T DeserializeSaveFile<T>(Mod mod, string fileName) where T : new()
        {
            string path = Path.Combine(Application.persistentDataPath, string.Format("{0}_{1}", mod.ID, fileName));

            return File.Exists(path) ? JsonConvert.DeserializeObject<T>(File.ReadAllText(path)) : default;
        }
    }
}