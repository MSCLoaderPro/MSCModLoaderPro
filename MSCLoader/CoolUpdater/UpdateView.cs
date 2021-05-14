using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace CoolUpdater
{
    public partial class UpdateView : Form
    {
        #region Style
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont,
           IntPtr pdv, [In] ref uint pcFonts);

        private PrivateFontCollection fonts = new PrivateFontCollection();

        Font myFont;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        readonly Color colorBtn = Color.FromArgb(255, 199, 152, 129);
        #endregion

        string modsPath, mscPath;
        readonly string[] SupportedArchives = { ".zip", ".rar", ".7z", ".dll" };

        public UpdateView(string modsPath, string mscPath)
        {
            InitializeComponent();
            this.modsPath = modsPath;
            this.mscPath = mscPath;
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            byte[] fontData = Properties.Resources.FugazOne_Regular;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.FugazOne_Regular.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.FugazOne_Regular.Length, IntPtr.Zero, ref dummy);
            Marshal.FreeCoTaskMem(fontPtr);

            myFont = new Font(fonts.Families[0], 16.0F);

            title.Font = myFont;
            title.SetToCenter(this);
            title.BackColor = Color.Transparent;
            title.MouseMove += DragWindowByThis;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labVer.Text = version.Major + "." + version.Minor;
            if (version.Build != 0)
            {
                labVer.Text += "." + version.Build;
            }

            panel1.Paint += WhiteBorder;
            panel1.MouseMove += DragWindowByThis;

            panel2.Paint += WhiteBorder;

            foreach (Control control in GetAllControls(this, typeof(Button)))
            {
                control.Font = myFont;
                (control as Button).TextAlign = ContentAlignment.MiddleCenter;
                (control as Button).BackColor = colorBtn;
                (control as Button).ForeColor = Color.Yellow;
                (control as Button).UseCompatibleTextRendering = true;
                (control as Button).FlatStyle = FlatStyle.Flat;
                (control as Button).FlatAppearance.BorderSize = 0;
            }

            foreach (Control control in GetAllControls(this, typeof(Label)))
            {
                control.Font = myFont;
                control.BackColor = Color.Transparent;
                (control as Label).ForeColor = Color.White;
                if (control.Name == "labVer") continue;
                control.SetToCenter(this);
                (control as Label).TextAlign = ContentAlignment.MiddleCenter;
            }

            foreach (Control control in GetAllControls(this, typeof(TextBox)))
            {
                (control as TextBox).BackColor = colorBtn;
                (control as TextBox).ForeColor = Color.White;
                (control as TextBox).BorderStyle = BorderStyle.None;
            }

            foreach (Control control in GetAllControls(this, typeof(ProgressBar)))
            {
                (control as ProgressBar).ForeColor = Color.Yellow;
                control.BackColor = colorBtn;
                //control.SetToCenter(this);
                (control as ProgressBar).Style = ProgressBarStyle.Continuous;
            }

            foreach (Control control in GetAllControls(this, typeof(CheckedListBox)))
            {
                (control as CheckedListBox).ForeColor = Color.White;
                control.BackColor = colorBtn;
                //control.SetToCenter(this);
            }

            btnQuit.SetToCenter(this);
            btnStartGame.SetToCenter(logBox);
            btnExit.Click += btnQuit_Click;
            btnExit.ForeColor = Color.Red;

            Font smallFont = new Font(myFont.FontFamily, 12, myFont.Style);
            btnNoSteam.Font = smallFont;
            btnWebsite.Font = smallFont;
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

                DirectoryInfo di = new DirectoryInfo(Program.Downloads);
                FileInfo[] archives = di.GetFiles("*.*").Where(f => SupportedArchives.Contains(f.Extension)).ToArray();

                // Populate mod list.
                foreach (var f in archives)
                {
                    modsList.Items.Add(f.Name.Split('.')[0]);
                }

                bool hasFailed = false;
                Directory.CreateDirectory("Temp");
                for (int i = 0; i < archives.Length; i++)
                {
                    FileInfo archive = archives[i];

                    // Check if a loose library file.
                    if (archive.Extension == ".dll")
                    {
                        // If so, move it directly into mods folder and carry on.
                        File.Copy(archive.FullName, Path.Combine(modsPath, archive.Name), true);
                    }
                    else
                    {
                        string tempPath = Path.Combine("Temp", archive.Name).Replace(archive.Extension, "");
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                        }

                        Directory.CreateDirectory(tempPath);

                        try
                        {
                            Log($"({GetSeconds(stopwatch)}) ({i + 1}/{archive.Length}) Unpacking {archive.Name}...");
                            await Task.Run(() =>
                            {
                                using (IArchive iArchive = ArchiveFactory.Open(archive.FullName))
                                {
                                    foreach (var entry in iArchive.Entries)
                                    {
                                        if (!entry.IsDirectory)
                                        {
                                            Console.WriteLine(entry.Key);
                                            entry.WriteToDirectory(tempPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                                        }
                                    }
                                }
                            });

                            // Now we are reading all files inside of that new folder.
                            foreach (string filePath in Directory.GetFiles(tempPath, "*.*", SearchOption.AllDirectories))
                            {
                                FileInfo file = new FileInfo(filePath);
                                string destination = Path.Combine(modsPath, file.Name);
                                string pathInStructure = filePath.Replace(tempPath, "");

                                // Move DLLs that are in References to References folder
                                if (file.Extension == ".dll" && pathInStructure.Contains("References"))
                                {
                                    destination = Path.Combine(destination, "References", file.Name);
                                }

                                // EXEs to main folder of the game.
                                if (file.Extension == ".exe" && !pathInStructure.ToLower().Contains("assets"))
                                {
                                    destination = Path.Combine(mscPath, file.Name);
                                }

                                if (!pathInStructure.ToLower().Contains("assets"))
                                {
                                    #region Outside of Mods folder
                                    // This file likely goes to Images folder.
                                    if (pathInStructure.ToLower().Contains("images"))
                                    {
                                        destination = GetSubPath(file, pathInStructure, "Images");
                                    }

                                    if (pathInStructure.ToLower().Contains("radio"))
                                    {
                                        destination = GetSubPath(file, pathInStructure, "Radio");
                                    }

                                    if (pathInStructure.ToLower().Contains("cd1"))
                                    {
                                        destination = GetSubPath(file, pathInStructure, "CD1");
                                    }

                                    if (pathInStructure.ToLower().Contains("cd2"))
                                    {
                                        destination = GetSubPath(file, pathInStructure, "CD2");
                                    }

                                    if (pathInStructure.ToLower().Contains("cd3"))
                                    {
                                        destination = GetSubPath(file, pathInStructure, "CD3");
                                    }
                                    #endregion
                                }

                                if (pathInStructure.ToLower().Contains("assets"))
                                {
                                    string assetsFolderName = file.Directory.Name;
                                    string folderAssetsPath = Path.Combine(modsPath, "Assets", assetsFolderName);
                                    if (!Directory.Exists(folderAssetsPath))
                                    {
                                        Directory.CreateDirectory(folderAssetsPath);
                                    }

                                    destination = Path.Combine(folderAssetsPath, file.Name);
                                }

                                if (pathInStructure.ToLower().Contains("settings"))
                                {
                                    string folderSettingsPath = Path.Combine(modsPath, "Settings", file.Directory.Name);
                                    if (!Directory.Exists(folderSettingsPath))
                                    {
                                        Directory.CreateDirectory(folderSettingsPath);
                                    }

                                    destination = Path.Combine(folderSettingsPath, file.Name);
                                }

                                File.Copy(file.FullName, destination, true);
                            }

                            modsList.SetItemChecked(i, true);
                            Log($"({GetSeconds(stopwatch)}) {archive.Name} unpacking completed!\n");
                        }
                        catch (Exception ex)
                        {
                            Log($"\n===============================================\nAn error has occured while extracting {archive.Name} :(\n\n" + ex.ToString() + "\n\n");
                            hasFailed = true;
                        }
                    }

                    updateProgress.Value = (int)(((double)i + 1) / archive.Length * 100);
                }

                // Cleanup after install.
                if (!hasFailed)
                {
                    Directory.Delete(Program.Downloads, true);
                    Directory.Delete(Program.Temp, true);
                }

                Log($"({GetSeconds(stopwatch)}) All mods have been updated, have a nice day :)");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
            ToggleButtons(true);
        }

        string GetSubPath(FileInfo file, string pathInStructure, string folder)
        {
            string subFolder = pathInStructure.Replace($"{folder}\\", "").Replace(file.Name, "");
            string path = Path.Combine(mscPath, folder, subFolder);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return Path.Combine(path, file.Name);
        }

        private void btnStartGame_Click(object sender, EventArgs e)
        {
            Log($"Restarting game now using Steam");
            Process cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C start \"\" \"steam://rungameid/516750\"",
                UseShellExecute = false,
                CreateNoWindow = true
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
            btnExit.Enabled = enabled;
            btnNoSteam.Enabled = enabled;
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

        public IEnumerable<Control> GetAllControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private void DragWindowByThis(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void WhiteBorder(object sender, PaintEventArgs e)
        {
            int thickness = 3;
            int halfThickness = thickness / 2;
            using (Pen p = new Pen(Color.White, thickness))
            {
                e.Graphics.DrawRectangle(p, new Rectangle(halfThickness,
                                                          halfThickness,
                                                          (sender as Control).ClientSize.Width - thickness,
                                                          (sender as Control).ClientSize.Height - thickness));
            }
        }
        
        private const int CS_DROPSHADOW = 0x20000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("https://mscloaderpro.github.io/docs/");
        }

        private void btnNoSteam_Click(object sender, EventArgs e)
        {
            Log($"Restarting game now using Steam");
            Process cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/C start \"\" \"..\\mysummercar.exe\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            cmd.Start();

            Environment.Exit(0);
        }
    }

    public static class CustomExtensions
    {
        public static void SetToCenter(this Control control, Form form)
        {
            int x = form.Width / 2 - control.Width / 2 - 12;
            control.Location = new Point(x, control.Location.Y);
        }

        public static void SetToCenter(this Control control, Control other)
        {
            int x = (other.Left + other.Width / 2) - control.Width / 2;
            control.Location = new Point(x, control.Location.Y);
        }
    }
}
