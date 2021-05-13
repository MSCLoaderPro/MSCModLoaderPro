using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

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
                        if (!IsUserAdministrator())
                        {
                            // Restart as an admin, if no admin right has been given
                            Process p = new Process();
                            p.StartInfo.FileName = Assembly.GetEntryAssembly().Location;
                            p.StartInfo.Arguments = string.Join(" ", args);
                            p.StartInfo.Verb = "runas";
                            p.Start();
                            Environment.Exit(0);
                            return;
                        }

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

        public static bool IsUserAdministrator()
        {
            //bool value to hold our return value
            bool isAdmin;
            try
            {
                //get the currently logged in user
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch (UnauthorizedAccessException ex)
            {
                isAdmin = false;
            }
            catch (Exception ex)
            {
                isAdmin = false;
            }
            return isAdmin;
        }
    }
}
