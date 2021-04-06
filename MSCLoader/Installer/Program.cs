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
            bool fastInstall = false;
            string mscPath = "";

            if (args.Length > 0 && args[0] == "fast-install")
            {
                // Check if missing path to MSC.
                if (args.Length < 2)
                {
                    MessageBox.Show("Missing path!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                    return;
                }

                fastInstall = true;
                mscPath = args[1].Replace("%20", " ");
            }

            //Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Installer(fastInstall, mscPath));
        }
    }
}
