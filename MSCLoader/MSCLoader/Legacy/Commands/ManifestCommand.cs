using System.Linq;
/*
namespace MSCLoader.Commands
{
    public class ManifestCommand : ConsoleCommand
    {
        public override string Name => "Manifest";
        public override string Help => "Command Description";
        public override bool ShowInHelp => false;

        public override void Run(string[] args)
        {
            if (args.Length == 2)
            {
                Mod mod = ModLoader.LoadedMods.FirstOrDefault(m => m.ID == args[1]);
                if (mod != null && args[0].ToLower() == "create") ManifestHandler.CreateManifest(mod);
                else if (mod != null && args[0].ToLower() == "update") ManifestHandler.UpdateManifest(mod);
                else ModConsole.Error("Invalid ModID (ModID is case sensitive)");
            }
            else ModConsole.Error("Invalid syntax");
        }

    }
}
*/