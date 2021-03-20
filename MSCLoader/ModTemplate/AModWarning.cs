using MSCLoader;

namespace MyMod1
{
    public class AModWarning : Mod
    {
        // This is a class that will ONLY load in MSCLoader.
        // By default it provides a link to Mod Loader Pro,
        // you can change that if you want to lead your mod users to MSCLoader version of the mod :)

        public override string ID => "$safeprojectname$";
        public override string Name => "$projectname$";
        public override string Author => "YOU";
        public override string Version => "1.0";
        public override bool LoadInMenu => true;
        const string DownloadLink = ""; // You can replace the download link with your mod link, or leave it as it is.

        Settings buttonDownload = new Settings("openDownload", "Download Mod Loader Pro", OpenDownload);
        public override void ModSettings()
        {
            Settings.AddText(this, "This mod has been made for Mod Loader Pro, and it doesn't work with MSCLoader. Download Mod Loader Pro now!");
            Settings.AddButton(this, buttonDownload);
        }

        public override void OnMenuLoad()
        {
            ModUI.ShowYesNoMessage($"Mod \"<b>{this.Name}</b>\" is made for <color=yellow>Mod Loader Pro</color>! Would you like to donwload it now?", OpenDownload);
        }

        static void OpenDownload()
        {
            System.Diagnostics.Process.Start(DownloadLink);
        }
    }
}
