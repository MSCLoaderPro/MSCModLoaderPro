using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;
using System;

namespace MSCLoader
{
    /// <summary>Container class for all things saving!</summary>
    public class ModSave
    {
        /// <summary>Saves a class (T) into an XML file of the specified name.</summary>
        /// <typeparam name="T">Class to save</typeparam>
        /// <param name="fileName">Name of the save file. (excluding extension)</param>
        /// <param name="data">Class to save.</param>
        /// <param name="encryptionKey">(Optional) Key for the save encryption.</param>
        public static void Save<T>(string fileName, T data, string encryptionKey = null) where T : class, new()
        {
            try
            {
                string filePath = Path.Combine(Application.persistentDataPath, $"{fileName}.xml");

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                XmlSerializerNamespaces xmlNamespace = new XmlSerializerNamespaces();
                xmlNamespace.Add("", "");
                StreamWriter output = new StreamWriter(filePath);
                XmlWriterSettings xmlSettings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "    ",
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = true
                };
                XmlWriter xmlWriter = XmlWriter.Create(output, xmlSettings);
                xmlSerializer.Serialize(xmlWriter, data, xmlNamespace);

                xmlWriter.Close();
                output.Close();

                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    string clearText = File.ReadAllText(filePath);
                    byte[] clearBytes = Encoding.Unicode.GetBytes(File.ReadAllText(Path.Combine(Application.persistentDataPath, $"{fileName}.xml")));
                    using (Aes encryptor = Aes.Create())
                    {
                        Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                        encryptor.Key = pdb.GetBytes(32);
                        encryptor.IV = pdb.GetBytes(16);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                            {
                                cs.Write(clearBytes, 0, clearBytes.Length);
                                cs.Close();
                            }
                            clearText = Convert.ToBase64String(ms.ToArray());
                        }
                    }
                    File.WriteAllText(filePath, clearText);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                ModConsole.LogError($"{fileName}: Couldn't be saved. \n{ex}");
            }
        }
        /// <summary>Loads a save file with the specified name.</summary>
        /// <typeparam name="T">Class to load.</typeparam>
        /// <param name="fileName">Name of the save file. (excluding extension)</param>
        /// <param name="encryptionKey">(Optional) Key for the save encryption.</param>
        /// <returns>Loaded save class (T).</returns>
        public static T Load<T>(string fileName, string encryptionKey = "") where T : class, new()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, $"{fileName}.xml");

                if (File.Exists(path))
                {
                    if (!string.IsNullOrEmpty(encryptionKey))
                    {
                        string cipherText = File.ReadAllText(path).Replace(" ", "+");
                        byte[] cipherBytes = Convert.FromBase64String(cipherText);
                        using (Aes encryptor = Aes.Create())
                        {
                            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                            encryptor.Key = pdb.GetBytes(32);
                            encryptor.IV = pdb.GetBytes(16);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                                {
                                    cs.Write(cipherBytes, 0, cipherBytes.Length);
                                    cs.Close();
                                }
                                cipherText = Encoding.Unicode.GetString(ms.ToArray());
                            }
                        }

                        File.WriteAllText(path, cipherText);
                    }

                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    StreamReader input = new StreamReader(path);
                    XmlReader xmlReader = XmlReader.Create(input);
					T t = xmlSerializer.Deserialize(xmlReader) as T;
                    input.Close();
                    return t;
                }
            }
            catch (Exception ex)
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
