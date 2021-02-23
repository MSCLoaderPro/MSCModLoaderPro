using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Ionic.Zip;
using System.Threading.Tasks;

namespace CoolUpdater
{
    public partial class UpdateView : Form
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public UpdateView()
        {
            InitializeComponent();

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        private void UpdateView_Shown(object sender, EventArgs e)
        {
            UpdateAll();
        }

        string GetSeconds(Stopwatch s)
        {
            return Math.Round(s.Elapsed.TotalSeconds, 2).ToString() + "s";
        }

        private async void UpdateAll()
        {
            ToggleButtons(false);
            try
            {
                var stopwatch = Stopwatch.StartNew();
                Log($"({GetSeconds(stopwatch)}) Mod Loader Pro Auto-Update Tool Initialized!\n");

                await Task.Run(() => Thread.Sleep(500));

                Process[] mscProcess = Process.GetProcessesByName("mysummercar");
                if (mscProcess.Length > 0)
                {
                    await Task.Run(() =>
                    {
                        foreach (Process process in mscProcess)
                        {
                            Log($"({GetSeconds(stopwatch)}) Killing process: {process.ProcessName}...");
                            process.Kill();
                        }
                        Log($"({GetSeconds(stopwatch)}) MSC process(es) have been killed!\n");
                    });
                }

                if (!Directory.Exists(Program.Downloads))
                {
                    throw new DirectoryNotFoundException("Downloads folder doesn't exist!");
                }

                // A small workaround for "not accessible files".
                Log($"({GetSeconds(stopwatch)}) Loading update procedure...\n");
                await Task.Run(() => Thread.Sleep(1000));

                string modsFolder = @"..\Mods";

                DirectoryInfo di = new DirectoryInfo(Program.Downloads);
                FileInfo[] files = di.GetFiles("*.zip");

                foreach (var f in files)
                {
                    modsList.Items.Add(f.Name.Split('.')[0]);
                }

                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i];
                    try
                    {
                        await Task.Run(() =>
                        {
                            Log($"({GetSeconds(stopwatch)}) ({i + 1}/{files.Length}) Unpacking {file.Name}...");
                            using (ZipFile zip = ZipFile.Read(file.FullName))
                            {
                                zip.ExtractAll(modsFolder, ExtractExistingFileAction.OverwriteSilently);
                            }
                            Log($"({GetSeconds(stopwatch)}) {file.Name} unpacking completed!\n");
                            modsList.SetItemChecked(i, true);
                        });
                    }
                    catch (Exception ex)
                    {
                        Log($"\n===============================================\nAn error has occured while extracting {file.Name} :(\n\n" + ex.ToString() + "\n\n");
                    }

                    updateProgress.Value = (int)(((double)i + 1) / files.Length * 100);
                }

                // Cleanup after install.
                Directory.Delete(Program.Downloads, true);

                Log($"({GetSeconds(stopwatch)}) All mods have been updated, have a nice day :)");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            ToggleButtons(true);
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            // Restart via .exe
            string args = "/C start \"\" \"steam://rungameid/516750\"";
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                args = "/C start \"\" \"..\\mysummercar.exe\"";
            }

            Log($"Restarting game now using Steam");
            Process cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args
            };
            cmd.Start();

            Environment.Exit(0);
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        void ToggleButtons(bool enabled)
        {
            btnQuit.Enabled = enabled;
            btnStartGame.Enabled = enabled;
        }

        private void UpdateView_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!btnQuit.Enabled)
            {
                e.Cancel = true;
            }
        }

        void Log(string s)
        {
            logBox.Text += s + Environment.NewLine;
        }
    }
}
