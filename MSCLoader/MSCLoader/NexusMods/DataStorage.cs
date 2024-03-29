﻿using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace MSCLoader.NexusMods
{
    class DataStorage
    {
        static string DataFile = Path.Combine(ModUpdater.UpdaterDirectory, "Data.db");

        internal static bool ThrewException;

        internal static void Save(string data)
        {
            if (File.Exists(DataFile))
            {
                File.Delete(DataFile);
            }

            using (StreamWriter sw = new StreamWriter(DataFile, append: true))
            {
                sw.Write(Encrypt(data));
            }
        }

        internal static string Load()
        {
            ThrewException = false;
            try
            {
                if (!File.Exists(DataFile))
                {
                    return "";
                }

                return Decrypt(File.ReadAllText(DataFile));
            }
            catch
            {
                ThrewException = true;
                return "";
            }
        }

        internal static void Delete()
        {
            if (File.Exists(DataFile))
                File.Delete(DataFile);
        }

        static string Encrypt(string clearText)
        {
            string EncryptionKey = "_nwv(i'l;r`?_<?NL*";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
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
            return clearText;
        }

        static string Decrypt(string cipherText)
        {
            string EncryptionKey = "_nwv(i'l;r`?_<?NL*";
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
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
            return cipherText;
        }
    }
}
