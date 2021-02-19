using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS1591
namespace MSCLoader
{
    public class ModINI
    {
        readonly string Path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public ModINI(string iniPath = null)
        {
            Path = new FileInfo($"{iniPath}.ini").FullName.ToString();
        }

        public string Read(string Key, string Section)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }
        public T Read<T>(string key, string section) where T : IConvertible => (T) Convert.ChangeType(Read(key, section), typeof(T));
        public T Read<T>(string key, string section, T defaultValue) where T : IConvertible 
        {
            if (!KeyExists(key, section)) Write(key, section, defaultValue);
            return Read<T>(key, section);
        }
        public void Write(string key, string section, object value) => Write(key, section, value.ToString());
        public void Write(string key, string section, string value) => WritePrivateProfileString(section, key, value, Path);
        public bool KeyExists(string key, string section) => Read(key, section).Length > 0;
        public void DeleteKey(string key, string section) => Write(key, section, null);
        public void DeleteSection(string section) => Write(null, section, null);
    }
}
