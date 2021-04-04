using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// GNU GPL 3.0
#pragma warning disable CS1591, IDE1006, CS0618
namespace MSCLoader
{
    [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
    public class SaveData
    {
        public List<SaveDataList> save = new List<SaveDataList>();
    }

    [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
    public class SaveDataList
    {
        public string name;
        public Vector3 pos;
        public float rotX, rotY, rotZ;
    }

    [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
    public class SaveLoad
    {
        [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
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

            File.WriteAllText(Path.Combine(Application.persistentDataPath, string.Format("{0}_{1}", mod.ID, fileName)), Newtonsoft.Json.JsonConvert.SerializeObject(save, Newtonsoft.Json.Formatting.Indented));
        }

        [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
        public static void LoadGameObject(Mod mod, string fileName)
        {
            SaveData data = DeserializeSaveFile<SaveData>(mod, fileName);
            GameObject go = GameObject.Find(data.save[0].name);
            go.transform.position = data.save[0].pos;
            go.transform.rotation = Quaternion.Euler(data.save[0].rotX, data.save[0].rotY, data.save[0].rotZ);
        }

        [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
        public static void SerializeSaveFile<T>(Mod mod, T saveDataClass, string fileName)
        {
            var config = new JsonSerializerSettings();
            config.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            config.Formatting = Formatting.Indented;
            string path = Path.Combine(ModLoader.GetModSettingsFolder(mod), fileName);
            string serializedData = JsonConvert.SerializeObject(saveDataClass, config);
            File.WriteAllText(path, serializedData);
        }
        [Obsolete("SaveLoad is obsolete, use ModSave instead.")]
        public static T DeserializeSaveFile<T>(Mod mod, string fileName) where T : new()
        {
            string path = Path.Combine(ModLoader.GetModSettingsFolder(mod), fileName);
            if (File.Exists(path))
            {
                string serializedData = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(serializedData);
            }
            return default(T);
        }
    }
}