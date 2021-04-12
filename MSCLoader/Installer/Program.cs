using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Installer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Modes mode = Modes.Regular;
            string arg = "";

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "fast-install":
                        // Check if missing path to MSC.
                        if (args.Length < 2)
                        {
                            MessageBox.Show("Missing path to MSC!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                            return;
                        }

                        mode = Modes.FastInstall;
                        arg = args[1].Replace("%20", " ");
                        break;
                    case "offline-install":                      
                        string modloaderzip = System.IO.Directory.GetFiles(Application.StartupPath).FirstOrDefault(f => f.Contains("MSCModLoaderPro") && f.EndsWith(".zip"));
                        if (string.IsNullOrEmpty(modloaderzip))
                        {
                            MessageBox.Show("No MSC Mod Loader Pro installation archive found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            Environment.Exit(0);
                            return;
                        }

                        mode = Modes.OfflineInstall;
                        arg = modloaderzip;
                        break;
                }
            }

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Installer(mode, arg));
        }
    }
}
