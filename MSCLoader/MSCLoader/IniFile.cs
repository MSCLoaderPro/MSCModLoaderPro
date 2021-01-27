using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MSCLoader
{
    public class IniFile
    {
        string Path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

        public IniFile(string iniPath = null)
        {
            Path = new FileInfo($"{iniPath}.ini").FullName.ToString();
        }

        public T Read<T>(string key, string Section) where T : IConvertible => (T)Convert.ChangeType(Read(key, Section), typeof(T));
        public string Read(string Key, string Section)
        {
            var RetVal = new StringBuilder(255);
            GetPrivateProfileString(Section, Key, "", RetVal, 255, Path);
            return RetVal.ToString();
        }

        public void Write(string key, string section, object value) => Write(key, section, value.ToString());
        public void Write(string key, string section, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }

        public bool KeyExists(string Key, string Section)
        {
            return Read(Key, Section).Length > 0;
        }
    }
}
