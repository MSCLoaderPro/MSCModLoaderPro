using System;
using System.Collections.Generic;
using System.Linq;

// GNU GPL 3.0
#pragma warning disable IDE1006 // Naming Styles
namespace MSCLoader
{
    public delegate void CommandHandler(string[] args);

    public class ConsoleController
    {
        // Used to communicate with ConsoleView
        public delegate void LogChangedHandler(string[] log);
        public event LogChangedHandler LogChanged;

        // Object to hold information about each command
        class CommandRegistration
        {
            public string command { get; private set; }
            public CommandHandler handler { get; private set; }
            public string help { get; private set; }
            public bool showInHelp { get; private set; }

            public CommandRegistration(string command, CommandHandler handler, string help, bool showInHelp)
            {
                this.command = command;
                this.handler = handler;
                this.help = help;
                this.showInHelp = showInHelp;
            }
        }

        const int scrollbackSize = 500;

        public Queue<string> scrollback = new Queue<string>(scrollbackSize);
        public List<string> commandHistory = new List<string>();
        Dictionary<string, CommandRegistration> commands = new Dictionary<string, CommandRegistration>();

        public string[] log { get; set; } //Copy of scrollback as an array for easier use by ConsoleView

        public ConsoleController()
        {
            RegisterCommand("help", HelpCommand, "This screen", "?");
            RegisterCommand("clear", ClearConsole, "Clears console screen", "cls");

            ConsoleCommand.cc = this;

            if (MSCUnloader.consoleText != null)
            {
                scrollback = MSCUnloader.consoleText;
                scrollback.Enqueue("\n----------------------------------\n");
                MSCUnloader.consoleText = null;
                while (scrollback.Count >= scrollbackSize) scrollback.Dequeue();
            }
        }

        public void RegisterCommand(string command, CommandHandler handler, string help, bool inHelp = true) =>
            commands.Add(command, new CommandRegistration(command, handler, help, inHelp));

        public void RegisterCommand(string command, CommandHandler handler, string help, string alias, bool inHelp = true)
        {
            CommandRegistration cmd = new CommandRegistration(command, handler, help, inHelp);
            commands.Add(command, cmd);
            commands.Add(alias, cmd);
        }

        void ClearConsole(string[] args)
        {
            scrollback.Clear();
            log = scrollback.ToArray();
            LogChanged(log);
        }

        public void AppendLogLine(string line)
        {
            if (scrollback.Count >= scrollbackSize) scrollback.Dequeue();
            scrollback.Enqueue(line);

            log = scrollback.ToArray();
            LogChanged?.Invoke(log);
        }

        public void RunCommandString(string commandString)
        {
            if (!string.IsNullOrEmpty(commandString))
            {
                AppendLogLine(string.Format("{1}<b><color=orange>></color></b> {0}", commandString, Environment.NewLine));

                string[] commandSplit = ParseArguments(commandString);
                string[] args = new string[0];
                if (commandSplit.Length < 1)
                {
                    AppendLogLine(string.Format("<color=red>Unable to process command:</color> <b>{0}</b>", commandString));
                    return;
                }
                else if (commandSplit.Length >= 2)
                {
                    int numArgs = commandSplit.Length - 1;
                    args = new string[numArgs];
                    Array.Copy(commandSplit, 1, args, 0, numArgs);
                }
                RunCommand(commandSplit[0].ToLower(), args);
                commandHistory.Add(commandString);
            }
        }

        void RunCommand(string command, string[] args)
        {
            if (!string.IsNullOrEmpty(command))
            {
                if (!commands.TryGetValue(command, out CommandRegistration reg))
                    AppendLogLine(string.Format("Unknown command <b><color=red>{0}</color></b>, type <color=lime><b>help</b></color> for list.", command));
                else
                {
                    if (reg.handler == null)
                        AppendLogLine(string.Format("<color=red>Unable to process command:</color> <b>{0}</b>, <color=red>handler was null.</color>", command));
                    else
                        reg.handler(args);
                }
            }
        }

        static string[] ParseArguments(string commandString)
        {
            LinkedList<char> parmChars = new LinkedList<char>(commandString.ToCharArray());
            bool inQuote = false;
            LinkedListNode<char> node = parmChars.First;
            while (node != null)
            {
                LinkedListNode<char> next = node.Next;
                if (node.Value == '"')
                {
                    inQuote = !inQuote;
                    parmChars.Remove(node);
                }
                if (!inQuote && node.Value == ' ') node.Value = '\n';
                node = next;
            }
            char[] parmCharsArr = new char[parmChars.Count];
            parmChars.CopyTo(parmCharsArr, 0);
            return new string(parmCharsArr).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        void HelpCommand(string[] args)
        {
            //ModConsole.Print("<color=green><b>Available commands:</b></color>");
            foreach (CommandRegistration reg in commands.Values.GroupBy(x => x.command).Select(g => g.First()).Distinct().ToList().Where(reg => reg.showInHelp))
                AppendLogLine(string.Format("<color=orange><b>{0}</b></color>: {1}", reg.command, reg.help));
        }
    }

    public abstract class ConsoleCommand
    {
        internal static ConsoleController cc;
        public abstract string Name { get; }
        public abstract string Help { get; }
        public virtual bool ShowInHelp => true;
        public abstract void Run(string[] args);

        public static void Add(ConsoleCommand cmd) => cc.RegisterCommand(cmd.Name.ToLower(), cmd.Run, cmd.Help, cmd.ShowInHelp);
    }
}
#pragma warning restore IDE1006 // Naming Styles
