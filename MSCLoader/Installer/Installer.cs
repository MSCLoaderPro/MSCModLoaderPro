using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading;

namespace Installer
{
    public partial class Installer : Form
    {
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

        string mscPath;
        public string MscPath => mscPath;

        static Installer instance;
        public static Installer Instance => instance;

        Downloader downloader;

        public Installer(bool fastInstall, string mscPath)
        {
            InitializeComponent();

            instance = this;

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labVer.Text = "RC-" + version.Major + "." + version.Minor;
            if (version.Build != 0)
            {
                labVer.Text += "." + version.Build;
            }

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
                control.Font = myFont;
                (control as TextBox).BackColor = colorBtn;
                (control as TextBox).ForeColor = Color.White;
                (control as TextBox).BorderStyle = BorderStyle.None;
            }

            foreach (Control control in GetAllControls(this, typeof(ProgressBar)))
            {
                (control as ProgressBar).ForeColor = Color.Yellow;
                control.BackColor = colorBtn;
                control.SetToCenter(this);
                (control as ProgressBar).Style = ProgressBarStyle.Continuous;
            }

            foreach (Control control in GetAllControls(this, typeof(CheckBox)))
            {
                control.Font = myFont;
                control.BackColor = Color.Transparent;
                (control as CheckBox).ForeColor = Color.White;
                control.SetToCenter(this);
            }

            tabs.Appearance = TabAppearance.FlatButtons;
            tabs.ItemSize = new Size(0, 1);
            tabs.SizeMode = TabSizeMode.Fixed;
            tabs.Left = -1;
            tabs.Top = -2;
            tabs.Width = tabs.Parent.Width + 1;
            tabs.Height = tabs.Parent.Height + 2;

            foreach (TabPage theTab in tabs.TabPages)
            {
                theTab.BackColor = this.BackColor;
                theTab.BorderStyle = BorderStyle.None;
            }

            panelPath.SetToCenter(this);
            btnBrowse.Height = txtboxPath.Height - 7;
            btnDownload.SetToCenter(this);

            labelBadMessage.ForeColor = Color.Red;

            btnExit.ForeColor = Color.Red;
            btnClose.Click += btnExit_Click;

            Font smallFont = new Font(myFont.FontFamily, 12, myFont.Style);
            labWarning.Font = smallFont;
            labWarning.SetToCenter(this);

            txtboxPath.ShortcutsEnabled = false;
            txtModsFolderName.ShortcutsEnabled = false;

            btnDevmenu.SetToCenter(this);
            btnInstallDev.SetToCenter(this);

            if (fastInstall)
            {
                this.mscPath = mscPath;
                DoFastInstall();
            }
            else
            {
                // Get MSC Path;
                mscPath = CustomExtensions.GetMSCPath();
                if (!string.IsNullOrEmpty(mscPath))
                {
                    SetBadMessage("My Summer Car folder found automatically!", Color.LightGreen);
                    this.mscPath = mscPath;
                    txtboxPath.Text = mscPath;
                    btnDownload.Enabled = true;
                }
            }
        }

        async void DoFastInstall()
        {
            btnExit.Enabled = false;
            tabs.SelectedIndex++;
            UpdateStatus(0, "Please wait...");
            await Task.Run(() => Thread.Sleep(500));
            Process[] mscProcess = Process.GetProcessesByName("mysummercar");
            if (mscProcess.Length > 0)
            {
                await Task.Run(() =>
                {
                    foreach (Process process in mscProcess)
                    {
                        process.Kill();
                    }
                });
            }

            // A small workaround for "not accessible files".
            await Task.Run(() => Thread.Sleep(1000));
            downloader = new Downloader();
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

        void TransparetBackground(Control C)
        {
            C.Visible = false;

            C.Refresh();
            Application.DoEvents();

            Rectangle screenRectangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRectangle.Top - this.Top;
            int Right = screenRectangle.Left - this.Left;

            Bitmap bmp = new Bitmap(this.Width, this.Height);
            this.DrawToBitmap(bmp, new Rectangle(0, 0, this.Width, this.Height));
            Bitmap bmpImage = new Bitmap(bmp);
            bmp = bmpImage.Clone(new Rectangle(C.Location.X + Right, C.Location.Y + titleHeight, C.Width, C.Height), bmpImage.PixelFormat);
            C.BackgroundImage = bmp;

            C.Visible = true;
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

        public IEnumerable<Control> GetAllControls(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAllControls(ctrl, type))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == type);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            if (downloader != null && downloader.DownloadFinished)
            {
                CreateFolders();
            }
            Environment.Exit(0);
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    if (string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        SetBadMessage("Path is empty!", Color.Red);
                        btnDownload.Enabled = false;
                        return;
                    }

                    if (!File.Exists(Path.Combine(fbd.SelectedPath, "mysummercar.exe")))
                    {
                        SetBadMessage("Not a My Summer Car folder.", Color.Red);
                        btnDownload.Enabled = false;
                        return;
                    }

                    SetBadMessage("My Summer Car folder found!", Color.LightGreen);
                    mscPath = fbd.SelectedPath;
                    txtboxPath.Text = mscPath;
                    btnDownload.Enabled = true;
                }
            }
        }

        void SetBadMessage(string text, Color color)
        {
            labelBadMessage.Text = text;
            labelBadMessage.SetToCenter(this);
            labelBadMessage.ForeColor = color;
            labelBadMessage.Visible = true;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (Process.GetProcessesByName("mysummercar").Length > 0)
            {
                labelBadMessage.Text = "Close My Summer Car first!";
                labelBadMessage.ForeColor = Color.Red;
                labelBadMessage.SetToCenter(this);
                return;
            }

            tabs.SelectedIndex++;
            downloader = new Downloader();
            btnExit.Enabled = false;
        }

        internal void UpdateStatus(int progressBarValue, string status)
        {
            progressBar.Value = progressBarValue;
            labelStatus.Text = status;
            labelStatus.SetToCenter(this);
        }

        internal void TabEnd()
        {
            tabs.SelectedTab = tabPage3;
            btnExit.Enabled = true;

            string[] userFile = File.ReadAllText(Path.Combine(MscPath, "ModLoaderSettings.ini")).Split('\n');
            foreach (var s in userFile)
            {
                if (s.StartsWith("ModsFolderPath="))
                {
                    txtModsFolderName.Text = s.Split('=')[1].Trim() ;
                }
            }
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            CreateFolders();
            string args = "/C start \"\" \"steam://rungameid/516750\"";
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                args = $"/C start \"\" \"{Path.Combine(MscPath, "mysummercar.exe")}\"";
            }

            Process cmd = new Process();
            cmd.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                WorkingDirectory = MscPath,
                CreateNoWindow = true,
                UseShellExecute = false
            };
            cmd.Start();

            Environment.Exit(0);
        }

        private void btnWebsite_Click(object sender, EventArgs e)
        {
            Process.Start("https://mscloaderpro.github.io/docs/");
        }

        void CreateFolders()
        {
            string modsPath = Path.Combine(MscPath, UserModsFolderName());
            if (!Directory.Exists(modsPath))
            {
                Directory.CreateDirectory(modsPath);
                Directory.CreateDirectory(Path.Combine(modsPath, "Assets"));
                Directory.CreateDirectory(Path.Combine(modsPath, "References"));
                Directory.CreateDirectory(Path.Combine(modsPath, "Settings"));

                string configFile = Path.Combine(MscPath, "ModLoaderSettings.ini");
                string input = File.ReadAllText(configFile);
                input = input.Replace("ModsFolderPath=Mods", "ModsFolderPath=" + txtModsFolderName.Text);
                File.WriteAllText(configFile, input);
            }
        }

        internal void SetVersionString(string s)
        {
            labVersionInfo.Text = $"Now downloading version: {s}";
            labVersionInfo.SetToCenter(this);
            labVersionInfo.Visible = true;
        }

        private void btnDevmenu_Click(object sender, EventArgs e)
        {
            tabs.SelectedIndex++;
            btnDevmenu.Visible = false;
            txtModsFolderName.ReadOnly = true; // just in case.
        }

        int x = 0;
        private void labVer_Click(object sender, EventArgs e)
        {
            x++;
            if (x > 5)
            {
                title.Text = "MADE BY ATHLON";
                title.SetToCenter(this);
            }
        }

        internal bool InstallVSTemplate()
        {
            return chkVSTemplate.Checked;
        }

        internal bool InstallUnityTemplate()
        {
            return chkUnityTemplate.Checked;
        }

        internal bool InstallDebugger()
        {
            return chkDebugger.Checked;
        }

        private void btnInstallDev_Click(object sender, EventArgs e)
        {
            btnExit.Enabled = false;
            tabs.SelectedTab = tabPage2;
            downloader.DownloadDevTools();
        }

        internal string UserModsFolderName()
        {
            return txtModsFolderName.Text;
        }
    }

    public static class CustomExtensions
    { 
        public static void SetToCenter(this Control control, Form form)
        {
            int x = form.Width / 2 - control.Width / 2 - 12;
            control.Location = new Point(x, control.Location.Y);
        }

        /// <summary>
        /// Tries to find My Summer Car folder.
        /// </summary>
        /// <returns></returns>
        public static string GetMSCPath()
        {
            // We're trying to find it in Steam root folder
            string steamFolder = "";
            if (Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam") != null)
            {
                using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Valve\Steam"))
                {
                    steamFolder = Key.GetValue("SteamPath").ToString();
                    steamFolder = steamFolder.Replace("/", "\\");
                }
            }

            // Check only if steamFolder is not empty
            if (steamFolder != "")
            {
                // MSC is installed in root Steam folder
                if (File.Exists($"{steamFolder}\\steamapps\\common\\My Summer Car\\mysummercar.exe"))
                {
                    return $"{steamFolder}\\steamapps\\common\\My Summer Car";
                }

                // MSC not found - gotta open config.vdf file and browse all libraries for MSC folder...
                // Dumping config.vdf to string array
                string[] config = File.ReadAllText($"{steamFolder}\\config\\config.vdf").Split('\n');
                // Creating list in which all BaseInstallFolder values will be stored
                foreach (string line in config)
                {
                    if (line.Contains("BaseInstallFolder"))
                    {
                        string path = line.Substring(line.LastIndexOf('\t')).Replace("\"", "").Replace("\\\\", "\\").Trim();
                        path += "\\steamapps\\common\\My Summer Car";
                        if (Directory.Exists(path))
                        {
                            return path;
                        }
                    }
                }
            }

            // Still haven't found? User will be asked to select it manually. Return null
            return null;
        }

        public static bool IsAnyNullOrEmpty(params string[] strings)
        {
            foreach (string s in strings)
            {
                if (s == "")
                    return true;
            }

            return false;
        }

        public static string ConvertToUrl(this string s)
        {
            string[] link = s.Split(':');
            return (link[1] + ":" + link[2]).Replace("\"", "").Replace("}", "").Replace(",", "").Trim();
        }
    }
}
