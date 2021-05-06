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

namespace Uninstaller
{
    public partial class Form1 : Form
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

        string MscPath = Application.StartupPath;

        public Form1()
        {
            InitializeComponent();

            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labVer.Text = version.Major + "." + version.Minor;
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

            btnExit.ForeColor = Color.Red;

#if !DEBUG
            if (!File.Exists(Path.Combine("mysummercar.exe")))
            {
                panel3.Visible = false;
                chkDebugger.Visible = false;
                labQuestion.Text = "Uninstaller is not in MSC folder.\nPlease move it to MSC folder first!";
                labQuestion.SetToCenter(this);
            }
#endif
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
            Environment.Exit(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        const string UninstallGuid = "{ef4c06bc-ec46-4bbb-9250-6fc5a25323bf}";
        bool uninstalled;

        private void button1_Click(object sender, EventArgs e)
        {
            string modsFolder = GetModFolderPath();
            DeleteIfExists(Path.Combine(MscPath, "winhttp.dll"));
            DeleteIfExists(Path.Combine(MscPath, "ModLoaderSettings.ini"));
            DeleteIfExists(Path.Combine(MscPath, "doorstop_config.ini"));
            DeleteIfExists(Path.Combine(MscPath, "mysummercar_Data/Managed/MSCLoader.dll"));
            DeleteIfExists(Path.Combine(MscPath, "mysummercar_Data/Managed/MSCLoader.Features.dll"));
            DeleteIfExists(Path.Combine(MscPath, "mysummercar_Data/Managed/MSCLoader.xml"));
            DeleteIfExists(Path.Combine(MscPath, "mysummercar_Data/Managed/Newtonsoft.Json.dll"));
            DeleteIfExists(Path.Combine(MscPath, "mysummercar_Data/Managed/NAudio.Flac.dll"));

            DeleteDirectoryIfExists(Path.Combine(MscPath, "ModUpdater"));
            if (chkDebugger.Checked)
            {
                DeleteDirectoryIfExists(modsFolder);
            }

            panel3.Visible = false;
            btnQuit.Visible = true;
            labQuestion.Text = "Mod Loader Pro has been succesfully removed from your system.";
            labQuestion.SetToCenter(this);
            chkDebugger.Visible = false;

            using (RegistryKey parent = Registry.CurrentUser.OpenSubKey(
                         @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", true))
            {
                try
                {
                    parent.DeleteSubKeyTree(UninstallGuid);
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "An error occurred while trying to remove the uninstaller registry value.",
                        ex);
                }
            }

            uninstalled = true;
        }

        void DeleteIfExists(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }

        void DeleteDirectoryIfExists(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }

        string GetModFolderPath()
        {
            if (!File.Exists(Path.Combine(MscPath, "ModLoaderSettings.ini"))) return "Mods";
            string[] userFile = File.ReadAllText(Path.Combine(MscPath, "ModLoaderSettings.ini")).Split('\n');
            foreach (var s in userFile)
            {
                if (s.StartsWith("ModsFolderPath="))
                {
                    return s.Split('=')[1].Trim();
                }
            }

            return "Mods";
        }

        private void btnQuit_Click(object sender, EventArgs e)
        {
            if (uninstalled)
            {
                Process p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C \"\" del Uninstaller.exe",
                        WorkingDirectory = Directory.GetCurrentDirectory(),
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                p.Start();
            }

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
    }
}
