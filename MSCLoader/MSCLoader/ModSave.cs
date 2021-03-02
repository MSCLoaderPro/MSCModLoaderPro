using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace MSCLoader
{
    /// <summary>Container class for all things saving!</summary>
    public class ModSave
    {
        /// <summary>Saves a class (T) into an XML file of the specified name.</summary>
        /// <typeparam name="T">Class to save</typeparam>
        /// <param name="fileName">Name of the save file. (excluding extension)</param>
        /// <param name="value">Class to save.</param>
        public static void Save<T>(string fileName, T value) where T : class, new()
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                XmlSerializerNamespaces xmlNamespace = new XmlSerializerNamespaces();
                xmlNamespace.Add("", "");
                StreamWriter output = new StreamWriter(Path.Combine(Application.persistentDataPath, $"{fileName}.xml"));
                XmlWriterSettings xmlSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true
                };
                XmlWriter xmlWriter = XmlWriter.Create(output, xmlSettings);
                xmlSerializer.Serialize(xmlWriter, value, xmlNamespace);

                xmlWriter.Close();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                ModConsole.LogError($"{fileName}: Couldn't be saved. \n{ex}");
            }
        }
        /// <summary>Loads a save file with the specified name.</summary>
        /// <typeparam name="T">Class to load.</typeparam>
        /// <param name="fileName">Name of the save file. (excluding extension)</param>
        /// <returns>Loaded save class (T).</returns>
        public static T Load<T>(string fileName) where T : class, new()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, $"{fileName}.xml");
                if (File.Exists(path))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    StreamReader input = new StreamReader(path);
                    XmlReader xmlReader = XmlReader.Create(input);
                    return xmlSerializer.Deserialize(xmlReader) as T;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                ModConsole.LogError($"{fileName}: {ex}");
            }

            return new T();
        }
        /// <summary>Deletes a save file of the specified name.</summary>
        /// <param name="fileName">Name of the save file. (excluding extension)</param>
        public static void Delete(string fileName)
        {
            string path = Path.Combine(Application.persistentDataPath, $"{fileName}.xml");
            if (File.Exists(path))
            {
                File.Delete(path);
                ModConsole.Log(fileName + ": Savefile found and deleted, mod is reset.");
            }
            else ModConsole.Log(fileName + ": Savefile not found, mod is already reset.");
        }
    }
}
