namespace MSCLoader.Commands
{
    public class CommandLogAll : ConsoleCommand
    {
        public override string Name => "log-all";
        public override string Help => "Log <b>ALL</b> mod errors (Warning! May spam console)";

        public override void Run(string[] args)
        {
            ModConsole.Print(string.Format("<color=orange>Log All errors is set to <b>{0}</b></color>", !ModLoader.LogAllErrors));
            ModLoader.LogAllErrors = !ModLoader.LogAllErrors;
        }
    }
}
