//using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
/*
namespace MSCLoader
{
    internal class ModsManifest
    {
        public string modID, version, description;
        public ManifestLinks links = new ManifestLinks();
        public ManifestIcon icon = new ManifestIcon();
        public ManifestMinReq minimumRequirements = new ManifestMinReq();
        public ManifestModConflict modConflicts = new ManifestModConflict();
        public ManifestModRequired requiredMods = new ManifestModRequired();
        public string sign, sid_sign;
        public byte type;
    }

    internal class ManifestLinks
    {
        public string nexusLink = null, rdLink = null, githubLink = null;
    }

    internal class ManifestIcon
    {
        public string iconFileName = null;
        public bool isIconRemote = false, isIconUrl = false;
    }

    internal class ManifestMinReq
    {
        public string MSCLoaderVer = null;
        public uint MSCbuildID = 0;
        public bool disableIfVer = false;
    }

    internal class ManifestModConflict
    {
        public string modIDs = null, customMessage = null;
        public bool disableIfConflict = false;
    }

    internal class ManifestModRequired
    {
        public string modID = null, minVer = null, customMessage = null;
    }

    internal class ManifestHandler
    {
        public static void CreateManifest(Mod mod)
        {
            try
            {
                ModsManifest mm = new ModsManifest
                {
                    modID = mod.ID,
                    version = mod.Version,
                    description = "",
                    sign = FileHash(mod.fileName),
                    sid_sign = ChecksumCalculator(ModLoader.steamID + mod.ID),
                    type = 0
                };

                string path = ModLoader.GetMetadataFolder($"{mod.ID}.json");

                if (File.Exists(path))
                {
                    ModConsole.Error("Metadata file already exists, to update use update command");
                    return;
                }

                File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(mm, Newtonsoft.Json.Formatting.Indented));

                ModConsole.Print("<color=green>Raw metadata file created successfully</color>");
            }
            catch (Exception e)
            {
                ModConsole.Error(e.Message);
                System.Console.WriteLine(e);
            }
        }

        public static void UpdateManifest(Mod mod)
        {
            if (!File.Exists(ModLoader.GetMetadataFolder(string.Format("{0}.json", mod.ID))))
                ModConsole.Error("Metadata file doesn't exists, to create use create command");
            else if (mod.RemMetadata == null)
                ModConsole.Error(string.Format("Your metadata file doesn't seem to be public, you need to upload first before you can update file.{0}If you want to just recreate metadata, delete old file and use create command", Environment.NewLine));
            else if (mod.RemMetadata.sid_sign != ChecksumCalculator(ModLoader.steamID + mod.ID))
                ModConsole.Error("This mod doesn't belong to you, can't continue");
            else
            {

                try
                {
                    ModsManifest metadata = mod.metadata;

                    switch (new Version(mod.Version).CompareTo(new Version(mod.metadata.version)))
                    {
                        case 0:
                            ModConsole.Error(string.Format("Mod version {0} is same as current metadata version {1}, nothing to update.", mod.Version, mod.metadata.version));
                            break;
                        case 1:
                            metadata.version = mod.Version;
                            metadata.sign = FileHash(mod.fileName);

                            File.WriteAllText(ModLoader.GetMetadataFolder(string.Format("{0}.json", mod.ID)), Newtonsoft.Json.JsonConvert.SerializeObject(metadata, Newtonsoft.Json.Formatting.Indented));

                            ModConsole.Print("<color=green>Metadata file updated successfully, you can upload it now!</color>");
                            break;
                        case -1:
                            ModConsole.Error(string.Format("Mod version {0} is <b>earlier</b> than current metadata version {1}, cannot update.", mod.Version, mod.metadata.version));
                            break;
                    }
                }
                catch (Exception e)
                {
                    ModConsole.Error(e.Message);
                    System.Console.WriteLine(e);
                }
            }
        }

        internal static string FileHash(string fn) =>
            BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(File.ReadAllBytes(fn))).Replace("-", "");

        internal static string ChecksumCalculator(string rawData)
        {
            using (System.Security.Cryptography.SHA1 sha256 = System.Security.Cryptography.SHA1.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
            }
        }
    }
}
*/